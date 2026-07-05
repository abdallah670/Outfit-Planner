using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.AI.Requests.Commands;

public class DeleteSessionCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public Guid SessionId { get; set; }
}