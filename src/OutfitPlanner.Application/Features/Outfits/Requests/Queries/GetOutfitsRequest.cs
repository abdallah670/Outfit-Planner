using MediatR;
using OutfitPlanner.Application.DTOs.Outfit;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Queries;

public class GetOutfitsRequest : IRequest<List<OutfitListDto>>
{
    public string UserId { get; set; } = string.Empty;
}
