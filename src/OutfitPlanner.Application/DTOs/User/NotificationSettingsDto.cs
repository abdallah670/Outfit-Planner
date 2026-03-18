namespace OutfitPlanner.Application.DTOs.User;

public class NotificationSettingsDto
{
    public bool DailyOutfitSuggestion { get; set; }
    public bool WeeklyStyleReport { get; set; }
    public bool WeatherAlerts { get; set; }
    public bool NewFeatures { get; set; }
    public bool SocialNotifications { get; set; }
    public bool PushNotificationsEnabled { get; set; }
}

public class UpdateNotificationSettingsDto
{
    public bool DailyOutfitSuggestion { get; set; }
    public bool WeeklyStyleReport { get; set; }
    public bool WeatherAlerts { get; set; }
    public bool NewFeatures { get; set; }
    public bool SocialNotifications { get; set; }
    public bool PushNotificationsEnabled { get; set; }
}
