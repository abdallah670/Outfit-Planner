namespace OutfitPlanner.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public string SenderId { get; set; } = string.Empty;      // User ID or "assistant"
    public string Content { get; set; } = string.Empty;        // Message text
    public string Role { get; set; } = string.Empty;           // "user" or "assistant"
    public string? Intent { get; set; }                        // Classified intent (optional)
    public string? Metadata { get; set; }                      // JSON: outfit suggestions, scores, etc.
    
    // Navigation
    public ChatSession Session { get; set; } = null!;
}