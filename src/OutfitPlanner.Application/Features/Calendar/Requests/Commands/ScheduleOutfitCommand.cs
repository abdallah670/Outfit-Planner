using MediatR;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Calendar.Requests.Commands;

public class ScheduleOutfitCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public ScheduleOutfitRequest Request { get; set; } = new();
}
