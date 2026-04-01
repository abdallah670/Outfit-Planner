using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.DTOs.Weather;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Outfits.Requests.Queries;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Queries;

public class GetTodaysPickQueryHandler
    : IRequestHandler<GetTodaysPickQuery, TodaysPickResult>
{
    private readonly IOutfitRepository _outfitRepository;
    private readonly IClothingItemRepository _clothingItemRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWeatherService _weatherService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTodaysPickQueryHandler> _logger;

    private static readonly Dictionary<Domain.Enums.CalendarEventType, OccasionType> EventToOccasionMap = new()
    {
        { Domain.Enums.CalendarEventType.Work, OccasionType.BusinessCasual },
        { Domain.Enums.CalendarEventType.Meeting, OccasionType.BusinessCasual },
        { Domain.Enums.CalendarEventType.Social, OccasionType.Social },
        { Domain.Enums.CalendarEventType.Date, OccasionType.Date },
        { Domain.Enums.CalendarEventType.Party, OccasionType.Social },
        { Domain.Enums.CalendarEventType.Sport, OccasionType.Athletic },
        { Domain.Enums.CalendarEventType.Travel, OccasionType.Travel },
        { Domain.Enums.CalendarEventType.Appointment, OccasionType.Casual },
        { Domain.Enums.CalendarEventType.General, OccasionType.Casual }
    };

    private static readonly Dictionary<string, Season> WeatherToSeasonMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Clear", Season.Summer },
        { "Sunny", Season.Summer },
        { "Clouds", Season.Spring },
        { "Rain", Season.Autumn },
        { "Drizzle", Season.Autumn },
        { "Thunderstorm", Season.Autumn },
        { "Snow", Season.Winter },
        { "Mist", Season.Autumn },
        { "Fog", Season.Autumn }
    };

    private static readonly Dictionary<string, string> WeatherToConditionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Clear", "Sunny" },
        { "Sunny", "Sunny" },
        { "Clouds", "Cloudy" },
        { "Rain", "Rainy" },
        { "Drizzle", "Rainy" },
        { "Thunderstorm", "Stormy" },
        { "Snow", "Snowy" },
        { "Mist", "Foggy" },
        { "Fog", "Foggy" }
    };

    public GetTodaysPickQueryHandler(
        IOutfitRepository outfitRepository,
        IClothingItemRepository clothingItemRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IWeatherService weatherService,
        IMapper mapper,
        ILogger<GetTodaysPickQueryHandler> logger)
    {
        _outfitRepository = outfitRepository;
        _clothingItemRepository = clothingItemRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _weatherService = weatherService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TodaysPickResult> Handle(GetTodaysPickQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);

        try
        {
            _logger.LogInformation("Generating today's pick for user {UserId}", request.UserId);

            var weatherContext = await GetWeatherContextAsync(request.Latitude, request.Longitude, cancellationToken);
            var todayEvent = await GetTodayEventAsync(request.UserId, cancellationToken);
            var styleProfile = await GetUserStyleProfileAsync(request.UserId, cancellationToken);

            var outfit = await FindBestOutfitAsync(
                request.UserId,
                weatherContext,
                todayEvent,
                styleProfile,
                cancellationToken);

            if (outfit == null)
            {
                return new TodaysPickResult
                {
                    Outfit = null,
                    WeatherContext = weatherContext,
                    TodayEvent = todayEvent,
                    MatchScore = 0,
                    RecommendationReason = "No outfits found in your wardrobe",
                    IsBestEffort = false
                };
            }

            var score = CalculateMatchScore(outfit, weatherContext, todayEvent, styleProfile);
            var isBestEffort = score < 5;
            var reason = GenerateRecommendationReason(outfit, weatherContext, todayEvent, score);

            return new TodaysPickResult
            {
                Outfit = _mapper.Map<OutfitDto>(outfit),
                WeatherContext = weatherContext,
                TodayEvent = todayEvent,
                MatchScore = score,
                RecommendationReason = reason,
                IsBestEffort = isBestEffort
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation cancelled while generating today's pick for user {UserId}", request.UserId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate today's pick for user {UserId}", request.UserId);
            throw new ApplicationException("Failed to generate today's pick. Please try again later.", ex);
        }
    }

    private async Task<WeatherContextDto?> GetWeatherContextAsync(double? latitude, double? longitude, CancellationToken cancellationToken)
    {
        try
        {
            WeatherDto? weather;

            if (latitude.HasValue && longitude.HasValue)
            {
                weather = await _weatherService.GetCurrentWeatherAsync(latitude: latitude.Value, longitude: longitude.Value, cancellationToken: cancellationToken);
            }
            else
            {
                weather = await _weatherService.GetCurrentWeatherAsync(city: "Cairo", cancellationToken: cancellationToken);
            }

            if (weather == null) return null;

            var condition = WeatherToConditionMap.GetValueOrDefault(weather.Condition, "Clear");
            var season = WeatherToSeasonMap.GetValueOrDefault(weather.Condition, Season.Spring);

            return new WeatherContextDto
            {
                Condition = condition,
                Temperature = weather.Temperature,
                Season = season.ToString(),
                City = weather.City
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get weather context, using defaults");
            return new WeatherContextDto
            {
                Condition = "Clear",
                Temperature = 22,
                Season = "Spring",
                City = "Cairo"
            };
        }
    }

    private async Task<TodayEventDto?> GetTodayEventAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTimeOffset.Now.Date;
            var events = await _unitOfWork.CalendarEvents.GetByUserIdAndDateAsync(userId, today);

            var firstEvent = events
                .OrderBy(e => e.StartTime)
                .FirstOrDefault();

            if (firstEvent == null) return null;

            return new TodayEventDto
            {
                Title = firstEvent.Title,
                EventType = firstEvent.EventType.ToString(),
                EventDate = firstEvent.EventDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get today's calendar events");
            return null;
        }
    }

    private async Task<UserStyleProfile?> GetUserStyleProfileAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.StyleProfile;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get user style profile");
            return null;
        }
    }

    private async Task<Outfit?> FindBestOutfitAsync(
        string userId,
        WeatherContextDto? weather,
        TodayEventDto? todayEvent,
        UserStyleProfile? styleProfile,
        CancellationToken cancellationToken)
    {
        var outfits = await _outfitRepository.GetByUserIdAsync(userId);

        var validOutfits = outfits
            .Where(o => o.Status == OutfitStatus.Active)
            .ToList();

        if (!validOutfits.Any())
        {
            return null;
        }

        var threeDaysAgo = DateTimeOffset.Now.AddDays(-3);
        var allClothingItems = await _clothingItemRepository.GetByUserIdAsync(userId);
        var clothingItemsDict = allClothingItems.ToDictionary(c => c.Id);

        var scoredOutfits = validOutfits
            .Select(o => new
            {
                Outfit = o,
                Score = CalculateRelevanceScore(o, weather, todayEvent, styleProfile, threeDaysAgo, clothingItemsDict)
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Outfit.TimesWorn > 0)
            .ThenByDescending(x => x.Outfit.LastWorn ?? DateTimeOffset.MinValue)
            .ToList();

        return scoredOutfits.FirstOrDefault()?.Outfit;
    }

    private int CalculateRelevanceScore(
        Outfit outfit,
        WeatherContextDto? weather,
        TodayEventDto? todayEvent,
        UserStyleProfile? styleProfile,
        DateTimeOffset excludeWornAfter,
        Dictionary<Guid, ClothingItem> clothingItemsDict)
    {
        var score = 0;

        if (weather != null)
        {
            if (outfit.WeatherCondition?.Equals(weather.Condition, StringComparison.OrdinalIgnoreCase) == true)
                score += 3;

            if (outfit.Season.ToString().Equals(weather.Season, StringComparison.OrdinalIgnoreCase))
                score += 2;
        }

        if (todayEvent != null)
        {
            if (Enum.TryParse<Domain.Enums.CalendarEventType>(todayEvent.EventType, out var eventType))
            {
                var occasion = EventToOccasionMap.GetValueOrDefault(eventType, OccasionType.Casual);
                if (outfit.Occasion == occasion)
                    score += 3;
            }
        }

        if (styleProfile?.PreferredColors != null && styleProfile.PreferredColors.Any())
        {
            var outfitItemIds = outfit.Items.Select(i => i.ClothingItemId).ToList();
            var outfitColors = outfitItemIds
                .Where(id => clothingItemsDict.ContainsKey(id))
                .Select(id => clothingItemsDict[id])
                .SelectMany(c => new[] { c.PrimaryColor }.Concat(c.SecondaryColors ?? new List<string>()))
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();

            var colorMatches = outfitColors
                .Where(c => styleProfile.PreferredColors.Any(pc =>
                    c.Equals(pc, StringComparison.OrdinalIgnoreCase)))
                .Count();

            if (outfitColors.Any())
            {
                score += (int)((double)colorMatches / outfitColors.Count * 2);
            }
        }

        if (outfit.LastWorn == null || outfit.LastWorn < excludeWornAfter)
        {
            score += 1;
        }

        return score;
    }

    private int CalculateMatchScore(
        Outfit outfit,
        WeatherContextDto? weather,
        TodayEventDto? todayEvent,
        UserStyleProfile? styleProfile)
    {
        var maxScore = 10;
        var actualScore = 0;

        if (weather != null)
        {
            if (outfit.WeatherCondition?.Equals(weather.Condition, StringComparison.OrdinalIgnoreCase) == true)
                actualScore += 3;
            if (outfit.Season.ToString().Equals(weather.Season, StringComparison.OrdinalIgnoreCase))
                actualScore += 2;
        }

        if (todayEvent != null)
        {
            if (Enum.TryParse<Domain.Enums.CalendarEventType>(todayEvent.EventType, out var eventType))
            {
                var occasion = EventToOccasionMap.GetValueOrDefault(eventType, OccasionType.Casual);
                if (outfit.Occasion == occasion)
                    actualScore += 3;
            }
        }

        if (styleProfile?.PreferredColors != null && styleProfile.PreferredColors.Any())
        {
            actualScore += 2;
        }

        return (int)((double)actualScore / maxScore * 100);
    }

    private string GenerateRecommendationReason(
        Outfit outfit,
        WeatherContextDto? weather,
        TodayEventDto? todayEvent,
        int score)
    {
        var reasons = new List<string>();

        if (weather != null)
        {
            if (outfit.WeatherCondition?.Equals(weather.Condition, StringComparison.OrdinalIgnoreCase) == true)
                reasons.Add($"perfect for {weather.Condition.ToLower()} weather");
            else if (outfit.Season.ToString().Equals(weather.Season, StringComparison.OrdinalIgnoreCase))
                reasons.Add($"ideal for {weather.Season.ToLower()}");
        }

        if (todayEvent != null)
        {
            reasons.Add($"great for your {todayEvent.Title} event");
        }

        if (!reasons.Any())
        {
            return score > 5 ? "A versatile choice for today" : "Based on your wardrobe options";
        }

        return string.Join(" and ", reasons);
    }
}