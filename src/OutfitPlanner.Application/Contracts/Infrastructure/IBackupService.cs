namespace OutfitPlanner.Application.Contracts.Infrastructure;

public interface IBackupService
{
    /// <summary>
    /// Creates a database backup
    /// </summary>
    Task<BackupResult> CreateBackupAsync(BackupRequest request);

    /// <summary>
    /// Gets a list of all available backups
    /// </summary>
    Task<IEnumerable<BackupInfo>> GetAvailableBackupsAsync();

    /// <summary>
    /// Gets detailed information about a specific backup
    /// </summary>
    Task<BackupInfo?> GetBackupInfoAsync(string backupId);

    /// <summary>
    /// Restores a database from backup
    /// </summary>
    Task<RestoreResult> RestoreBackupAsync(string backupId);

    /// <summary>
    /// Deletes a backup file
    /// </summary>
    Task<bool> DeleteBackupAsync(string backupId);

    /// <summary>
    /// Gets backup statistics and health information
    /// </summary>
    Task<BackupStatistics> GetBackupStatisticsAsync();

    /// <summary>
    /// Schedules automatic backups
    /// </summary>
    Task ScheduleBackupAsync(BackupSchedule schedule);

    /// <summary>
    /// Validates backup integrity
    /// </summary>
    Task<bool> ValidateBackupAsync(string backupId);
}

public record BackupRequest(
    string Name,
    string Description,
    bool IncludeFiles = true,
    bool Compress = true,
    string[]? ExcludedTables = null
);

public record BackupResult(
    bool Success,
    string BackupId,
    string FileName,
    long FileSize,
    DateTime CreatedAt,
    string? ErrorMessage = null
);

public record BackupInfo(
    string Id,
    string Name,
    string Description,
    string FileName,
    long FileSize,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    BackupStatus Status,
    string CreatedBy
);

public record RestoreResult(
    bool Success,
    DateTime RestoredAt,
    string? ErrorMessage = null
);

public record BackupStatistics(
    int TotalBackups,
    long TotalStorageUsed,
    DateTime LastBackupAt,
    int SuccessfulBackups,
    int FailedBackups,
    double AverageBackupSize
);

public record BackupSchedule(
    bool Enabled,
    string Frequency, // "daily", "weekly", "monthly"
    TimeSpan TimeOfDay,
    string[]? DaysOfWeek = null,
    int? DayOfMonth = null,
    bool RetentionPolicy = true,
    int RetentionDays = 30
);

public enum BackupStatus
{
    InProgress,
    Completed,
    Failed,
    Expired
}
