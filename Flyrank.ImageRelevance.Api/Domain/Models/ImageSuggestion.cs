namespace Flyrank.ImageRelevance.Api.Domain.Models;

public sealed record ImageSuggestion(
    Guid PostId,
    Guid ImageId,
    double Score,
    bool IsApproved,
    string? RejectionReason);
