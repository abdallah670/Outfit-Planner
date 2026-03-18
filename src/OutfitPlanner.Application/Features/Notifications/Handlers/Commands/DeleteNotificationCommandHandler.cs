using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands.Validators;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Notifications.Handlers.Commands;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, BaseCommandResponse>
{
    private readonly ILogger<DeleteNotificationCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNotificationCommandHandler(ILogger<DeleteNotificationCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            // Validate the request
            var validationResult = await new DeleteNotificationCommandValidator().ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for delete notification request for user with ID {UserId}", request.UserId);
                throw new Exceptions.ValidationException(validationResult);
            }

            // Check if user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found while attempting to delete notification", request.UserId);
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
                _logger.LogWarning("User {UserId} attempted to delete notification {NotificationId} belonging to another user", request.UserId, request.NotificationId);
                throw new Exceptions.UnauthorizedAccessException("You are not authorized to delete this notification");
            }

            // Delete the notification
            await _unitOfWork.Notifications.DeleteAsync(notification.Id);

            response.Success = true;
            response.Message = "Notification deleted successfully";
            response.Id = request.NotificationId;
            
            _logger.LogInformation("Notification {NotificationId} deleted successfully for user {UserId}", request.NotificationId, request.UserId);
            
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
            _logger.LogError(ex, "An error occurred while deleting notification {NotificationId} for user {UserId}", request.NotificationId, request.UserId);
            throw new BadRequestException("An error occurred while deleting the notification. Please try again later.");
        }
    }
}
