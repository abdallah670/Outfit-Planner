using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands.Validators;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Notifications.Handlers.Commands;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, BaseCommandResponse>
{
    private readonly ILogger<MarkAsReadCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAsReadCommandHandler(ILogger<MarkAsReadCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            // Validate the request
            var validationResult = await new MarkAsReadCommandValidator().ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for mark as read notification request for user with ID {UserId}", request.UserId);
                throw new Exceptions.ValidationException(validationResult);
            }

            // Check if user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found while attempting to mark notification as read", request.UserId);
                throw new NotFoundException("User", request.UserId);
            }

            // Check if notification exists
            var notification = await _unitOfWork.Notifications.GetByIdAsync(request.NotificationId);
            if (notification == null)
            {
                _logger.LogWarning("Notification with ID {NotificationId} not found", request.NotificationId);
                throw new NotFoundException("Notification", request.NotificationId);
            }

            // Check if notification belongs to the user
            if (notification.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to mark notification {NotificationId} as read belonging to another user", request.UserId, request.NotificationId);
                throw new Exceptions.UnauthorizedAccessException("You are not authorized to mark this notification as read");
            }

            // Check if already read
            if (notification.IsRead)
            {
                _logger.LogInformation("Notification {NotificationId} is already marked as read for user {UserId}", request.NotificationId, request.UserId);
                response.Success = true;
                response.Message = "Notification is already marked as read";
                response.Id = request.NotificationId;
                return response;
            }

            // Mark as read
            await _unitOfWork.Notifications.MarkAsReadAsync(request.NotificationId);

            response.Success = true;
            response.Message = "Notification marked as read successfully";
            response.Id = request.NotificationId;
            
            _logger.LogInformation("Notification {NotificationId} marked as read successfully for user {UserId}", request.NotificationId, request.UserId);
            
            return response;
        }
        catch (NotFoundException)
        {
            throw; // Re-throw NotFoundException to be handled by the API
        }
        catch (Exceptions.ValidationException)
        {
            throw; // Re-throw ValidationException to be handled by the API
        }
        catch (Exceptions.UnauthorizedAccessException)
        {
            throw; // Re-throw UnauthorizedAccessException to be handled by the API
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while marking notification {NotificationId} as read for user {UserId}", request.NotificationId, request.UserId);
            throw new BadRequestException("An error occurred while marking the notification as read. Please try again later.");
        }
    }
}
