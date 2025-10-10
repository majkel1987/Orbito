namespace Orbito.Application.Common.Models;

/// <summary>
/// Represents a date range for metrics and statistics queries
/// </summary>
public record DateRange
{
    /// <summary>
    /// Start date of the range (inclusive)
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// End date of the range (inclusive)
    /// </summary>
    public DateTime EndDate { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public DateRange()
    {
    }

    /// <summary>
    /// Constructor with start and end dates
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    public DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Creates a date range for the last N days
    /// </summary>
    /// <param name="days">Number of days</param>
    /// <returns>Date range for the last N days</returns>
    public static DateRange LastDays(int days)
    {
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-days + 1);
        return new DateRange(startDate, endDate);
    }

    /// <summary>
    /// Creates a date range for the current month
    /// </summary>
    /// <returns>Date range for the current month</returns>
    public static DateRange CurrentMonth()
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return new DateRange(startDate, endDate);
    }

    /// <summary>
    /// Creates a date range for the last month
    /// </summary>
    /// <returns>Date range for the last month</returns>
    public static DateRange LastMonth()
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return new DateRange(startDate, endDate);
    }

    /// <summary>
    /// Creates a date range for the current year
    /// </summary>
    /// <returns>Date range for the current year</returns>
    public static DateRange CurrentYear()
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, 1, 1);
        var endDate = new DateTime(now.Year, 12, 31);
        return new DateRange(startDate, endDate);
    }

    /// <summary>
    /// Validates the date range
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return StartDate <= EndDate;
    }

    /// <summary>
    /// Gets the duration of the date range
    /// </summary>
    /// <returns>Time span representing the duration</returns>
    public TimeSpan Duration => EndDate - StartDate;

    /// <summary>
    /// Gets the number of days in the range
    /// </summary>
    /// <returns>Number of days</returns>
    public int Days => (int)Duration.TotalDays + 1;
}
