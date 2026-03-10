using MediatR;
using OutfitPlanner.Application.DTOs.Calendar;

namespace OutfitPlanner.Application.Features.Calendar.Requests.Queries;

public class GetMonthlyStatsRequest : IRequest<MonthlyStatsDto>
{
    public string UserId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
}
