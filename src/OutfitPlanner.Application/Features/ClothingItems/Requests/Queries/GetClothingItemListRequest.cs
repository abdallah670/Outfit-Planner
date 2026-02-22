using MediatR;
using OutfitPlanner.Application.DTOs.Wardrobe;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Queries{

public class GetClothingItemListRequest : IRequest<List<ClothingItemDto>>
{
   public string UserId{set;get;}

}
}