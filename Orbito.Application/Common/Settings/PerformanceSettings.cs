namespace Orbito.Application.Common.Settings;

/// <summary>
/// Configuration settings for performance monitoring
/// </summary>
public class PerformanceSettings
{
    /// <summary>
    /// Threshold in milliseconds to start monitoring operation performance
    /// </summary>
    public int MonitorThresholdMs { get; set; } = 200;

    /// <summary>
    /// Threshold in milliseconds to log performance warnings
    /// </summary>
    public int WarningThresholdMs { get; set; } = 500;

    /// <summary>
    /// Threshold in milliseconds to log critical performance issues
    /// </summary>
    public int CriticalThresholdMs { get; set; } = 1000;
}
