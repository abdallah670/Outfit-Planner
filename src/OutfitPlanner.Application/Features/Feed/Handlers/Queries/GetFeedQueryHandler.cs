using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

/// <summary>
/// Handler for GetFeedQuery with cursor-based pagination
/// </summary>
public class GetFeedQueryHandler : IRequestHandler<GetFeedQuery, CursorPagination.CursorPagedResult<FeedPostDto>>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetFeedQueryHandler(
        IFeedPostRepository feedPostRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _feedPostRepository = feedPostRepository;
        _mapper = mapper;
    }

    public async Task<CursorPagination.CursorPagedResult<FeedPostDto>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
    {
        PostType? postType = Enum.TryParse<PostType>(request.PostType, true, out var pt) ? pt : null;
        var visibility = Enum.TryParse<Visibility>(request.Visibility, true, out var v) ? v : Visibility.Public;

        var result = await _feedPostRepository.GetFeedAsync(
            request.UserId,
            request.Cursor,
            request.PageSize,
            request.SortBy,
            visibility,
            postType);

        // Get user's follows
        var followedUserIds = (await _unitOfWork.Follows.FindAsync(f => f.FollowerId == request.UserId, cancellationToken))
                        .Select(f => f.FollowedId)
                        .ToList();

        // Get user's votes with option details
        var userVotes = await _unitOfWork.Votes.FindAsync(vote => vote.VoterId == request.UserId, cancellationToken);
        var votesByPollId = userVotes
            .GroupBy(vote => vote.Option.PollId)
            .ToDictionary(g => g.Key, g => g.FirstOrDefault());

        // Get user's liked posts
        var likedPostIds = (await _unitOfWork.PostReactions.FindAsync(r => r.UserId == request.UserId, cancellationToken))
            .Select(r => r.PostId)
            .ToList();

        // Map and enrich posts with user context
        var dtos = result.Items.Select(post =>
        {
            var dto = _mapper.Map<FeedPostDto>(post);
            dto.IsOwner = post.User.Id == request.UserId;

            // Check if user voted on this poll
            if (post.PollId.HasValue && votesByPollId.TryGetValue(post.PollId.Value, out var vote) && vote != null)
            {
                dto.HasVoted = true;
                // Store the option ID as the user's vote
                dto.UserVote = vote.Option.Id.ToString();
            }
            else
            {
                dto.HasVoted = false;
                dto.UserVote = null;
            }

            dto.IsFollowing = followedUserIds.Contains(post.User.Id);
            dto.IsLiked = likedPostIds.Contains(post.Id);

            return dto;
        }).ToList();

        return new CursorPagination.CursorPagedResult<FeedPostDto>
        {
            Items = dtos,
            NextCursor = result.NextCursor,
            HasMore = result.HasMore,
            PageSize = result.PageSize
        };
    }
}
