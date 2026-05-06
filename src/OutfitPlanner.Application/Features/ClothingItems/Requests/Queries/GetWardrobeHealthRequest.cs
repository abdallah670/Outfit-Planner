using MediatR;
using OutfitPlanner.Application.DTOs.Wardrobe;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Queries;

public class GetWardrobeHealthRequest : IRequest<WardrobeHealthDto>
{
    public string UserId { get; set; } = string.Empty;
}
