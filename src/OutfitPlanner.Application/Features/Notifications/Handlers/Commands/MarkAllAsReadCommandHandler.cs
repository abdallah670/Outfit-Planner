using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands.Validators;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Notifications.Handlers.Commands;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, BaseCommandResponse>
{
    private readonly ILogger<MarkAllAsReadCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllAsReadCommandHandler(ILogger<MarkAllAsReadCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            // Validate the request
            var validationResult = await new MarkAllAsReadCommandValidator().ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for mark all as read notification request for user with ID {UserId}", request.UserId);
                throw new Exceptions.ValidationException(validationResult);
            }

            // Check if user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found while attempting to mark all notifications as read", request.UserId);
                throw new NotFoundException("User", request.UserId);
            }

            // Get unread count
            var unreadCount = await _unitOfWork.Notifications.GetUnreadCountAsync(request.UserId);
            
            // Mark all as read
            await _unitOfWork.Notifications.MarkAllAsReadAsync(request.UserId);

            response.Success = true;
            response.Message = unreadCount > 0 
                ? $"All {unreadCount} notifications marked as read successfully" 
                : "All notifications are already read";
            
            _logger.LogInformation("All notifications marked as read successfully for user {UserId}", request.UserId);
            
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while marking all notifications as read for user {UserId}", request.UserId);
            throw new BadRequestException("An error occurred while marking all notifications as read. Please try again later.");
        }
    }
}
