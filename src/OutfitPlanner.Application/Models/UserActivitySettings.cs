namespace OutfitPlanner.Application.Models
{
    public class UserActivitySettings
    {
        public const string SectionName = "UserActivitySettings";

        public int ActivityRetentionDays { get; set; } = 90;
        public int SessionTimeoutMinutes { get; set; } = 30;
        public bool EnableDetailedTracking { get; set; } = true;
        public bool TrackPageViews { get; set; } = true;
        public bool TrackUserActions { get; set; } = true;
    }
}