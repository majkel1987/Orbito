namespace Orbito.Application.Common.Settings
{
    public class PerformanceSettings
    {
        public int MonitorThresholdMs { get; set; } = 200;
        public int WarningThresholdMs { get; set; } = 500;
        public int CriticalThresholdMs { get; set; } = 1000;
    }
}
