using MediatR;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.Notifications.Requests.Queries;

namespace OutfitPlanner.Application.Features.Notifications.Handlers.Queries;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _notificationRepository;

    public GetUnreadCountQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        return await _notificationRepository.GetUnreadCountAsync(request.UserId);
    }
}
