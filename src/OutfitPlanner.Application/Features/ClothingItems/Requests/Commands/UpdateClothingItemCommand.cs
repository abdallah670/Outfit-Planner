using MediatR;
using OutfitPlanner.Application.DTOs.Wardrobe;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;

public class UpdateClothingItemCommand : IRequest<ClothingItemDto>
{
    public Guid Id { get; set; }
    public UpdateClothingItemDto? Request { get; set; }
    public string? UserId { get;  set; }
}
