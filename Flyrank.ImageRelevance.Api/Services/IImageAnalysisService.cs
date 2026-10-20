namespace Flyrank.ImageRelevance.Api.Services;

public interface IImageAnalysisService
{
    Task<ImageAnalysisResult> AnalyzeAsync(
        string imageBase64,
        string mimeType,
        CancellationToken cancellationToken);
}

public sealed record ImageAnalysisResult(
    string Subject,
    string Category,
    IReadOnlyList<string> Attributes,
    string Caption,
    double Confidence);
