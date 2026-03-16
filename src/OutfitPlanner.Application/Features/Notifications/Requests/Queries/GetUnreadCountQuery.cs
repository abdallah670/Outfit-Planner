using MediatR;

namespace OutfitPlanner.Application.Features.Notifications.Requests.Queries;

public class GetUnreadCountQuery : IRequest<int>
{
    public string UserId { get; set; } = string.Empty;
}
