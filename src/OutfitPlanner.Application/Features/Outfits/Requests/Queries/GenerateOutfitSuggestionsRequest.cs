using MediatR;
using OutfitPlanner.Application.DTOs.Outfit;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Queries;

/// <summary>
/// Query to generate outfit suggestions based on user preferences,
/// occasion, season, and weather conditions.
/// </summary>
public class GenerateOutfitSuggestionsQuery : IRequest<List<OutfitDto>>
{
    public string UserId { get; set; } = string.Empty;
    public OutfitSuggestionsDto OutfitSuggestionsDto { get; set; } = new OutfitSuggestionsDto();
   
}
