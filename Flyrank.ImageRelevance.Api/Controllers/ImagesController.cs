using Flyrank.ImageRelevance.Api.Contracts;
using Flyrank.ImageRelevance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Flyrank.ImageRelevance.Api.Controllers;

[ApiController]
[Route("api/images")]
public sealed class ImagesController(IImageRelevanceService imageRelevanceService) : ControllerBase
{
    /// <summary>Analyzes an image with Gemini and ranks it against existing posts.</summary>
    [HttpPost("process")]
    [ProducesResponseType<ProcessImageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ProcessImageResponse>> Process(
        [FromBody] ProcessImageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await imageRelevanceService.ProcessImageAsync(request, cancellationToken);
        return Ok(result);
    }
}
