using Flyrank.ImageRelevance.Api.Contracts;
using Flyrank.ImageRelevance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Flyrank.ImageRelevance.Api.Controllers;

[ApiController]
[Route("api/posts")]
public sealed class PostsController(
    IPostService postService,
    IImageRelevanceService imageRelevanceService) : ControllerBase
{
    /// <summary>Creates and indexes a post for future semantic image matching.</summary>
    [HttpPost]
    [ProducesResponseType<PostResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PostResponse>> Create(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var response = await postService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetImages), new { id = response.Id }, response);
    }

    /// <summary>Returns image suggestions ranked for the specified post.</summary>
    [HttpGet("{id:guid}/images")]
    [ProducesResponseType<RankedImagesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<RankedImagesResponse> GetImages(Guid id)
    {
        var response = imageRelevanceService.GetImagesForPost(id);
        return response is null ? NotFound() : Ok(response);
    }
}
