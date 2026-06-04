using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Common.Interfaces.Persistence;

namespace OutfitPlanner.Infrastructure.Services.AI;

public class ChatService : IChatService
{
    private readonly IIntentClassifier _intentClassifier;
    private readonly IWardrobeContextBuilder _wardrobeContextBuilder;
    private readonly IColorHarmonyService _colorHarmonyService;
    private readonly IStyleCompatibilityService _styleCompatibilityService;
    private readonly IOutfitCombinationService _outfitCombinationService;
    private readonly ILLMResponseGenerator _responseGenerator;
    private readonly IChatHistoryCache _chatHistoryCache;
    private readonly IChatSessionRepository _sessionRepository;

    public ChatService(
        IIntentClassifier intentClassifier,
        IWardrobeContextBuilder wardrobeContextBuilder,
        IColorHarmonyService colorHarmonyService,
        IStyleCompatibilityService styleCompatibilityService,
        IOutfitCombinationService outfitCombinationService,
        ILLMResponseGenerator responseGenerator,
        IChatHistoryCache chatHistoryCache,
        IChatSessionRepository sessionRepository)
    {
        _intentClassifier = intentClassifier;
        _wardrobeContextBuilder = wardrobeContextBuilder;
        _colorHarmonyService = colorHarmonyService;
        _styleCompatibilityService = styleCompatibilityService;
        _outfitCombinationService = outfitCombinationService;
        _responseGenerator = responseGenerator;
        _chatHistoryCache = chatHistoryCache;
        _sessionRepository = sessionRepository;
    }

    public async Task<ChatResponse> ProcessMessageAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var intent = await _intentClassifier.ClassifyAsync(request.Message, cancellationToken);
        var sessionId = request.SessionId ?? Guid.NewGuid();

        // Save user message to cache
        await _chatHistoryCache.AddMessageAsync(sessionId, new CachedChatMessage
        {
            Role = "user",
            Content = request.Message,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Build context for outfit-related intents
        var needsWardrobe = intent.Intent is "outfit_suggestion" or "outfit_rating" or "wardrobe_analysis";
        var context = needsWardrobe
            ? await _wardrobeContextBuilder.BuildContextAsync(request.UserId, intent, cancellationToken)
            : new WardrobeContext { UserId = request.UserId };

        var combinations = needsWardrobe
            ? await _outfitCombinationService.GenerateCombinationsAsync(request.UserId, intent.Occasion, intent.WeatherCondition, maxResults: 3, cancellationToken: cancellationToken)
            : new OutfitCombinationResult();

        ColorHarmonyResult? harmony = null;
        StyleScoreResult? styleScore = null;

        if (combinations.Combinations.Any())
        {
            var best = combinations.Combinations.First();
            harmony = await _colorHarmonyService.CalculateHarmonyAsync(best.Items.Select(i => i.HexColor), cancellationToken);
            styleScore = await _styleCompatibilityService.CalculateScoreAsync(
                best.Items.Select(i => i.Id), intent.Occasion ?? "casual", intent.WeatherCondition, request.UserId, cancellationToken);
        }
        else
        {
            harmony = new ColorHarmonyResult { Score = 0, Scheme = "None", Explanation = "", HexColors = new List<string>() };
            styleScore = new StyleScoreResult { TotalScore = 0, Breakdown = new(), Summary = "", Suggestions = new() };
        }

        var recentHistory = await _chatHistoryCache.GetRecentMessagesAsync(sessionId, 5, cancellationToken);
        var historyEntries = recentHistory.Select(m => new ChatHistoryEntry { Role = m.Role, Content = m.Content }).ToList();

        var llmResponse = await _responseGenerator.GenerateResponseAsync(
            request.Message, intent, context, combinations, harmony, styleScore, historyEntries, cancellationToken);

        // Save AI response to cache
        await _chatHistoryCache.AddMessageAsync(sessionId, new CachedChatMessage
        {
            Role = "assistant",
            Content = llmResponse.Text,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Persist to database (fire-and-forget)
        _ = PersistSessionAsync(sessionId, request.UserId, request.Message, llmResponse.Text);

        return new ChatResponse
        {
            Message = llmResponse.Text,
            SessionId = sessionId,
            OutfitSuggestions = combinations.Combinations.Select(c => new OutfitSuggestionDto
            {
                Rank = c.Rank,
                TotalScore = c.TotalScore,
                ScoreBreakdown = c.ScoreBreakdown,
                Items = c.Items.Select(i => new SuggestedItemDto { Id = i.Id, Name = i.Name, Type = i.Type, ImageUrl = i.ImageUrl, HexColor = i.HexColor }).ToList()
            }).ToList(),
            SuggestedActions = llmResponse.SuggestedActions
        };
    }

    private async Task PersistSessionAsync(Guid sessionId, string userId, string userMessage, string aiText)
    {
        try
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                session = new Domain.Entities.ChatSession
                {
                    Id = sessionId,
                    UserId = userId,
                    Title = userMessage.Length > 100 ? userMessage[..100] : userMessage,
                    Status = "Active",
                    MessageCount = 1,
                    LastActivityAt = DateTimeOffset.UtcNow
                };
                await _sessionRepository.AddAsync(session);
            }
            else
            {
                session.MessageCount++;
                session.LastActivityAt = DateTimeOffset.UtcNow;
                await _sessionRepository.UpdateAsync(session);
            }

            await _sessionRepository.AddMessageAsync(new Domain.Entities.ChatMessage
            {
                SessionId = sessionId,
                SenderId = userId,
                Content = userMessage,
                Role = "user"
            });

            await _sessionRepository.AddMessageAsync(new Domain.Entities.ChatMessage
            {
                SessionId = sessionId,
                SenderId = "ai",
                Content = aiText,
                Role = "assistant"
            });
        }
        catch
        {
            // Fire-and-forget, don't fail the response
        }
    }
}