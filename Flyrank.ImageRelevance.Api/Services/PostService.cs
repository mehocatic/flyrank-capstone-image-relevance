using Flyrank.ImageRelevance.Api.Contracts;
using Flyrank.ImageRelevance.Api.Data;
using Flyrank.ImageRelevance.Api.Domain.Models;

namespace Flyrank.ImageRelevance.Api.Services;

public sealed class PostService(
    IImageRelevanceRepository repository,
    IEmbeddingService embeddingService) : IPostService
{
    public async Task<PostResponse> CreateAsync(
        CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();
        var content = request.Content.Trim();
        if (title.Length == 0 || content.Length == 0)
            throw new ArgumentException("Naslov i sadržaj posta ne mogu biti prazni.");

        var embedding = await embeddingService.CreateEmbeddingAsync(
            $"{title}. {content}",
            "RETRIEVAL_DOCUMENT",
            cancellationToken);
        var post = new Post(
            Guid.NewGuid(),
            title,
            content,
            embedding,
            DateTimeOffset.UtcNow);

        repository.AddPost(post);
        return new PostResponse(post.Id, post.Title, post.Content, post.CreatedAt);
    }
}
