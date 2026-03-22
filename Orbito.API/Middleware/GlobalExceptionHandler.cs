using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Orbito.API.Middleware;

/// <summary>
/// Global exception handler for unhandled exceptions
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Handle FluentValidation.ValidationException
        if (exception is ValidationException validationException)
        {
            _logger.LogWarning("Validation exception occurred. Path: {Path}, Method: {Method}, Errors: {Errors}",
                httpContext.Request.Path,
                httpContext.Request.Method,
                string.Join(", ", validationException.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

            var validationErrors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var validationResponse = new
            {
                isSuccess = false,
                message = "Validation failed",
                errors = validationException.Errors.Select(e => e.ErrorMessage).ToList(),
                validationErrors
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(validationResponse, cancellationToken);

            return true;
        }

        _logger.LogError(exception, "Unhandled exception occurred. Path: {Path}, Method: {Method}",
            httpContext.Request.Path, httpContext.Request.Method);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request",
            Detail = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                ? exception.Message
                : "An internal server error occurred",
            Instance = httpContext.Request.Path
        };

        // Add exception type for better debugging in development
        if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
