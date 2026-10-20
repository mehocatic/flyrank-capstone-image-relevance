namespace Flyrank.ImageRelevance.Api.Domain.Models;

public sealed record Post(
    Guid Id,
    string Title,
    string Content,
    IReadOnlyList<float> Embedding,
    DateTimeOffset CreatedAt);
