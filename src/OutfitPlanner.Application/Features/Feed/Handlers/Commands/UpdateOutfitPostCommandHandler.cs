using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

/// <summary>
/// Handler for UpdateOutfitPostCommand
/// </summary>
public class UpdateOutfitPostCommandHandler : IRequestHandler<UpdateOutfitPostCommand, BaseCommandResponse>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOutfitPostCommandHandler(
        IFeedPostRepository feedPostRepository,
        IUnitOfWork unitOfWork)
    {
        _feedPostRepository = feedPostRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(UpdateOutfitPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _feedPostRepository.GetByIdAsync(request.PostId);

        if (post == null)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "Post not found"
            };
        }

        // Verify ownership
        if (post.UserId != request.UserId)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "You can only update your own posts"
            };
        }

        // Update fields
        if (request.Caption != null)
            post.Caption = request.Caption;
        
        post.Visibility = request.Visibility;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        await _feedPostRepository.UpdateAsync(post);
        await _unitOfWork.SaveChangesAsync();

        return new BaseCommandResponse
        {
            Success = true,
            Id = post.Id,
            Message = "Post updated successfully"
        };
    }
}
