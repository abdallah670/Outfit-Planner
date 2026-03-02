using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Outfits.Requests.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Queries;

/// <summary>
/// Handler for generating outfit suggestions based on user preferences,
/// occasion, season, and weather conditions.
/// </summary>
public class GenerateOutfitSuggestionsQueryHandler
    : IRequestHandler<GenerateOutfitSuggestionsQuery, List<OutfitDto>>
{
    private readonly IOutfitRepository _outfitRepository;
    private readonly IClothingItemRepository _clothingItemRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GenerateOutfitSuggestionsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the handler with required dependencies.
    /// </summary>
    public GenerateOutfitSuggestionsQueryHandler(
        IOutfitRepository outfitRepository,
        IClothingItemRepository clothingItemRepository,
        IMapper mapper,
        ILogger<GenerateOutfitSuggestionsQueryHandler> logger)
    {
        _outfitRepository = outfitRepository ?? throw new ArgumentNullException(nameof(outfitRepository));
        _clothingItemRepository = clothingItemRepository ?? throw new ArgumentNullException(nameof(clothingItemRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the query to generate outfit suggestions.
    /// </summary>
    public async Task<List<OutfitDto>> Handle(
        GenerateOutfitSuggestionsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);

        try
        {
            _logger.LogInformation(
                "Generating outfit suggestions for user {UserId} with filters: Occasion={Occasion}, Season={Season}, Weather={Weather}",
                request.UserId,
                request.OutfitSuggestionsDto.Occasion ?? "any",
                request.OutfitSuggestionsDto.Season ?? "any",
                request.OutfitSuggestionsDto.WeatherCondition ?? "any");

            // Get user's clothing items for potential outfit generation
            var userClothingItems = await _clothingItemRepository
                .GetByUserIdAsync(request.UserId)
                .ConfigureAwait(false);

            // Get existing outfits that match the criteria
            var outfits = await _outfitRepository
                .GetByUserIdAsync(request.UserId)
                .ConfigureAwait(false);

            // Filter outfits based on criteria if provided
            var filteredOutfits = FilterOutfitsByCriteria(outfits, request.OutfitSuggestionsDto);

            if (!filteredOutfits.Any())
            {
                _logger.LogInformation(
                    "No matching outfits found for user {UserId} with specified criteria",
                    request.UserId);
                return new List<OutfitDto>();
            }

            // Sort by relevance: prioritize outfits that match more criteria
            var sortedOutfits = SortOutfitsByRelevance(filteredOutfits, request.OutfitSuggestionsDto);

            // Limit results to top suggestions
            var suggestions = sortedOutfits
                .Take(request.OutfitSuggestionsDto.MaxSuggestions)
                .ToList();

            _logger.LogInformation(
                "Generated {Count} outfit suggestions for user {UserId}",
                suggestions.Count,
                request.UserId);

            return _mapper.Map<List<OutfitDto>>(suggestions);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Operation cancelled while generating outfit suggestions for user {UserId}",
                request.UserId);
            throw;
        }
        catch (Exception ex) when (ex is not ApplicationException)
        {
            _logger.LogError(
                ex,
                "Failed to generate outfit suggestions for user {UserId}",
                request.UserId);

            throw new ApplicationException(
                $"Failed to generate outfit suggestions for user {request.UserId}. " +
                "Please try again later.", ex);
        }
    }

    /// <summary>
    /// Filters outfits based on the provided criteria.
    /// </summary>
    private static IEnumerable<Outfit> FilterOutfitsByCriteria(
        IEnumerable<Outfit> outfits,
        OutfitSuggestionsDto criteria)
    {
        var query = outfits;

        if (!string.IsNullOrWhiteSpace(criteria.Occasion))
        {
            query = query.Where(o =>
                o.Occasion.ToString().Equals(criteria.Occasion, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Season))
        {
            query = query.Where(o =>
                o.Season.ToString().Equals(criteria.Season, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.WeatherCondition))
        {
            query = query.Where(o =>
                o.WeatherCondition.Equals(criteria.WeatherCondition, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }

    /// <summary>
    /// Sorts outfits by relevance score based on how many criteria they match.
    /// </summary>
    private static IEnumerable<Outfit> SortOutfitsByRelevance(
        IEnumerable<Outfit> outfits,
        OutfitSuggestionsDto criteria)
    {
        return outfits
            .Select(o => new { Outfit = o, Score = CalculateRelevanceScore(o, criteria) })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Outfit.TimesWorn) // Prefer frequently worn outfits
            .ThenByDescending(x => x.Outfit.LastWorn)   // Prefer recently worn outfits
            .Select(x => x.Outfit);
    }

    /// <summary>
    /// Calculates a relevance score for an outfit based on matching criteria.
    /// </summary>
    private static int CalculateRelevanceScore(Outfit outfit, OutfitSuggestionsDto criteria)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(criteria.Occasion) &&
            outfit.Occasion.ToString().Equals(criteria.Occasion, StringComparison.OrdinalIgnoreCase))
        {
            score += 3;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Season) &&
            outfit.Season.ToString().Equals(criteria.Season, StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }

        if (!string.IsNullOrWhiteSpace(criteria.WeatherCondition) &&
            outfit.WeatherCondition.Equals(criteria.WeatherCondition, StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }

        return score;
    }
}
