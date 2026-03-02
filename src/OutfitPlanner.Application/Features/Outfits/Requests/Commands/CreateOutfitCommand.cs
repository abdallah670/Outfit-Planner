using MediatR;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Commands;

public class CreateOutfitCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public CreateOutfitDto Request { get; set; } = null!;
}
