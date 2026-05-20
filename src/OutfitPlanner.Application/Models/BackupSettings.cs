namespace OutfitPlanner.Application.Models
{
    public class BackupSettings
    {
        public const string SectionName = "BackupSettings";

        public string BackupDirectory { get; set; } = "Backups";
        public int DefaultRetentionDays { get; set; } = 30;
        public int MaxBackupSizeMB { get; set; } = 500;
        public bool CompressionEnabled { get; set; } = true;
        public ScheduledBackupSettings ScheduledBackups { get; set; } = new();
    }

    public class ScheduledBackupSettings
    {
        public string Daily { get; set; } = "0 2 * * *";
        public string Weekly { get; set; } = "0 3 * * 0";
        public string Monthly { get; set; } = "0 4 1 * *";
    }
}