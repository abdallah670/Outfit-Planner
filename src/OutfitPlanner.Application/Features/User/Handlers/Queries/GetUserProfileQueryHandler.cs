using MediatR;
using Microsoft.AspNetCore.Identity;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Queries;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IClothingItemRepository _clothingItemRepository;
    private readonly IOutfitRepository _outfitRepository;
    private readonly IWearEventRepository _wearEventRepository;
    private readonly IUserStyleProfileRepository _styleProfileRepository;
    private readonly IUserPreferencesRepository _preferencesRepository;

    public GetUserProfileQueryHandler(
        UserManager<Domain.Entities.User> userManager,
        IClothingItemRepository clothingItemRepository,
        IOutfitRepository outfitRepository,
        IWearEventRepository wearEventRepository,
        IUserStyleProfileRepository styleProfileRepository,
        IUserPreferencesRepository preferencesRepository)
    {
        _userManager = userManager;
        _clothingItemRepository = clothingItemRepository;
        _outfitRepository = outfitRepository;
        _wearEventRepository = wearEventRepository;
        _styleProfileRepository = styleProfileRepository;
        _preferencesRepository = preferencesRepository;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);
        }

        // Load related entities separately
        var styleProfile = await _styleProfileRepository.GetByUserIdAsync(request.UserId);
        var preferences = await _preferencesRepository.GetByUserIdAsync(request.UserId);

        // Get user stats
        var wardrobeItems = await _clothingItemRepository.GetByUserIdAsync(request.UserId);
        var outfits = await _outfitRepository.GetByUserIdAsync(request.UserId);
        var wearEvents = await _wearEventRepository.GetByUserIdAsync(request.UserId);

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin,
            WardrobeItemCount = wardrobeItems.Count(),
            OutfitCount = outfits.Count(),
            TotalWears = wearEvents.Count(),
            StyleProfile = styleProfile != null ? new UserStyleProfileDto
            {
                Style = styleProfile.Style,
                PreferredColors = styleProfile.PreferredColors,
                FitPreferences = styleProfile.FitPreferences,
                ComfortPriority = styleProfile.ComfortPriority,
                AcceptsTrends = styleProfile.AcceptsTrends
            } : null,
            Preferences = preferences != null ? new UserPreferencesDto
            {
                ShareOutfitsAnonymously = preferences.ShareOutfitsAnonymously,
                IncludeInTrendAnalysis = preferences.IncludeInTrendAnalysis,
                AllowFriendRequests = preferences.AllowFriendRequests,
                DefaultOutfitPrivacy = preferences.DefaultOutfitPrivacy,
                ShowBodyMetrics = preferences.ShowBodyMetrics,
                AllowLocationTracking = preferences.AllowLocationTracking
            } : null
        };
    }
}
