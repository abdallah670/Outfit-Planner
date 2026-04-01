using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands;

/// <summary>
/// Command to delete a poll post
/// </summary>
public class DeletePollPostCommand : IRequest<BaseCommandResponse>
{
    public Guid PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
