using System.ComponentModel.DataAnnotations;

namespace Flyrank.ImageRelevance.Api.Contracts;

/// <summary>Reviewer decision for a specific image and post pair.</summary>
public sealed record ReviewRequest
{
    /// <summary>Identifier of the post in the candidate pair.</summary>
    [Required]
    public required Guid PostId { get; init; }

    /// <summary>Identifier of the image in the candidate pair.</summary>
    [Required]
    public required Guid ImageId { get; init; }

    /// <summary>True to approve the pair, false to reject it.</summary>
    public required bool IsApproved { get; init; }

    /// <summary>Optional reviewer explanation when rejecting the pair.</summary>
    [MaxLength(500)]
    public string? RejectionReason { get; init; }
}

/// <summary>Stored reviewer decision for an image and post pair.</summary>
public sealed record ReviewResponse(
    Guid PostId,
    Guid ImageId,
    double Score,
    bool IsApproved,
    string? RejectionReason);
