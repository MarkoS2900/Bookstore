using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using Bookstore.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Api.ExceptionHandling;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            AuthorNotFoundException => (StatusCodes.Status400BadRequest, "Invalid author", exception.Message),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed", exception.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrent modification",
                "The record changed or was deleted while this request was being processed. Reload it and try again."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred",
                "The request could not be completed. Please try again later.")
        };

        if (status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Request {Method} {Path} failed. Trace ID: {TraceId}",
                httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);
        else
            logger.LogWarning("Request {Method} {Path} rejected with status {StatusCode}: {Reason}",
                httpContext.Request.Method, httpContext.Request.Path, status, detail);

        await Results.Problem(
            statusCode: status,
            title: title,
            detail: detail,
            instance: httpContext.Request.Path,
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier
            }).ExecuteAsync(httpContext);

        return true;
    }
}
