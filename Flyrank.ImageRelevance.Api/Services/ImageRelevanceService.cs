using Flyrank.ImageRelevance.Api.Contracts;
using Flyrank.ImageRelevance.Api.Data;
using Flyrank.ImageRelevance.Api.Domain.Models;

namespace Flyrank.ImageRelevance.Api.Services;

public sealed class ImageRelevanceService(
    IImageRelevanceRepository repository,
    IImageAnalysisService imageAnalysisService,
    IEmbeddingService embeddingService,
    IMismatchGuard mismatchGuard) : IImageRelevanceService
{
    private const int MaximumImageBytes = 10 * 1024 * 1024;

    public async Task<ProcessImageResponse> ProcessImageAsync(
        ProcessImageRequest request,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var imageUri) ||
            imageUri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(imageUri.UserInfo))
        {
            throw new ArgumentException("URL slike mora biti validan HTTPS URL bez korisničkih podataka.");
        }

        var mimeType = request.MimeType.Trim().ToLowerInvariant();
        if (mimeType is not ("image/jpeg" or "image/png" or "image/webp"))
            throw new ArgumentException("Podržani formati slika su JPEG, PNG i WebP.");

        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(request.ImageBase64);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("ImageBase64 nije ispravan Base64 sadržaj.", nameof(request), exception);
        }

        if (imageBytes.Length == 0 || imageBytes.Length > MaximumImageBytes)
            throw new ArgumentException("Veličina slike mora biti između 1 bajta i 10 MB.");

        var analysis = await imageAnalysisService.AnalyzeAsync(
            request.ImageBase64,
            mimeType,
            cancellationToken);

        var image = new ImageMetadata(
            Guid.NewGuid(),
            imageUri.ToString(),
            analysis.Subject,
            analysis.Category,
            analysis.Attributes,
            analysis.Caption,
            analysis.Confidence,
            DateTimeOffset.UtcNow);

        var imageText = $"{image.Subject}. {image.Category}. {string.Join(", ", image.Attributes)}. {image.Caption}";
        var imageEmbedding = await embeddingService.CreateEmbeddingAsync(
            imageText,
            "RETRIEVAL_QUERY",
            cancellationToken);

        var suggestions = repository.GetPosts()
            .Select(post =>
            {
                var score = CosineSimilarity.Calculate(imageEmbedding, post.Embedding);
                var rejectionReason = mismatchGuard.GetRejectionReason(image, post);
                return new ImageSuggestion(post.Id, image.Id, score, false, rejectionReason);
            })
            .OrderByDescending(suggestion => suggestion.Score)
            .ToArray();

        repository.AddImage(image);
        foreach (var suggestion in suggestions)
            repository.SaveSuggestion(suggestion);

        return new ProcessImageResponse(
            image.Id,
            image.Subject,
            image.Category,
            image.Attributes,
            image.Caption,
            image.Confidence,
            suggestions.Select(ToResponse).ToArray());
    }

    public RankedImagesResponse? GetImagesForPost(Guid postId)
    {
        var post = repository.GetPost(postId);
        if (post is null)
            return null;

        var images = repository.GetSuggestionsForPost(postId)
            .OrderByDescending(suggestion => suggestion.Score)
            .Select(suggestion =>
            {
                var image = repository.GetImage(suggestion.ImageId);
                return image is null ? null : new RankedImageResponse(
                    image.Id,
                    image.Url,
                    image.Subject,
                    image.Category,
                    image.Caption,
                    image.Confidence,
                    suggestion.Score,
                    suggestion.IsApproved,
                    suggestion.RejectionReason);
            })
            .Where(image => image is not null)
            .Cast<RankedImageResponse>()
            .ToArray();

        var message = images.Length == 0
            ? "Za ovaj post još nema obrađenih slika."
            : images.All(image => image.RejectionReason is not null)
                ? $"Sve pronađene slike su odbijene. Razlog: {images[0].RejectionReason}"
                : "Slike su rangirane po cosine similarity rezultatu.";

        return new RankedImagesResponse(postId, message, images);
    }

    private static ImageSuggestionResponse ToResponse(ImageSuggestion suggestion) =>
        new(
            suggestion.PostId,
            suggestion.ImageId,
            suggestion.Score,
            suggestion.IsApproved,
            suggestion.RejectionReason);
}
