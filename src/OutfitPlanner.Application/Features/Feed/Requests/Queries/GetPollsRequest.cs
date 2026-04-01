using MediatR;
using OutfitPlanner.Application.DTOs.Social;

namespace OutfitPlanner.Application.Features.Social.Requests.Queries;

/// <summary>
/// Request to get all polls for a user
/// </summary>
public class GetPollsRequest : IRequest<List<ValidationPollDto>>
{
    public string UserId { get; set; } = string.Empty;
}
