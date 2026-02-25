using MediatR;
using OutfitPlanner.Application.DTOs.Wardrobe;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Queries
{
    public class GetClothingItemListRequest : IRequest<List<ClothingItemListDto>>
    {
        
        public string UserId { get; set; }
    }
}