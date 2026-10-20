namespace Flyrank.ImageRelevance.Api.Services;

public interface IEmbeddingService
{
    Task<IReadOnlyList<float>> CreateEmbeddingAsync(
        string text,
        string taskType,
        CancellationToken cancellationToken);
}
