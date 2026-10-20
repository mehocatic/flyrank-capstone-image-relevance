using Flyrank.ImageRelevance.Api.Contracts;
using Flyrank.ImageRelevance.Api.Data;
using Flyrank.ImageRelevance.Api.Domain.Models;

namespace Flyrank.ImageRelevance.Api.Services;

public sealed class MismatchApprovalException(string message) : Exception(message);

public sealed class ReviewService(IImageRelevanceRepository repository) : IReviewService
{
    public ReviewResponse? Review(ReviewRequest request)
    {
        var suggestion = repository.GetSuggestion(request.PostId, request.ImageId);
        if (suggestion is null)
            return null;

        if (request.IsApproved &&
            suggestion.RejectionReason?.StartsWith("Mismatch Guard:", StringComparison.Ordinal) == true)
        {
            throw new MismatchApprovalException(
                "Preporuka koju je Mismatch Guard odbio ne može biti odobrena.");
        }

        var reason = request.IsApproved
            ? null
            : string.IsNullOrWhiteSpace(request.RejectionReason)
                ? suggestion.RejectionReason ?? "Preporuka je odbijena tokom ručne provjere."
                : request.RejectionReason.Trim();
        var reviewed = suggestion with
        {
            IsApproved = request.IsApproved,
            RejectionReason = reason
        };

        repository.SaveSuggestion(reviewed);
        return new ReviewResponse(
            reviewed.PostId,
            reviewed.ImageId,
            reviewed.Score,
            reviewed.IsApproved,
            reviewed.RejectionReason);
    }
}
