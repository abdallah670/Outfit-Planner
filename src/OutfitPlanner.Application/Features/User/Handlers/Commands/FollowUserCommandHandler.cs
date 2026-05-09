using MediatR;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, BaseCommandResponse>
{
    private readonly IFollowRepository _followRepository;

    public FollowUserCommandHandler(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    public async Task<BaseCommandResponse> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        if (request.FollowerId == request.FollowingId)
        {
            response.Success = false;
            response.Message = "You cannot follow yourself";
            return response;
        }

        var alreadyFollowing = await _followRepository.IsFollowingAsync(request.FollowerId, request.FollowingId);
        if (alreadyFollowing)
        {
            response.Success = false;
            response.Message = "You are already following this user";
            return response;
        }

        var follow = new Follow
        {
            FollowerId = request.FollowerId,
            FollowingId = request.FollowingId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _followRepository.AddAsync(follow);

        response.Success = true;
        response.Message = "Successfully followed user";
        return response;
    }
}
