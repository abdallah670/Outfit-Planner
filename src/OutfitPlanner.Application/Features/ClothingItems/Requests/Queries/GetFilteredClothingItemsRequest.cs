using MediatR;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Queries;

public class GetFilteredClothingItemsRequest : IRequest<PagedResult<ClothingItemListDto>>
{
    public string UserId { get; set; } = string.Empty;
    public string? Category { get; set; }         // e.g. "Casual", "Formal" (stored as string)
    public string? Color { get; set; }            // e.g. "Blue", "Red" (color name)
    public string? Condition { get; set; }        // e.g. "good", "excellent"
    public string? Fabric { get; set; }           // FabricType enum name
    public string? Size { get; set; }             // e.g. "M", "L"
    public string? Type { get; set; }             // ClothingType enum name (Top, Bottom, etc.)
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SearchQuery { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
