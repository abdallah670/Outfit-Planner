using MediatR;
using OutfitPlanner.Application.DTOs.Outfit;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Queries;

public class GetOutfitByIdRequest : IRequest<OutfitDto>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
}
