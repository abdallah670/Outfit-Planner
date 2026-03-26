using MediatR;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Requests.Commands;

public class AddOutfitCommentCommand : IRequest<BaseCommandResponse>
{
    public Guid OutfitId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
}

public class DeleteOutfitCommentCommand : IRequest<BaseCommandResponse>
{
    public Guid CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
