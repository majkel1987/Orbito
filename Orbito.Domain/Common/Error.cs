namespace Orbito.Domain.Common;

/// <summary>
/// Represents an error in the domain layer.
/// Immutable value object for error handling.
/// </summary>
public sealed record Error
{
    /// <summary>
    /// Represents no error (successful operation)
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents a null value error
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");

    /// <summary>
    /// Error code for programmatic handling
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; }

    private Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// Creates a new error
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="message">Error message</param>
    /// <returns>New error instance</returns>
    public static Error Create(string code, string message) => new(code, message);

    /// <summary>
    /// Implicit conversion to string returns error code
    /// </summary>
    public static implicit operator string(Error error) => error.Code;

    /// <summary>
    /// Checks if error represents a failure
    /// </summary>
    public bool IsFailure => this != None;

    /// <summary>
    /// Checks if error represents success
    /// </summary>
    public bool IsSuccess => this == None;
}
