using Flyrank.ImageRelevance.Api.Domain.Models;

namespace Flyrank.ImageRelevance.Api.Data;

public interface IImageRelevanceRepository
{
    void AddImage(ImageMetadata image);
    ImageMetadata? GetImage(Guid imageId);
    IReadOnlyList<Post> GetPosts();
    Post? GetPost(Guid postId);
    void AddPost(Post post);
    IReadOnlyList<ImageSuggestion> GetSuggestionsForPost(Guid postId);
    ImageSuggestion? GetSuggestion(Guid postId, Guid imageId);
    void SaveSuggestion(ImageSuggestion suggestion);
}
