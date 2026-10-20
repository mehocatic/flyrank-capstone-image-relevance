using System.ComponentModel.DataAnnotations;

namespace Flyrank.ImageRelevance.Api.Contracts;

/// <summary>Content to index as a post for image matching.</summary>
public sealed record CreatePostRequest
{
    /// <summary>Post title.</summary>
    [Required, MinLength(1), MaxLength(200)]
    public required string Title { get; init; }

    /// <summary>Post body text.</summary>
    [Required, MinLength(1), MaxLength(20_000)]
    public required string Content { get; init; }
}

/// <summary>Post data returned by the API.</summary>
public sealed record PostResponse(
    Guid Id,
    string Title,
    string Content,
    DateTimeOffset CreatedAt);

/// <summary>Images ranked for a post, including any mismatch rejection explanations.</summary>
public sealed record RankedImagesResponse(
    Guid PostId,
    string Message,
    IReadOnlyList<RankedImageResponse> Images);

/// <summary>Image metadata and its matching score for a post.</summary>
public sealed record RankedImageResponse(
    Guid ImageId,
    string Url,
    string Subject,
    string Category,
    string Caption,
    double Confidence,
    double Score,
    bool IsApproved,
    string? RejectionReason);
