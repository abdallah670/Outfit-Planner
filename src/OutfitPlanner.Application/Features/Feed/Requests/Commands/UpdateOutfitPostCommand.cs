using MediatR;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands;

/// <summary>
/// Command to update an outfit post
/// </summary>
public class UpdateOutfitPostCommand : IRequest<BaseCommandResponse>
{
    public Guid PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public Visibility Visibility { get; set; }
    public List<string> Tags { get; set; }  
    public Guid OutfitId { get; set; }  
}
