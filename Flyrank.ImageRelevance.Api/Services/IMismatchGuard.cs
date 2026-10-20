using Flyrank.ImageRelevance.Api.Domain.Models;

namespace Flyrank.ImageRelevance.Api.Services;

public interface IMismatchGuard
{
    string? GetRejectionReason(ImageMetadata image, Post post);
}
