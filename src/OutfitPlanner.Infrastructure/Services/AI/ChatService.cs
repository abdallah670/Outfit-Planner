using System.Text.Json;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IIntentClassifier intentClassifier,
        IWardrobeContextBuilder wardrobeContextBuilder,
        IColorHarmonyService colorHarmonyService,
        IStyleCompatibilityService styleCompatibilityService,
        IOutfitCombinationService outfitCombinationService,
        ILLMResponseGenerator responseGenerator,
        IChatHistoryCache chatHistoryCache,
        IChatSessionRepository sessionRepository,
        ILogger<ChatService> logger)
    {
        _intentClassifier = intentClassifier;
        _wardrobeContextBuilder = wardrobeContextBuilder;
        _colorHarmonyService = colorHarmonyService;
        _styleCompatibilityService = styleCompatibilityService;
        _outfitCombinationService = outfitCombinationService;
        _responseGenerator = responseGenerator;
        _chatHistoryCache = chatHistoryCache;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<ChatResponse> ProcessMessageAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var intent = await _intentClassifier.ClassifyAsync(request.Message, cancellationToken);
        var sessionId = request.SessionId ?? Guid.NewGuid();
        var uploadedImageUrls = request.UploadedImageUrls ?? new List<string>();

        var hasImages = request.Images != null && request.Images.Any();

        // When the user attaches an image, their intent is always to get visual feedback on it.
        // Override vague/unrecognised intents so the full LLM pipeline always runs with the image.
        if (hasImages && intent.Intent is "general" or "greeting")
        {
            intent = new IntentResult
            {
                Intent = "outfit_rating",
                Occasion = intent.Occasion,
                WeatherCondition = intent.WeatherCondition,
                Season = intent.Season,
                MentionedItems = intent.MentionedItems
            };
        }

        // Save user message to cache
        await _chatHistoryCache.AddMessageAsync(sessionId, new CachedChatMessage
        {
            Role = "user",
            Content = request.Message,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);

        // When the user uploads an image they want visual feedback on it — not wardrobe combinations.
        // Wardrobe context is only needed for text-only outfit suggestion / analysis requests.
        var needsWardrobe = !hasImages &&
                            intent.Intent is "outfit_suggestion" or "outfit_rating" or "wardrobe_analysis";
        
        var context = needsWardrobe 
            ? await _wardrobeContextBuilder.BuildContextAsync(request.UserId, intent, cancellationToken)
            : new WardrobeContext { UserId = request.UserId, AvailableItems = new List<WardrobeItemContext>() };

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
            request.Message, intent, context, combinations, harmony, styleScore, historyEntries, request.Images, cancellationToken);

        // Save AI response to cache
        await _chatHistoryCache.AddMessageAsync(sessionId, new CachedChatMessage
        {
            Role = "assistant",
            Content = llmResponse.Text,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Build structured metadata for persistence (outfit suggestions only).
        // NOTE: uploadedImageUrls are now stored on the user message, NOT here,
        // to prevent images from appearing on the AI bubble when reloaded.
        // Use camelCase property names to match frontend TypeScript interfaces.
        var metadata = new
        {
            outfitSuggestions = combinations.Combinations.Select(c => new
            {
                rank = c.Rank,
                totalScore = c.TotalScore,
                scoreBreakdown = c.ScoreBreakdown,
                items = c.Items.Select(i => new { id = i.Id, name = i.Name, type = i.Type, imageUrl = i.ImageUrl, hexColor = i.HexColor })
            })
        };

        // Persist to database with proper error logging
        try
        {
            await PersistSessionAsync(sessionId, request.UserId, request.Message, llmResponse.Text, metadata, intent.Intent, uploadedImageUrls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist chat session {SessionId}", sessionId);
        }

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
            }).ToList()
        };
    }
    
    private async Task PersistSessionAsync(Guid sessionId, string userId, string userMessage, string aiText, object? metadata = null, string? intent = null, List<string>? uploadedImageUrls = null)
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
                CreatedAt = DateTimeOffset.UtcNow,
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

        // Store uploaded image URLs directly in the user message's Images field
        // so they are correctly attributed to the user bubble on history reload.
        var userImagesJson = uploadedImageUrls != null && uploadedImageUrls.Any()
            ? JsonSerializer.Serialize(uploadedImageUrls)
            : null;

        await _sessionRepository.AddMessageAsync(new Domain.Entities.ChatMessage
        {
            SessionId = sessionId,
            SenderId = userId,
            Content = userMessage,
            Role = "user",
            Intent = intent,
            Images = userImagesJson
        });

        var aiMessage = new Domain.Entities.ChatMessage
        {
            SessionId = sessionId,
            SenderId = "ai",
            Content = aiText,
            Role = "assistant",
            Intent = "assistant"
        };

        if (metadata != null)
        {
            aiMessage.Metadata = JsonSerializer.Serialize(metadata);
        }

        await _sessionRepository.AddMessageAsync(aiMessage);
    }
}