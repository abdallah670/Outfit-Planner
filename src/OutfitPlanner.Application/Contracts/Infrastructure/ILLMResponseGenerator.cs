namespace OutfitPlanner.Application.Contracts.Infrastructure;

public class LLMResponse
{
    public string Text { get; set; } = string.Empty;
    public List<string> SuggestedActions { get; set; } = new();
    public string? RawJson { get; set; }
}

public class ChatHistoryEntry
{
    public string Role { get; set; } = string.Empty;    // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
}

public interface ILLMResponseGenerator
{
    Task<LLMResponse> GenerateResponseAsync(
        string userMessage,
        IntentResult intent,
        WardrobeContext context,
        OutfitCombinationResult combinations,
        ColorHarmonyResult harmony,
        StyleScoreResult styleScore,
        List<ChatHistoryEntry> recentHistory,
        List<string>? images = null,
        CancellationToken cancellationToken = default);
}