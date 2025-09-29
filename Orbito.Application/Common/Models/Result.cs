namespace Orbito.Application.Common.Models
{
    /// <summary>
    /// Generic Result pattern for better error handling
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public record Result<T>
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public required bool IsSuccess { get; init; }

        /// <summary>
        /// The value if successful
        /// </summary>
        public T? Value { get; init; }

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Error code if failed
        /// </summary>
        public string? ErrorCode { get; init; }

        /// <summary>
        /// Additional error details
        /// </summary>
        public Dictionary<string, string> ErrorDetails { get; init; } = new();

        /// <summary>
        /// Timestamp when the result was created
        /// </summary>
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Create a successful result
        /// </summary>
        public static Result<T> Success(T value) => new()
        {
            IsSuccess = true,
            Value = value
        };

        /// <summary>
        /// Create a failed result
        /// </summary>
        public static Result<T> Failure(string errorMessage, string? errorCode = null, Dictionary<string, string>? errorDetails = null) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            ErrorDetails = errorDetails ?? new Dictionary<string, string>()
        };

        /// <summary>
        /// Create a failed result from exception
        /// </summary>
        public static Result<T> Failure(Exception exception, string? errorCode = null) => new()
        {
            IsSuccess = false,
            ErrorMessage = exception.Message,
            ErrorCode = errorCode ?? exception.GetType().Name,
            ErrorDetails = new Dictionary<string, string>
            {
                ["ExceptionType"] = exception.GetType().Name,
                ["StackTrace"] = exception.StackTrace ?? string.Empty
            }
        };

        /// <summary>
        /// Implicit conversion from value to success result
        /// </summary>
        public static implicit operator Result<T>(T value) => Success(value);

        /// <summary>
        /// Implicit conversion from exception to failure result
        /// </summary>
        public static implicit operator Result<T>(Exception exception) => Failure(exception);
    }

    /// <summary>
    /// Non-generic Result for operations that don't return a value
    /// </summary>
    public record Result
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public required bool IsSuccess { get; init; }

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Error code if failed
        /// </summary>
        public string? ErrorCode { get; init; }

        /// <summary>
        /// Additional error details
        /// </summary>
        public Dictionary<string, string> ErrorDetails { get; init; } = new();

        /// <summary>
        /// Timestamp when the result was created
        /// </summary>
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Create a successful result
        /// </summary>
        public static Result Success() => new()
        {
            IsSuccess = true
        };

        /// <summary>
        /// Create a failed result
        /// </summary>
        public static Result Failure(string errorMessage, string? errorCode = null, Dictionary<string, string>? errorDetails = null) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            ErrorDetails = errorDetails ?? new Dictionary<string, string>()
        };

        /// <summary>
        /// Create a failed result from exception
        /// </summary>
        public static Result Failure(Exception exception, string? errorCode = null) => new()
        {
            IsSuccess = false,
            ErrorMessage = exception.Message,
            ErrorCode = errorCode ?? exception.GetType().Name,
            ErrorDetails = new Dictionary<string, string>
            {
                ["ExceptionType"] = exception.GetType().Name,
                ["StackTrace"] = exception.StackTrace ?? string.Empty
            }
        };

        /// <summary>
        /// Implicit conversion from exception to failure result
        /// </summary>
        public static implicit operator Result(Exception exception) => Failure(exception);
    }
}
