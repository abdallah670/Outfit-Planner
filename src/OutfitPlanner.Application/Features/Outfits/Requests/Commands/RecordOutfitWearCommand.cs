using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Commands;

public class RecordOutfitWearCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public Guid OutfitId { get; set; }
    public DateTimeOffset WornAt { get; set; }
    public string? WeatherCondition { get; set; }
    public Guid? EventId { get; set; }
}
