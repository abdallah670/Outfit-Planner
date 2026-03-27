using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Calendar.Requests.Commands;

public class DeleteWearEventCommand : IRequest<BaseCommandResponse>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
}
