using MediatR;
using OutfitPlanner.Application.DTOs.Wardrobe;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Queries;

public class GetClothingItemsByCategoryRequest : IRequest<List<ClothingItemListDto>>
{
    public string UserId { get; set; }
    public string Category { get; set; }
}
