namespace Flyrank.ImageRelevance.Api.Services;

public sealed class DemoEmbeddingService : IEmbeddingService
{
    public Task<IReadOnlyList<float>> CreateEmbeddingAsync(
        string text,
        string taskType,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        cancellationToken.ThrowIfCancellationRequested();

        var normalized = text.ToLowerInvariant();
        IReadOnlyList<float> embedding =
            normalized.Contains("fox", StringComparison.Ordinal) ? [1, 0, 0, 0] :
            normalized.Contains("wolf", StringComparison.Ordinal) ? [0, 1, 0, 0] :
            normalized.Contains("cat", StringComparison.Ordinal) ? [0, 0, 1, 0] :
            [0, 0, 0, 1];

        return Task.FromResult(embedding);
    }
}
