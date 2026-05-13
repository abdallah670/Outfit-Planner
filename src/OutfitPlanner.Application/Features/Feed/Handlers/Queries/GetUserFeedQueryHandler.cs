using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common; // added
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;


namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

public class GetUserFeedQueryHandler : IRequestHandler<GetUserFeedQuery, CursorPagination.CursorPagedResult<FeedPostDto>>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserFeedQueryHandler(IFeedPostRepository feedPostRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _feedPostRepository = feedPostRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CursorPagination.CursorPagedResult<FeedPostDto>> Handle(GetUserFeedQuery request, CancellationToken cancellationToken)
    {
        var result = await _feedPostRepository.GetFeedAsync(
            request.UserId,
            request.Cursor,
            request.PageSize,
            null,
            Visibility.Public,
            null);

        var dtos = _mapper.Map<List<FeedPostDto>>(result.Items);

        // Enrich DTOs with viewer-specific data if a viewer is provided
        if (!string.IsNullOrEmpty(request.ViewerUserId))
        {
            // Get viewer's follows
            var viewerFollowedUserIds = (await _unitOfWork.Follows.FindAsync(f => f.FollowerId == request.ViewerUserId, cancellationToken))
                .Select(f => f.FollowedId)
                .ToList();

            // Get viewer's votes
            var viewerVotes = await _unitOfWork.Votes.FindAsync(v => v.VoterId == request.ViewerUserId, cancellationToken);
            var viewerVotesByPollId = viewerVotes
                .GroupBy(vote => vote.Option.PollId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault());

            // Get viewer's liked posts
            var viewerLikedPostIds = (await _unitOfWork.PostReactions.FindAsync(r => r.UserId == request.ViewerUserId, cancellationToken))
                .Select(r => r.PostId)
                .ToList();

            // Enrich DTOs with viewer context
            for (int i = 0; i < result.Items.Count && i < dtos.Count; i++)
            {
                var entity = result.Items[i];
                var dto = dtos[i];

                // Check if viewer reacted to this post
                var viewerReaction = entity.Reactions?.FirstOrDefault(r => r.UserId == request.ViewerUserId);
                if (viewerReaction != null)
                {
                    dto.UserReaction = viewerReaction.ReactionType.ToString();
                }

                // Check if viewer voted on this poll
                if (entity.PollId.HasValue && viewerVotesByPollId.TryGetValue(entity.PollId.Value, out var vote) && vote != null)
                {
                    dto.HasVoted = true;
                    dto.UserVote = vote.Option.Id.ToString();
                }

                dto.IsFollowing = viewerFollowedUserIds.Contains(entity.User.Id);
                dto.IsLiked = viewerLikedPostIds.Contains(entity.Id);
            }
        }

        return new CursorPagination.CursorPagedResult<FeedPostDto>
        {
            Items = dtos,
            NextCursor = result.NextCursor,
            HasMore = result.HasMore,
            PageSize = result.PageSize
        };
    }
}
