using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

public class GetPollVotersQueryHandler : IRequestHandler<GetPollVotersQuery, IEnumerable<(Vote, string, string?)>>
{
    private readonly IVoteRepository _voteRepository;

    public GetPollVotersQueryHandler(IVoteRepository voteRepository)
    {
        _voteRepository = voteRepository;
    }

    public async Task<IEnumerable<(Vote, string, string?)>> Handle(GetPollVotersQuery request, CancellationToken cancellationToken)
    {
        return await _voteRepository.GetVotersWithDetailsAsync(request.PollId, request.OptionId);
    }
}
