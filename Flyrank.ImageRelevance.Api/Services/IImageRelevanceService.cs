using Flyrank.ImageRelevance.Api.Contracts;

namespace Flyrank.ImageRelevance.Api.Services;

public interface IImageRelevanceService
{
    Task<ProcessImageResponse> ProcessImageAsync(
        ProcessImageRequest request,
        CancellationToken cancellationToken);

    RankedImagesResponse? GetImagesForPost(Guid postId);
}
