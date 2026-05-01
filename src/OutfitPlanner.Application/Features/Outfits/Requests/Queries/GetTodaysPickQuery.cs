using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Queries;

public class GetTodaysPickQuery : IRequest<TodaysPickResult>
{
    public string UserId { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTimeOffset? Date { get; set; }
}


