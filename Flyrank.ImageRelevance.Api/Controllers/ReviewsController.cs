using Flyrank.ImageRelevance.Api.Contracts;
using Flyrank.ImageRelevance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Flyrank.ImageRelevance.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController(IReviewService reviewService) : ControllerBase
{
    /// <summary>Approves or rejects an image and post suggestion.</summary>
    [HttpPost]
    [ProducesResponseType<ReviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public ActionResult<ReviewResponse> Review([FromBody] ReviewRequest request)
    {
        var response = reviewService.Review(request);
        return response is null ? NotFound() : Ok(response);
    }
}
