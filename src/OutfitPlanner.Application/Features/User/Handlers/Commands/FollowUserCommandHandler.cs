using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<FollowUserCommandHandler> _logger;

    public FollowUserCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, ILogger<FollowUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        try{
        var response = new BaseCommandResponse();

        if (request.FollowerId == request.FollowingId)
        {
            response.Success = false;
            response.Message = "You cannot follow yourself";
           
            return response;
        }

        var alreadyFollowing = await _unitOfWork.Follows.IsFollowingAsync(request.FollowerId, request.FollowingId);
        if (alreadyFollowing)
        {
            response.Success = false;
            response.Message = "You are already following this user";
            return response;
        }

        var follow = new Follow
        {
            FollowerId = request.FollowerId,
            FollowedId = request.FollowingId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork.Follows.AddAsync(follow);
        await _unitOfWork.CompleteAsync();
        
        await _mediator.Send(new CreateNotificationCommand
        {
            UserId = request.FollowingId,
            Request = new CreateNotificationDto
            {
                Type = OutfitPlanner.Domain.Enums.NotificationType.Social,
                Title = "New Follower",
                Message = "Someone started following you.",
                ActionUrl = "/community"
            }
        });


       _logger.LogInformation("User {FollowerId} followed user {FollowingId}", request.FollowerId, request.FollowingId);
        response.Success = true;
        response.Message = "Successfully followed user";
        return response;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error occurred while trying to follow user {FollowerId} to {FollowingId}", request.FollowerId, request.FollowingId);
            return new BaseCommandResponse
            {
                Success = false,
                Message = "An error occurred while trying to follow the user. Please try again later."
            };
    }
    }
}
