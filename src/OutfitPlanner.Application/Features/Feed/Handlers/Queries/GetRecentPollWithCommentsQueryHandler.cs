using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Common;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

/// <summary>
/// Handler for GetRecentPollWithCommentsQuery - returns most voted poll with cursor-paginated comments
/// </summary>
public class GetRecentPollWithCommentsQueryHandler : IRequestHandler<GetRecentPollWithCommentsQuery, RecentPollWithCommentsDto>
{
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IPostCommentRepository _postCommentRepository;
    private readonly IMapper _mapper;

    public GetRecentPollWithCommentsQueryHandler(
        IValidationPollRepository validationPollRepository,
        IFeedPostRepository feedPostRepository,
        IPostCommentRepository postCommentRepository,
        IMapper mapper)
    {
        _validationPollRepository = validationPollRepository;
        _feedPostRepository = feedPostRepository;
        _postCommentRepository = postCommentRepository;
        _mapper = mapper;
    }

    public async Task<RecentPollWithCommentsDto> Handle(GetRecentPollWithCommentsQuery request, CancellationToken cancellationToken)
    {
        // Get the most voted active poll
        var poll = await _validationPollRepository.GetMostVotedActivePollAsync();

        if (poll == null)
        {
            return new RecentPollWithCommentsDto
            {
                Poll = null,
                Comments = new List<PostCommentDto>(),
                CommentsNextCursor = null,
                CommentsHasMore = false,
                TotalComments = 0,
                TotalVotes = 0
            };
        }

        // Get the FeedPost associated with this poll
        var feedPost = await _feedPostRepository.GetByPollIdAsync(poll.Id);

        // Get comments with cursor pagination using the repository
        var commentsResult = feedPost != null 
            ? await _postCommentRepository.GetRootCommentsCursorAsync(
                feedPost.Id, 
                request.CommentsCursor, 
                request.CommentsPageSize)
            : new CursorPagination.CursorPagedResult<PostComment> 
            { 
                Items = new List<PostComment>(), 
                NextCursor = null, 
                HasMore = false,
                PageSize = request.CommentsPageSize 
            };

        // Calculate total votes
        var totalVotes = poll.Votes?.Count ?? 0;
        var totalComments = feedPost?.Comments?.Count(c => !c.IsDeleted) ?? 0;

        // Map poll and comments to DTOs
        var pollDto = _mapper.Map<ValidationPollDto>(poll);
        var commentDtos = _mapper.Map<List<PostCommentDto>>(commentsResult.Items);

        return new RecentPollWithCommentsDto
        {
            Poll = pollDto,
            Comments = commentDtos,
            CommentsNextCursor = commentsResult.NextCursor,
            CommentsHasMore = commentsResult.HasMore,
            TotalComments = totalComments,
            TotalVotes = totalVotes
        };
    }
}
