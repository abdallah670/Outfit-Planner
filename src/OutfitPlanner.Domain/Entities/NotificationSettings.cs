namespace OutfitPlanner.Domain.Entities;

public class NotificationSettings : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    
    // Email Notifications
    public bool DailyOutfitSuggestion { get; set; } = true;
    public bool WeeklyStyleReport { get; set; } = false;
    public bool WeatherAlerts { get; set; } = true;
    public bool NewFeatures { get; set; } = true;
    public bool SocialNotifications { get; set; } = true;
    
    // Push Notifications
    public bool PushNotificationsEnabled { get; set; } = true;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
