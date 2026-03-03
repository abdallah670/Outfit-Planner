using MediatR;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Application.DTOs.Outfit;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Commands;

public class RecordOutfitWearCommand : IRequest<OutfitDto>
{
    public string UserId { get; set; } = string.Empty;
    public Guid OutfitId { get; set; }
    public DateTimeOffset WornAt { get; set; }
    public string? WeatherCondition { get; set; }
    public Guid? EventId { get; set; }
}
