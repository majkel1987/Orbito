namespace Orbito.Domain.Common;

/// <summary>
/// Represents the result of an operation without a return value.
/// Implements the Result Pattern for better error handling.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Error information if operation failed
    /// </summary>
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Cannot create a successful result with an error");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Cannot create a failed result without an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failed result with an error
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result with a value type
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);

    /// <summary>
    /// Creates a failed result with an error code and message
    /// </summary>
    public static Result Failure(string code, string message) =>
        new(false, Error.Create(code, message));

    /// <summary>
    /// Creates a failed result with a value type and error code and message
    /// </summary>
    public static Result<TValue> Failure<TValue>(string code, string message) =>
        new(default, false, Error.Create(code, message));
}

/// <summary>
/// Represents the result of an operation with a return value.
/// Implements the Result Pattern for better error handling.
/// </summary>
/// <typeparam name="TValue">Type of the value returned by the operation</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value if operation was successful.
    /// Throws InvalidOperationException if operation failed.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result");

    /// <summary>
    /// Implicit conversion from value to successful result
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) =>
        Success(value);
}
