using MediatR;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;

public class CreateClothingItemCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public CreateClothingItemDto Request { get; set; }
}
