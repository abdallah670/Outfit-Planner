using System.Text.Json;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Queries;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class ExportUserDataQueryHandler : IRequestHandler<ExportUserDataQuery, ExportUserDataResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IClothingItemRepository _clothingItemRepository;
    private readonly IOutfitRepository _outfitRepository;
    private readonly IWearEventRepository _wearEventRepository;
    private readonly IUserStyleProfileRepository _styleProfileRepository;
    private readonly IUserPreferencesRepository _userPreferencesRepository;

    public ExportUserDataQueryHandler(
        IUserRepository userRepository,
        IClothingItemRepository clothingItemRepository,
        IOutfitRepository outfitRepository,
        IWearEventRepository wearEventRepository,
        IUserStyleProfileRepository styleProfileRepository,
        IUserPreferencesRepository userPreferencesRepository)
    {
        _userRepository = userRepository;
        _clothingItemRepository = clothingItemRepository;
        _outfitRepository = outfitRepository;
        _wearEventRepository = wearEventRepository;
        _styleProfileRepository = styleProfileRepository;
        _userPreferencesRepository = userPreferencesRepository;
    }

    public async Task<ExportUserDataResult> Handle(ExportUserDataQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        
        if (user == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        var clothingItems = await _clothingItemRepository.FindAsync(c => c.UserId == request.UserId);
        var outfits = await _outfitRepository.FindAsync(o => o.UserId == request.UserId);
        var wearEvents = await _wearEventRepository.FindAsync(w => w.UserId == request.UserId);
        var styleProfile = await _styleProfileRepository.GetByUserIdAsync(request.UserId);
        var preferences = await _userPreferencesRepository.GetByUserIdAsync(request.UserId);

        var exportData = new
        {
            ExportedAt = DateTime.UtcNow,
            User = new
            {
                user.Name,
                user.Email,
                user.Provider,
                user.CreatedAt,
                user.LastLogin
            },
            StyleProfile = styleProfile != null ? new
            {
                styleProfile.Style,
                styleProfile.PreferredColors,
                styleProfile.FitPreferences,
                styleProfile.ComfortPriority,
                styleProfile.AcceptsTrends
            } : null,
            Preferences = preferences != null ? new
            {
                preferences.ShareOutfitsAnonymously,
                preferences.IncludeInTrendAnalysis,
                preferences.AllowFriendRequests,
                preferences.DefaultOutfitPrivacy,
                preferences.ShowBodyMetrics,
                preferences.AllowLocationTracking
            } : null,
            Wardrobe = clothingItems.Select(c => new
            {
                c.Name,
                c.Category,
                c.PrimaryColor,
                c.Brand,
                c.PurchasePrice,
                c.PurchaseDate,
                c.ImageUrl,
                c.WearCount
            }),
            Outfits = outfits.Select(o => new
            {
                o.Name,
                o.Occasion,
                o.Season,
                o.ImageUrl,
                o.TimesWorn,
                Items = o.Items?.Select(i => new { i.ClothingItemId })
            }),
            WearHistory = wearEvents.Select(w => new
            {
                w.WornAt,
                w.OutfitId,
                w.Notes,
                w.WeatherCondition,
                w.Rating
            }),
            Statistics = new
            {
                TotalClothingItems = clothingItems.Count(),
                TotalOutfits = outfits.Count(),
                TotalWearEvents = wearEvents.Count(),
                AverageTimesWorn = clothingItems.Any() ? clothingItems.Average(c => c.WearCount) : 0
            }
        };

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return new ExportUserDataResult
        {
            Data = System.Text.Encoding.UTF8.GetBytes(json)
        };
    }
}
