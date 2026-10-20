namespace Flyrank.ImageRelevance.Api.Domain.Models;

public sealed record ImageMetadata(
    Guid Id,
    string Url,
    string Subject,
    string Category,
    IReadOnlyList<string> Attributes,
    string Caption,
    double Confidence,
    DateTimeOffset CreatedAt);
