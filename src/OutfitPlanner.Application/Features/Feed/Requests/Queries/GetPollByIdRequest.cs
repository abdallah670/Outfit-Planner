using MediatR;
using OutfitPlanner.Application.DTOs.Feed;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

/// <summary>
/// Request to get a specific poll by ID
/// </summary>
public class GetPollByIdRequest : IRequest<ValidationPollDto>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
}
