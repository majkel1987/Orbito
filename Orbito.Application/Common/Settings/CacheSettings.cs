namespace Orbito.Application.Common.Settings
{
    public class CacheSettings
    {
        public int DepartmentCacheMinutes { get; set; } = 15;
        public int DefaultAbsoluteExpirationHours { get; set; } = 1;
        public bool EnableCaching { get; set; } = true;
    }
}
