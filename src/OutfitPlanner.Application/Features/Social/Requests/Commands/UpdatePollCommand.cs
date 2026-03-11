using MediatR;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Requests.Commands;

public class UpdatePollCommand : IRequest<BaseCommandResponse>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UpdatePollRequest Request { get; set; } = new();
}
