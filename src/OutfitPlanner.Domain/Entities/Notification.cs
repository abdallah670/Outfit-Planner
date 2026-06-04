using OutfitPlanner.Domain.Enums;
namespace OutfitPlanner.Domain.Entities;


public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    
    // Notification Content
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    
    // Status
    public bool IsRead { get; set; }
   
}

