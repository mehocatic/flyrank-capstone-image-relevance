using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Flyrank.ImageRelevance.Api.Services;

public sealed class GeminiApiClient(
    HttpClient httpClient,
    IOptions<GeminiOptions> options) : IImageAnalysisService, IEmbeddingService
{
    private readonly GeminiOptions _options = options.Value;

    public async Task<ImageAnalysisResult> AnalyzeAsync(
        string imageBase64,
        string mimeType,
        CancellationToken cancellationToken)
    {
        var request = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new
                        {
                            text = """
                                Analyze the image for semantic matching to written posts.
                                Return one JSON object with subject, category, attributes, caption, and confidence.
                                Subject must identify the main visible subject as specifically as the image supports.
                                Category must be a short lowercase category. Attributes must be 1 to 12 concise tags.
                                Caption must describe only visible content. Confidence must be a number from 0 to 1.
                                Do not infer facts that are not visually supported.
                                """
                        },
                        new
                        {
                            inlineData = new
                            {
                                mimeType,
                                data = imageBase64
                            }
                        }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        subject = new { type = "STRING" },
                        category = new { type = "STRING" },
                        attributes = new { type = "ARRAY", items = new { type = "STRING" } },
                        caption = new { type = "STRING" },
                        confidence = new { type = "NUMBER" }
                    },
                    required = new[] { "subject", "category", "attributes", "caption", "confidence" }
                },
                temperature = 0.1,
                maxOutputTokens = 512
            }
        };

        using var response = await httpClient.PostAsJsonAsync(
            BuildModelUri(_options.VisionModel, "generateContent"),
            request,
            cancellationToken);
        EnsureSuccess(response);

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        var outputText = ReadGeneratedText(document.RootElement);
        ImageAnalysisResult? result;
        try
        {
            result = JsonSerializer.Deserialize<ImageAnalysisResult>(
                outputText,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Gemini returned invalid JSON for the image analysis.", exception);
        }

        return ValidateAnalysis(result);
    }

    public async Task<IReadOnlyList<float>> CreateEmbeddingAsync(
        string text,
        string taskType,
        CancellationToken cancellationToken)
    {
        var request = new
        {
            content = new
            {
                parts = new[] { new { text } }
            },
            taskType
        };

        using var response = await httpClient.PostAsJsonAsync(
            BuildModelUri(_options.EmbeddingModel, "embedContent"),
            request,
            cancellationToken);
        EnsureSuccess(response);

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("embedding", out var embedding) ||
            !embedding.TryGetProperty("values", out var values) ||
            values.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Gemini returned an embedding response without vector values.");
        }

        var vector = values.EnumerateArray()
            .Select(value => value.GetSingle())
            .ToArray();
        if (vector.Length == 0 || vector.Any(value => !float.IsFinite(value)))
            throw new InvalidOperationException("Gemini returned an empty or invalid embedding vector.");

        return vector;
    }

    private Uri BuildModelUri(string model, string action)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException(
                "Gemini API key is missing. Configure Gemini:ApiKey or the Gemini__ApiKey environment variable.");

        if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var baseUri) ||
            baseUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("Gemini:BaseUrl must be a valid HTTPS URL.");
        }

        return new Uri(
            $"{baseUri.ToString().TrimEnd('/')}/models/{Uri.EscapeDataString(model)}:{action}" +
            $"?key={Uri.EscapeDataString(_options.ApiKey)}");
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var statusCode = (int)response.StatusCode;
        throw new HttpRequestException(
            $"Gemini API request failed with HTTP status {statusCode}.",
            null,
            response.StatusCode);
    }

    private static string ReadGeneratedText(JsonElement root)
    {
        if (!root.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0 ||
            !candidates[0].TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts))
        {
            throw new InvalidOperationException("Gemini returned no image analysis content.");
        }

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var text) &&
                text.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(text.GetString()))
            {
                return text.GetString()!;
            }
        }

        throw new InvalidOperationException("Gemini returned no text in its image analysis response.");
    }

    private static ImageAnalysisResult ValidateAnalysis(ImageAnalysisResult? result)
    {
        if (result is null ||
            string.IsNullOrWhiteSpace(result.Subject) ||
            result.Subject.Length > 120 ||
            string.IsNullOrWhiteSpace(result.Category) ||
            result.Category.Length > 80 ||
            string.IsNullOrWhiteSpace(result.Caption) ||
            result.Caption.Length > 1000 ||
            result.Attributes is null ||
            result.Attributes.Count is < 1 or > 12 ||
            result.Attributes.Any(attribute =>
                string.IsNullOrWhiteSpace(attribute) || attribute.Length > 80) ||
            !double.IsFinite(result.Confidence) ||
            result.Confidence is < 0 or > 1)
        {
            throw new InvalidOperationException(
                "Gemini returned image analysis outside the supported tag or confidence limits.");
        }

        var attributes = result.Attributes
            .Select(attribute => attribute.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (attributes.Length == 0)
            throw new InvalidOperationException("Gemini returned no valid image attributes.");

        return result with
        {
            Subject = result.Subject.Trim(),
            Category = result.Category.Trim().ToLowerInvariant(),
            Attributes = attributes,
            Caption = result.Caption.Trim()
        };
    }
}
