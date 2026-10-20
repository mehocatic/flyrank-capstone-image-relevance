using System.ComponentModel.DataAnnotations;

namespace Flyrank.ImageRelevance.Api.Contracts;

/// <summary>Image URL and image bytes submitted for visual analysis.</summary>
public sealed record ProcessImageRequest
{
    /// <summary>Public HTTPS URL associated with the image.</summary>
    [Required, Url, MaxLength(2048)]
    public required string Url { get; init; }

    /// <summary>Base64-encoded image bytes, without a data URL prefix.</summary>
    [Required, MinLength(1), MaxLength(14_000_000)]
    public required string ImageBase64 { get; init; }

    /// <summary>Image media type. Supported values are image/jpeg, image/png, and image/webp.</summary>
    [Required, MaxLength(40)]
    public required string MimeType { get; init; }
}

/// <summary>Structured image analysis and the resulting post matches.</summary>
public sealed record ProcessImageResponse(
    Guid ImageId,
    string Subject,
    string Category,
    IReadOnlyList<string> Attributes,
    string Caption,
    double Confidence,
    IReadOnlyList<ImageSuggestionResponse> Suggestions);

/// <summary>A post candidate ranked for an analyzed image.</summary>
public sealed record ImageSuggestionResponse(
    Guid PostId,
    Guid ImageId,
    double Score,
    bool IsApproved,
    string? RejectionReason);
