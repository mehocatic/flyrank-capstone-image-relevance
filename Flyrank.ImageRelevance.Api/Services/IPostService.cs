using Flyrank.ImageRelevance.Api.Contracts;

namespace Flyrank.ImageRelevance.Api.Services;

public interface IPostService
{
    Task<PostResponse> CreateAsync(CreatePostRequest request, CancellationToken cancellationToken);
}
