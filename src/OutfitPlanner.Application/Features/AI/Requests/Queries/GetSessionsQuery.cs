using MediatR;
using OutfitPlanner.Application.DTOs.AI;

namespace OutfitPlanner.Application.Features.AI.Requests.Queries;

public class GetSessionsQuery : IRequest<List<ChatSessionDto>>
{
    public string UserId { get; set; } = string.Empty;
}
