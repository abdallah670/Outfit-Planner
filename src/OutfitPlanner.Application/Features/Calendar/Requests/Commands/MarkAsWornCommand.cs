using MediatR;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Calendar.Requests.Commands;

public class MarkAsWornCommand : IRequest<BaseCommandResponse>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public MarkAsWornRequest Request { get; set; } = new();
}
