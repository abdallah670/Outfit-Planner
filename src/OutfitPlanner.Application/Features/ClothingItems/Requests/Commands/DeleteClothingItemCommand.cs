using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;

public class DeleteClothingItemCommand : IRequest<BaseCommandResponse>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
}
