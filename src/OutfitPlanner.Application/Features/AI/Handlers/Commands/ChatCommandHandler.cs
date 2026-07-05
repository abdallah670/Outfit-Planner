using System.Text.Json;
using MediatR;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.AI.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.DTOs.AI;

namespace OutfitPlanner.Application.Features.AI.Handlers.Commands;

public class ChatCommandHandler : IRequestHandler<ChatCommand, BaseCommandResponse>
{
    private readonly ILogger<ChatCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IImageStorageService _imageStorageService; 
    private readonly IChatService _chatService;
    private readonly IIntentClassifier _intentClassifier;
    private readonly IUserRepository _userRepository;
    private readonly IClothingItemRepository _clothingItemRepository;
    private readonly IOutfitRepository _outfitRepository;
    private readonly IWearEventRepository _wearEventRepository;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;


    public ChatCommandHandler(
        IChatService chatService,
        IIntentClassifier intentClassifier,
        IUserRepository userRepository,
        IClothingItemRepository clothingItemRepository,
        IOutfitRepository outfitRepository,
        IWearEventRepository wearEventRepository,
        IChatSessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ILogger<ChatCommandHandler> logger,
        IMediator mediator)
    {
        _logger = logger;
        _imageStorageService = imageStorageService;
        _chatService = chatService;
        _intentClassifier = intentClassifier;
        _userRepository = userRepository;
        _clothingItemRepository = clothingItemRepository;
        _outfitRepository = outfitRepository;
        _wearEventRepository = wearEventRepository;
        _sessionRepository = sessionRepository;
        _mediator = mediator;
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
            response = await HandleStatisticsRequest(request, response);
            await PersistSpecialMessageAsync(request, response, intentResult.Intent);
            return response;
        }

        if (intentResult.Intent == "style_query" &&
            (lower.Contains("my style") || lower.Contains("style profile") || lower.Contains("what is my style")))
        {
            response = await HandleStyleProfileRequest(request, response);
            await PersistSpecialMessageAsync(request, response, intentResult.Intent);
            return response;
        }

        if (intentResult.Intent == "calendar_action" ||
            lower.Contains("wear this today") || lower.Contains("schedule") || lower.Contains("add to calendar"))
        {
            string intent = lower.Contains("wear this today") ? "wear_this_today" :  lower.Contains("schedule") ? "schedule" : "add_to_calendar";
            response = await HandleCalendarAction(request, response, intent);
            await PersistSpecialMessageAsync(request, response, intentResult.Intent);
            return response;
        }

        if (intentResult.Intent == "save_action" ||
            lower.Contains("save outfit") || lower.Contains("save this") || lower.Contains("bookmark"))
        {
            response = await HandleSaveOutfit(request, response);
            await PersistSpecialMessageAsync(request, response, intentResult.Intent);
            return response;
        }
       
        // Default: forward to LLM
        var uploadedUrls = new List<string>();

        var chatRequest = new ChatRequest
        {
            UserId = request.UserId,
            Message = request.Message,
            SessionId = request.SessionId,
            Images = request.Images ?? new List<string>(),
            UploadedImageUrls = uploadedUrls
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

                // Also save the image to persistent storage
                try
                {
                    var uploadResult = await _imageStorageService.UploadImageAsync(
                        new MemoryStream(ms.ToArray()),
                        img.FileName,
                        request.UserId);
                    uploadedUrls.Add(uploadResult.OriginalPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to save uploaded image {FileName}", img.FileName);
                }
            }
        }

        if (chatRequest.Images.Count == 0 && request.UploadedImages != null && request.UploadedImages.Any())
        {
            // Fallback: ensure at least one image is passed
            using var fallbackMs = new MemoryStream();
            await request.UploadedImages.First().CopyToAsync(fallbackMs, cancellationToken);
            chatRequest.Images.Add(Convert.ToBase64String(fallbackMs.ToArray()));
        }

        var chatResponse = await _chatService.ProcessMessageAsync(chatRequest, cancellationToken);

        response.Success = true;
        response.Message = chatResponse.Message;
        response.Id = chatResponse.SessionId;
        response.UploadedImageUrls = uploadedUrls;
        response.Data = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            SessionId = chatResponse.SessionId,
            SenderId = "ai",
            Content = chatResponse.Message,
            Role = "assistant",
            Images = uploadedUrls,
            OutfitSuggestions = chatResponse.OutfitSuggestions,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return response;
    }

    private async Task PersistSpecialMessageAsync(ChatCommand request, BaseCommandResponse response, string intent)
    {
        var sessionId = request.SessionId ?? Guid.NewGuid();

        // Create or update session
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            session = new Domain.Entities.ChatSession
            {
                Id = sessionId,
                UserId = request.UserId,
                Title = request.Message.Length > 100 ? request.Message[..100] : request.Message,
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

        // Save user message (images for user)
        await _sessionRepository.AddMessageAsync(new Domain.Entities.ChatMessage
        {
            SessionId = sessionId,
            SenderId = request.UserId,
            Content = request.Message,
            Role = "user",
            Intent = intent,
            Images = response.UploadedImageUrls is null
                ? null
                : JsonSerializer.Serialize(response.UploadedImageUrls),
            CreatedAt = DateTimeOffset.UtcNow
        });

        // Save AI response with metadata (metadata for ai)
        var metadataJson = response.Data != null ? JsonSerializer.Serialize(response.Data) : null;
        await _sessionRepository.AddMessageAsync(new Domain.Entities.ChatMessage
        {
            SessionId = sessionId,
            SenderId = "ai",
            Content = response.Message,
            Role = "assistant",
            Intent = "assistant",
            Metadata = metadataJson
        });

        // Set the session ID on the response
        response.Id = sessionId;
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

    private async Task<BaseCommandResponse> HandleCalendarAction(ChatCommand request, BaseCommandResponse response, string intent)
    {
    
                foreach(var itemId in request.ClothingItemIds)
                {
                    var clothingItem = await _clothingItemRepository.GetByIdAsync(itemId);
                    if (clothingItem != null && clothingItem.UserId == request.UserId)
                    {

                        await _wearEventRepository.AddAsync(new WearEvent
                        {
                            UserId = request.UserId,
                            ClothingItemId = clothingItem.Id,
                            WornAt = DateTimeOffset.UtcNow,
                            CreatedAt = DateTimeOffset.UtcNow
                        });
                        clothingItem.LastWorn = DateTimeOffset.UtcNow;
                        clothingItem.WearCount++;
                        clothingItem.UpdatedAt = DateTimeOffset.UtcNow;
                        await _unitOfWork.ClothingItems.UpdateAsync(clothingItem);
                    }
                    else
                    {
                        _logger.LogWarning($"Clothing item {itemId} not found or does not belong to user {request.UserId}");
                    }
                }
                foreach(var outfitId in request.OutfitIds)
                {
                    var outfit = await _unitOfWork.Outfits.GetByIdAsync(outfitId);
                    if (outfit != null && outfit.UserId == request.UserId)
                    {
                        await _wearEventRepository.AddAsync(new WearEvent
                        {
                            UserId = request.UserId,
                            OutfitId = outfit.Id,
                            WornAt = DateTimeOffset.UtcNow,
                            CreatedAt = DateTimeOffset.UtcNow
                        });
                        outfit.LastWorn = DateTimeOffset.UtcNow;
                        outfit.TimesWorn++;
                        outfit.UpdatedAt = DateTimeOffset.UtcNow;
                        await _unitOfWork.Outfits.UpdateAsync(outfit);
                        
                    }
                    else
                    {
                        _logger.LogWarning($"Outfit {outfitId} not found or does not belong to user {request.UserId}");
                    }
                }
               
                await _unitOfWork.SaveChangesAsync();
      
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
    Guid? sessionId = null;
    try { sessionId = Guid.Parse(request.SessionId?.ToString() ?? Guid.NewGuid().ToString()); }
    catch { sessionId = Guid.NewGuid(); }

    var validItems = new List<ClothingItem>();
    
    // Priority 1: Use outfit suggestion items from AI
    if (request.OutfitSuggestion != null && request.OutfitSuggestion.Any())
    {
        foreach (var suggestion in request.OutfitSuggestion)
        {
            foreach (var itemRef in suggestion.Items)
            {
                if (Guid.TryParse(itemRef.Id, out var itemId))
                {
                    var item = await _clothingItemRepository.GetByIdAsync(itemId);
                    if (item != null && item.UserId == request.UserId)
                    {
                        validItems.Add(item);
                    }
                }
            }
        }
    }
    // Priority 2: Use explicit ClothingItemIds
    else if (request.ClothingItemIds?.Any() == true)
    {
        foreach (var itemId in request.ClothingItemIds)
        {
            var item = await _clothingItemRepository.GetByIdAsync(itemId);
            if (item != null && item.UserId == request.UserId)
            {
                validItems.Add(item);
            }
        }
    }
    
    if (!validItems.Any())
    {
        response.Success = false;
        response.Message = "No valid wardrobe items to save";
        return response;
    }

    var outfitDto = new CreateOutfitDto
    {
        Name = "AI Suggested Outfit",
        Occasion = OccasionType.Casual.ToString(),
        Season = Season.Spring.ToString(),
        WeatherCondition = string.Empty,
        Items = validItems.Select((item, index) => new CreateOutfitItemDto
        {
            ClothingItemId = item.Id,
            Role = "Primary",
            LayeringOrder = index,
            IsEssential = true
        }).ToList()
    };

    var command = new CreateOutfitCommand { UserId = request.UserId, Request = outfitDto };
    var createOutfitResult = await _mediator.Send(command);

    if (createOutfitResult == null)
    {
        response.Success = false;
        response.Message = "Failed to save outfit";
        return response;
    }

    response.Success = true;
    response.Message = "Outfit saved to your wardrobe!";
    response.Data = new { type = "save_outfit", saved = true };
    response.Id = sessionId ?? Guid.NewGuid();
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