using MediatR;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands;

public class CreateOutfitPostCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public Guid OutfitId { get; set; }
    public string? Caption { get; set; }
    public List<string> Tags { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Public;
}
