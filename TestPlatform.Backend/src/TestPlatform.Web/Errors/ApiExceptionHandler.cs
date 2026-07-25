using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TestPlatform.Web.Errors;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception while processing {RequestMethod} {RequestPath}. TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        var statusCode = exception is ArgumentOutOfRangeException
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;
        var title = statusCode == StatusCodes.Status400BadRequest
            ? "The request is invalid."
            : "An unexpected server error occurred.";
        var code = statusCode == StatusCodes.Status400BadRequest
            ? "request.invalid"
            : "server.unexpected_error";

        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Extensions =
                {
                    ["code"] = code,
                    ["traceId"] = httpContext.TraceIdentifier,
                },
            },
        });
    }
}
