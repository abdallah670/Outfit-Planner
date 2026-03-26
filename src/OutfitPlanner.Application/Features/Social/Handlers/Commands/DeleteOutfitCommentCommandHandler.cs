using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

public class DeleteOutfitCommentCommandHandler : IRequestHandler<DeleteOutfitCommentCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOutfitCommentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(DeleteOutfitCommentCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var success = await _unitOfWork.OutfitEngagement.SoftDeleteCommentAsync(request.CommentId, request.UserId);

        if (!success)
        {
            response.Success = false;
            response.Message = "Comment not found or you do not have permission to delete it";
            response.Errors = new List<string> { "Comment not found or you do not have permission to delete it" };
            return response;
        }

        response.Success = true;
        response.Message = "Comment deleted successfully";
        return response;
    }
}
