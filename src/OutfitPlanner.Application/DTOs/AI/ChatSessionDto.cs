using OutfitPlanner.Application.Contracts.Infrastructure;

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
public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Intent { get; set; }
    public string? Metadata { get; set; }
    public List<string> Images { get; set; } = new List<string>();
    [System.Text.Json.Serialization.JsonPropertyName("outfitSuggestions")]
    public List<OutfitSuggestionDto>? OutfitSuggestions { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

