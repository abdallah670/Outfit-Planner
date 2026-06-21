using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.AI;
using OutfitPlanner.Application.Features.AI.Requests.Queries;

namespace OutfitPlanner.Application.Features.AI.Handlers.Queries;

public class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, List<ChatSessionDto>>
{
    private readonly IChatSessionRepository _chatSessionRepository;

    public GetSessionsQueryHandler(IChatSessionRepository chatSessionRepository)
    {
        _chatSessionRepository = chatSessionRepository;
    }

    public async Task<List<ChatSessionDto>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _chatSessionRepository.GetByUserIdAsync(request.UserId);

        return sessions.Select(s => new ChatSessionDto
        {
            Id = s.Id,
            Title = s.Title,
            Status = s.Status,
            MessageCount = s.MessageCount,
            CreatedAt = s.CreatedAt,
            LastActivityAt = s.LastActivityAt
        }).OrderByDescending(s => s.LastActivityAt ?? s.CreatedAt).ToList();
    }
}
