using Flyrank.ImageRelevance.Api.Contracts;

namespace Flyrank.ImageRelevance.Api.Services;

public interface IReviewService
{
    ReviewResponse? Review(ReviewRequest request);
}
