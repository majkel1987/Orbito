namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Abstraction for date/time operations to enable testability
/// </summary>
public interface IDateTime
{
    /// <summary>
    /// Gets the current local date and time
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }
}
