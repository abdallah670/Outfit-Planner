using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Notifications.Requests.Queries;

namespace OutfitPlanner.Application.Features.Notifications.Handlers.Queries;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>
{
    private readonly ILogger<GetNotificationsQueryHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationsQueryHandler(ILogger<GetNotificationsQueryHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.UserId))
            {
                _logger.LogWarning("User ID is required to get notifications");
                throw new BadRequestException("User ID is required");
            }

            // Check if user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found while attempting to get notifications", request.UserId);
                throw new NotFoundException("User", request.UserId);
            }

            var notifications = await _unitOfWork.Notifications.GetNotificationsByUserIdAsync(request.UserId);

            if (notifications == null || notifications.Count() == 0)
            {
                _logger.LogInformation("No notifications found for user with ID {UserId}", request.UserId);
                return new List<NotificationDto>();
            }

            _logger.LogInformation("Retrieved {Count} notifications for user with ID {UserId}", notifications.Count(), request.UserId);

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving notifications for user {UserId}", request.UserId);
            throw new BadRequestException("An error occurred while retrieving notifications. Please try again later.");
        }
    }
}
