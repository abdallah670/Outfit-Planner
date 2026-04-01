using MediatR;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands;

/// <summary>
/// Command to update a poll post
/// </summary>
public class UpdatePollPostCommand : IRequest<BaseCommandResponse>
{
    public Guid PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Question { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public Visibility Visibility { get; set; }
}
