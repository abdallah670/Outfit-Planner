using MediatR;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;

namespace OutfitPlanner.Application.Features.Notifications.Handlers.Commands;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, BaseCommandResponse>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkAsReadCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<BaseCommandResponse> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();
        
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);
        if (notification == null)
        {
            response.Success = false;
            response.Message = "Notification not found";
            return response;
        }

        if (notification.UserId != request.UserId)
        {
            response.Success = false;
            response.Message = "Unauthorized";
            return response;
        }

        await _notificationRepository.MarkAsReadAsync(request.NotificationId);
        
        response.Success = true;
        response.Message = "Notification marked as read";
        return response;
    }
}
