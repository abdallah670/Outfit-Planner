using MediatR;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Application.DTOs.Wardrobe;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Queries;

public class GetFilteredClothingItemsRequest : IRequest<PagedResult<ClothingItemListDto>>
{
    public string UserId { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Color { get; set; }
    public string? Occasion { get; set; }
    public string? SearchQuery { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
