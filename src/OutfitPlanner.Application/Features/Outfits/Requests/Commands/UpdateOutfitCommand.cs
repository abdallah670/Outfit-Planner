using MediatR;
using OutfitPlanner.Application.DTOs.Outfit;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Commands;

public class UpdateOutfitCommand : IRequest<OutfitDto>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UpdateOutfitDto Request { get; set; } = null!;
}
