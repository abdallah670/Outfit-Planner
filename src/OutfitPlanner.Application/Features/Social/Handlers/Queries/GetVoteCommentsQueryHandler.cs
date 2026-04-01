using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.Social.Requests.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OutfitPlanner.Application.Features.Social.Handlers.Queries;

public class GetVoteCommentsQueryHandler : IRequestHandler<GetVoteCommentsQuery, List<VoteCommentDto>>
{
    private readonly IVoteCommentRepository _voteCommentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetVoteCommentsQueryHandler> _logger;

    public GetVoteCommentsQueryHandler(
        IVoteCommentRepository voteCommentRepository,
        IMapper mapper,
        ILogger<GetVoteCommentsQueryHandler> logger)
    {
        _voteCommentRepository = voteCommentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<VoteCommentDto>> Handle(GetVoteCommentsQuery request, CancellationToken cancellationToken)
    {
        var comments = await _voteCommentRepository.GetCommentsForVoteAsync(request.VoteId, request.MaxDepth);
        
        // This relies on AutoMapper mappings
        // If mapping isn't fully defined yet, we can map manually or assume Map works
        var mappedComments = _mapper.Map<List<VoteCommentDto>>(comments);
        return mappedComments;
    }
}
