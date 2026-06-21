namespace OutfitPlanner.Application.DTOs.AI;

public class ChatSessionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
}
