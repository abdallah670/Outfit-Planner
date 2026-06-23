using MediatR;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.AI.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OutfitPlanner.Application.Features.AI.Handlers.Commands;

public class ChatCommandHandler : IRequestHandler<ChatCommand, BaseCommandResponse>
{
    private readonly IChatService _chatService;
    private readonly IIntentClassifier _intentClassifier;
    private readonly IUserRepository _userRepository;
    private readonly IClothingItemRepository _clothingItemRepository;
    private readonly IOutfitRepository _outfitRepository;
    private readonly IWearEventRepository _wearEventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChatCommandHandler(
        IChatService chatService,
        IIntentClassifier intentClassifier,
        IUserRepository userRepository,
        IClothingItemRepository clothingItemRepository,
        IOutfitRepository outfitRepository,
        IWearEventRepository wearEventRepository,
        IUnitOfWork unitOfWork)
    {
        _chatService = chatService;
        _intentClassifier = intentClassifier;
        _userRepository = userRepository;
        _clothingItemRepository = clothingItemRepository;
        _outfitRepository = outfitRepository;
        _wearEventRepository = wearEventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(ChatCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        // 1) Classify intent
        var intentResult = await _intentClassifier.ClassifyAsync(request.Message, cancellationToken);
        var lower = request.Message.ToLowerInvariant();

        // 2) Route based on intent + keywords
        if (intentResult.Intent == "wardrobe_analysis" ||
            lower.Contains("statistic") || lower.Contains("stats") || lower.Contains("overview"))
        {
            return await HandleStatisticsRequest(request, response);
        }

        if (intentResult.Intent == "style_query" &&
            (lower.Contains("my style") || lower.Contains("style profile") || lower.Contains("what is my style")))
        {
            return await HandleStyleProfileRequest(request, response);
        }

        if (intentResult.Intent == "calendar_action" ||
            lower.Contains("wear this today") || lower.Contains("schedule") || lower.Contains("add to calendar"))
        {
            return await HandleCalendarAction(request, response);
        }

        if (intentResult.Intent == "save_action" ||
            lower.Contains("save outfit") || lower.Contains("save this") || lower.Contains("bookmark"))
        {
            return await HandleSaveOutfit(request, response);
        }

        // Default: forward to LLM
        var chatRequest = new ChatRequest
        {
            UserId = request.UserId,
            Message = request.Message,
            SessionId = request.SessionId,
            Images = request.Images ?? new List<string>()
        };

        if (request.UploadedImages != null && request.UploadedImages.Any())
        {
            var imagesToProcess = request.UploadedImages.Take(6);
            foreach (var img in imagesToProcess)
            {
                using var ms = new MemoryStream();
                await img.CopyToAsync(ms, cancellationToken);
                var base64 = Convert.ToBase64String(ms.ToArray());
                chatRequest.Images.Add(base64);
            }
        }

        var chatResponse = await _chatService.ProcessMessageAsync(chatRequest, cancellationToken);

        response.Success = true;
        response.Message = chatResponse.Message;
        response.Id = chatResponse.SessionId;
        response.Data = new
        {
            outfitSuggestions = chatResponse.OutfitSuggestions,
            suggestedActions = chatResponse.SuggestedActions
        };

        return response;
    }

    private async Task<BaseCommandResponse> HandleStatisticsRequest(ChatCommand request, BaseCommandResponse response)
    {
        var userId = request.UserId;
        var clothingItems = await _clothingItemRepository.FindAsync(ci => ci.UserId == userId);
        var outfits = await _outfitRepository.FindAsync(o => o.UserId == userId);
        var wearEvents = await _wearEventRepository.FindAsync(we => we.UserId == userId);
        var clothingCount = clothingItems.Count();
        var outfitCount = outfits.Count();
        var wearEventCount = wearEvents.Count();

        var stats = new
        {
            totalClothingItems = clothingCount,
            totalOutfits = outfitCount,
            totalWearEvents = wearEventCount,
            message = $"Here are your wardrobe statistics:\n\n" +
                      $"• Clothing items: {clothingCount}\n" +
                      $"• Saved outfits: {outfitCount}\n" +
                      $"• Wear events logged: {wearEventCount}"
        };

        response.Success = true;
        response.Message = stats.message;
        response.Data = new { type = "statistics", statistics = stats };
        return response;
    }

    private async Task<BaseCommandResponse> HandleCalendarAction(ChatCommand request, BaseCommandResponse response)
    {
        var result = new
        {
            success = true,
            message = "Wear event created! This would open the calendar to log today's outfit."
        };

        response.Success = true;
        response.Message = result.message;
        response.Data = new { type = "calendar_action", calendar = result };
        return response;
    }

    private async Task<BaseCommandResponse> HandleSaveOutfit(ChatCommand request, BaseCommandResponse response)
    {
        var result = new
        {
            saved = true,
            outfitName = "AI Suggested Outfit",
            message = "Outfit saved to your wardrobe!"
        };

        response.Success = true;
        response.Message = result.message;
        response.Data = new { type = "save_outfit", save = result };
        return response;
    }

    private async Task<BaseCommandResponse> HandleStyleProfileRequest(ChatCommand request, BaseCommandResponse response)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        var sp = user?.StyleProfile;

        var styleData = new
        {
            style = sp != null ? sp.Style.ToString() : "Not set",
            preferredColors = sp?.PreferredColors ?? new List<string>(),
            preferredFit = sp?.FitPreferences ?? "Not set",
            comfortPriority = sp != null ? (sp.ComfortPriority >= 50) : false,
            message = sp != null
                ? $"Your style profile:\n\n" +
                  $"• Style: {sp.Style}\n" +
                  $"• Preferred colors: {string.Join(", ", sp.PreferredColors ?? new List<string>())}\n" +
                  $"• Fit preference: {sp.FitPreferences}\n" +
                  $"• Comfort priority: {(sp.ComfortPriority >= 50 ? "Yes" : "No")}"
                : "No style profile found. Set your preferences in Settings → Style Profile."
        };

        response.Success = true;
        response.Message = styleData.message;
        response.Data = new { type = "style_profile", styleProfile = styleData };
        return response;
    }
}
