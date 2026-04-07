using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Orbito.Application.Common.Extensions;

/// <summary>
/// Extensions for structured logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs operation execution with timing
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="operation">Operation to execute</param>
    /// <param name="additionalProperties">Additional properties to log</param>
    public static async Task<T> LogOperationAsync<T>(
        this ILogger logger, 
        string operationName, 
        Func<Task<T>> operation,
        Dictionary<string, object>? additionalProperties = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var properties = new Dictionary<string, object>
        {
            ["OperationName"] = operationName,
            ["StartTime"] = DateTime.UtcNow
        };

        if (additionalProperties != null)
        {
            foreach (var prop in additionalProperties)
            {
                properties[prop.Key] = prop.Value;
            }
        }

        using var scope = logger.BeginScope(properties);
        
        try
        {
            logger.LogInformation("Starting {OperationName}", operationName);
            
            var result = await operation();
            
            stopwatch.Stop();
            logger.LogInformation("Completed {OperationName} in {ElapsedMilliseconds}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Failed {OperationName} after {ElapsedMilliseconds}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Logs operation execution with timing (void return)
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="operation">Operation to execute</param>
    /// <param name="additionalProperties">Additional properties to log</param>
    public static async Task LogOperationAsync(
        this ILogger logger,
        string operationName,
        Func<Task> operation,
        Dictionary<string, object>? additionalProperties = null)
    {
        await LogOperationAsync<object?>(logger, operationName, async () =>
        {
            await operation();
            return null;
        }, additionalProperties);
    }

    /// <summary>
    /// Logs security events with structured data
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="eventType">Type of security event</param>
    /// <param name="message">Event message</param>
    /// <param name="properties">Additional properties</param>
    public static void LogSecurityEvent(
        this ILogger logger, 
        string eventType, 
        string message, 
        Dictionary<string, object>? properties = null)
    {
        var logProperties = new Dictionary<string, object>
        {
            ["SecurityEventType"] = eventType,
            ["Timestamp"] = DateTime.UtcNow
        };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                logProperties[prop.Key] = prop.Value;
            }
        }

        using var scope = logger.BeginScope(logProperties);
        logger.LogWarning("SECURITY: {SecurityEventType} - {Message}", eventType, message);
    }

    /// <summary>
    /// Logs performance metrics
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="metricName">Name of the metric</param>
    /// <param name="value">Metric value</param>
    /// <param name="unit">Unit of measurement</param>
    /// <param name="properties">Additional properties</param>
    public static void LogPerformanceMetric(
        this ILogger logger, 
        string metricName, 
        double value, 
        string unit = "ms",
        Dictionary<string, object>? properties = null)
    {
        var logProperties = new Dictionary<string, object>
        {
            ["MetricName"] = metricName,
            ["MetricValue"] = value,
            ["MetricUnit"] = unit,
            ["Timestamp"] = DateTime.UtcNow
        };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                logProperties[prop.Key] = prop.Value;
            }
        }

        using var scope = logger.BeginScope(logProperties);
        logger.LogInformation("PERFORMANCE: {MetricName} = {MetricValue} {MetricUnit}", 
            metricName, value, unit);
    }

    /// <summary>
    /// Logs business events with structured data
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="eventType">Type of business event</param>
    /// <param name="message">Event message</param>
    /// <param name="properties">Additional properties</param>
    public static void LogBusinessEvent(
        this ILogger logger, 
        string eventType, 
        string message, 
        Dictionary<string, object>? properties = null)
    {
        var logProperties = new Dictionary<string, object>
        {
            ["BusinessEventType"] = eventType,
            ["Timestamp"] = DateTime.UtcNow
        };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                logProperties[prop.Key] = prop.Value;
            }
        }

        using var scope = logger.BeginScope(logProperties);
        logger.LogInformation("BUSINESS: {BusinessEventType} - {Message}", eventType, message);
    }
}
