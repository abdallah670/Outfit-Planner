using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.DTOs.Notification;

public class CreateNotificationDto
{
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
}
