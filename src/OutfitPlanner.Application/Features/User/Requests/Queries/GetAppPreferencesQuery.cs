using MediatR;
using OutfitPlanner.Application.DTOs.User;

namespace OutfitPlanner.Application.Features.User.Requests.Queries;

public class GetAppPreferencesQuery : IRequest<AppPreferencesDto>
{
    public string UserId { get; set; } = string.Empty;
}
