using MediatR;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;

namespace OutfitPlanner.Application.Features.Notifications.Handlers.Commands;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, BaseCommandResponse>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkAllAsReadCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<BaseCommandResponse> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();
        
        await _notificationRepository.MarkAllAsReadAsync(request.UserId);
        
        response.Success = true;
        response.Message = "All notifications marked as read";
        return response;
    }
}
