using Flyrank.ImageRelevance.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Flyrank.ImageRelevance.Api.Middleware;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Neispravan zahtjev",
                exception.Message),
            MismatchApprovalException => (
                StatusCodes.Status409Conflict,
                "Odluka nije dozvoljena",
                exception.Message),
            HttpRequestException => (
                StatusCodes.Status502BadGateway,
                "Greška eksternog servisa",
                "Gemini servis nije uspješno obradio zahtjev."),
            _ => (0, string.Empty, string.Empty)
        };

        if (status == 0)
            return false;

        logger.LogWarning(exception, "API zahtjev završen statusom {StatusCode}.", status);
        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            },
            Exception = exception
        });
    }
}
