using MediatR;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Application.DTOs.Outfit;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Queries;

public class GetFilteredOutfitsRequest : IRequest<PagedResult<OutfitDto>>
{
    public string UserId { get; set; } = string.Empty;
    public string? Occasion { get; set; }
    public string? SearchQuery { get; set; }
    public string? SortBy { get; set; } // "recent", "mostWorn", "name"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Season { get; set; }
}
