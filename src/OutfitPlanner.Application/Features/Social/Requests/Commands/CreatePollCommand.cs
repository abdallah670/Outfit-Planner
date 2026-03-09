using MediatR;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Requests.Commands;

/// <summary>
/// Command to create a new validation poll
/// </summary>
public class CreatePollCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public CreatePollDto Request { get; set; } = new();
}
