namespace OutfitPlanner.Application.Models
{
    public class ServiceManagementSettings
    {
        public const string SectionName = "ServiceManagementSettings";

        public int HealthCheckIntervalSeconds { get; set; } = 60;
        public int LogRetentionDays { get; set; } = 7;
        public int MetricsRetentionHours { get; set; } = 24;
        public int ServiceRestartTimeoutSeconds { get; set; } = 30;
        public bool EnableAutoRestart { get; set; } = false;
    }
}