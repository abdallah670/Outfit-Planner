using MediatR;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands;

public class CreatePollPostCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<Guid> OutfitIds { get; set; } = new();
    public DateTimeOffset ExpiresAt { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Public;
}
