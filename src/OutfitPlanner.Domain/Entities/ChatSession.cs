namespace OutfitPlanner.Domain.Entities;

public class ChatSession : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active, Archived
    public int MessageCount { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
    
    // Navigation
    public User User { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}