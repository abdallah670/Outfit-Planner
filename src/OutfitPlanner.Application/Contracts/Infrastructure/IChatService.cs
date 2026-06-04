namespace OutfitPlanner.Application.Contracts.Infrastructure;

public class ChatRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid SessionId { get; set; }
    public List<OutfitSuggestionDto> OutfitSuggestions { get; set; } = new();
    public List<string> SuggestedActions { get; set; } = new();
}

public class OutfitSuggestionDto
{
    public int Rank { get; set; }
    public double TotalScore { get; set; }
    public Dictionary<string, double> ScoreBreakdown { get; set; } = new();
    public List<SuggestedItemDto> Items { get; set; } = new();
}

public class SuggestedItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
}

public interface IChatService
{
    Task<ChatResponse> ProcessMessageAsync(ChatRequest request, CancellationToken cancellationToken = default);
}