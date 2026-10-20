using System.Collections.Concurrent;
using Flyrank.ImageRelevance.Api.Domain.Models;

namespace Flyrank.ImageRelevance.Api.Data;

public sealed class InMemoryImageRelevanceRepository : IImageRelevanceRepository
{
    private readonly ConcurrentDictionary<Guid, ImageMetadata> _images = new();
    private readonly ConcurrentDictionary<Guid, Post> _posts = new();
    private readonly ConcurrentDictionary<(Guid PostId, Guid ImageId), ImageSuggestion> _suggestions = new();

    public void AddImage(ImageMetadata image)
    {
        if (!_images.TryAdd(image.Id, image))
            throw new InvalidOperationException("An image with this identifier already exists.");
    }

    public ImageMetadata? GetImage(Guid imageId) =>
        _images.TryGetValue(imageId, out var image) ? image : null;

    public IReadOnlyList<Post> GetPosts() => _posts.Values.ToArray();

    public Post? GetPost(Guid postId) =>
        _posts.TryGetValue(postId, out var post) ? post : null;

    public void AddPost(Post post)
    {
        if (!_posts.TryAdd(post.Id, post))
            throw new InvalidOperationException("A post with this identifier already exists.");
    }

    public IReadOnlyList<ImageSuggestion> GetSuggestionsForPost(Guid postId) =>
        _suggestions.Values.Where(suggestion => suggestion.PostId == postId).ToArray();

    public ImageSuggestion? GetSuggestion(Guid postId, Guid imageId) =>
        _suggestions.TryGetValue((postId, imageId), out var suggestion) ? suggestion : null;

    public void SaveSuggestion(ImageSuggestion suggestion) =>
        _suggestions[(suggestion.PostId, suggestion.ImageId)] = suggestion;
}
