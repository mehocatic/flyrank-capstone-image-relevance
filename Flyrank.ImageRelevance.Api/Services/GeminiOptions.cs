namespace Flyrank.ImageRelevance.Api.Services;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public string VisionModel { get; set; } = "gemini-2.5-flash";
    public string EmbeddingModel { get; set; } = "gemini-embedding-001";
}
