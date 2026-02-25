using MediatR;
using OutfitPlanner.Application.DTOs.Wardrobe;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Queries{

public class GetClothingItemByIdRequest : IRequest<ClothingItemDto>
{
    public Guid Id { get; set; }
        public string? UserId { get; set; }
    }
}