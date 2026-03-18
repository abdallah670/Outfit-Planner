using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands.Validators;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Notifications.Handlers.Commands;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    private readonly ILogger<CreateNotificationCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateNotificationCommandHandler(
        ILogger<CreateNotificationCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the request
            var validationResult = await new CreateNotificationCommandValidator().ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for create notification request for user with ID {UserId}", request.UserId);
                throw new Exceptions.ValidationException(validationResult);
            }

            // Check if user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found while attempting to create notification", request.UserId);
                throw new NotFoundException("User", request.UserId);
            }

            // Create the notification entity
            var notification = new Notification
            {
                UserId = request.UserId,
                Type = request.Request.Type,
                Title = request.Request.Title,
                Message = request.Request.Message,
                ActionUrl = request.Request.ActionUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Notification created successfully with ID {NotificationId} for user {UserId}", 
                notification.Id, request.UserId);

            return _mapper.Map<NotificationDto>(notification);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exceptions.ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating notification for user {UserId}", request.UserId);
            throw new BadRequestException("An error occurred while creating the notification. Please try again later.");
        }
    }
}
