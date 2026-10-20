namespace Flyrank.ImageRelevance.Api.Services;

public sealed class DemoImageAnalysisService : IImageAnalysisService
{
    public Task<ImageAnalysisResult> AnalyzeAsync(
        string imageBase64,
        string mimeType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new ImageAnalysisResult(
            "wolf",
            "wildlife",
            ["wolf", "gray fur", "forest animal"],
            "A gray wolf standing in a forest.",
            0.99));
    }
}
