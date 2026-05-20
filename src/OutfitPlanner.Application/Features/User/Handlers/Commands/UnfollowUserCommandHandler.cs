using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FollowUserCommandHandler> _logger;
        public UnfollowUserCommandHandler(IUnitOfWork unitOfWork, ILogger<FollowUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();
        if (request.FollowerId == request.FollowingId)
        {
            response.Success = false;
            response.Message = "You cannot unfollow yourself";
            return response;
        }
        var follows = await _unitOfWork.Follows.FindAsync(f => f.FollowerId == request.FollowerId && f.FollowedId == request.FollowingId);
        var follow = follows.FirstOrDefault();

        if (follow == null)
        {
            response.Success = false;
            response.Message = "You are not following this user";
            return response;
        }

        await _unitOfWork.Follows.RemoveAsync(follow);
        await _unitOfWork.CompleteAsync();

        response.Success = true;
        response.Message = "Successfully unfollowed user";
        return response;
    }
}
