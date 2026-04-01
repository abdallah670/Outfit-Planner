using MediatR;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, BaseCommandResponse>
{
    private readonly IFollowRepository _followRepository;

    public UnfollowUserCommandHandler(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    public async Task<BaseCommandResponse> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var follows = await _followRepository.FindAsync(f => f.FollowerId == request.FollowerId && f.FollowingId == request.FollowingId);
        var follow = follows.FirstOrDefault();

        if (follow == null)
        {
            response.Success = false;
            response.Message = "You are not following this user";
            return response;
        }

        await _followRepository.RemoveAsync(follow);

        response.Success = true;
        response.Message = "Successfully unfollowed user";
        return response;
    }
}