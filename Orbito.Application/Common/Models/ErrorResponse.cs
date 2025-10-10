namespace Orbito.Application.Common.Models;

/// <summary>
/// Standard error response model for API endpoints
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// Error code for programmatic handling
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Detailed error description (optional)
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Request correlation ID for tracking
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Additional error properties (for validation errors, etc.)
    /// </summary>
    public Dictionary<string, object>? Properties { get; init; }

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    public static ErrorResponse ValidationError(string message, Dictionary<string, object>? properties = null)
    {
        return new ErrorResponse
        {
            Code = "VALIDATION_ERROR",
            Message = message,
            Properties = properties
        };
    }

    /// <summary>
    /// Creates a not found error response
    /// </summary>
    public static ErrorResponse NotFound(string resource, string identifier)
    {
        return new ErrorResponse
        {
            Code = "NOT_FOUND",
            Message = $"{resource} with identifier '{identifier}' was not found"
        };
    }

    /// <summary>
    /// Creates an unauthorized error response
    /// </summary>
    public static ErrorResponse Unauthorized(string message = "Unauthorized access")
    {
        return new ErrorResponse
        {
            Code = "UNAUTHORIZED",
            Message = message
        };
    }

    /// <summary>
    /// Creates a forbidden error response
    /// </summary>
    public static ErrorResponse Forbidden(string message = "Access forbidden")
    {
        return new ErrorResponse
        {
            Code = "FORBIDDEN",
            Message = message
        };
    }

    /// <summary>
    /// Creates a business logic error response
    /// </summary>
    public static ErrorResponse BusinessError(string code, string message, string? details = null)
    {
        return new ErrorResponse
        {
            Code = code,
            Message = message,
            Details = details
        };
    }

    /// <summary>
    /// Creates an internal server error response
    /// </summary>
    public static ErrorResponse InternalError(string? correlationId = null)
    {
        return new ErrorResponse
        {
            Code = "INTERNAL_ERROR",
            Message = "An internal server error occurred",
            CorrelationId = correlationId
        };
    }
}
