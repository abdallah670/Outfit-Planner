using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using OutfitPlanner.Domain.Entities;
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
        _unitOfWork = unitOfWork;
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
            postType,
            request.FollowingOnly);




        // Get user's follows
        var followedUserIds = string.IsNullOrEmpty(request.UserId)
            ? new List<string>()
            : (await _unitOfWork.Follows.FindAsync(f => f.FollowerId == request.UserId, cancellationToken))
                .Select(f => f.FollowedId)
                .ToList();

        // Get user's votes with option details
        var userVotes = string.IsNullOrEmpty(request.UserId)
            ? Enumerable.Empty<Vote>()
            : await _unitOfWork.Votes.FindAsync(vote => vote.VoterId == request.UserId, cancellationToken);

        var votesByPollId = userVotes
            .GroupBy(vote => vote.PollId)
            .ToDictionary(g => g.Key, g => g.FirstOrDefault());


        // Get user's liked posts
        var likedPostIds = string.IsNullOrEmpty(request.UserId)
            ? new List<Guid>()
            : (await _unitOfWork.PostReactions.FindAsync(r => r.UserId == request.UserId, cancellationToken))
                .Select(r => r.PostId)
                .ToList();

        var dtos = _mapper.Map<List<FeedPostDto>>(result.Items);

        // Enrich DTOs with viewer context
        for (int i = 0; i < result.Items.Count && i < dtos.Count; i++)
        {
            var entity = result.Items[i];
            var dto = dtos[i];

            // Check if viewer reacted to this post
            var viewerReaction = entity.Reactions?.FirstOrDefault(r => r.UserId == request.UserId);
            if (viewerReaction != null)
            {
                dto.UserReaction = viewerReaction.ReactionType.ToString();
            }

            // Check if viewer voted on this poll
            if (entity.PollId.HasValue && votesByPollId.TryGetValue(entity.PollId.Value, out var vote) && vote != null)
            {
                dto.HasVoted = true;
                dto.Poll.UserVotedOptionId = vote.OptionId;
            }


            if (dto.PostType == PostType.Poll)
            {
                //get votecounts for each option
                var options = await _unitOfWork.PollOptions.FindAsync(o => o.PollId == entity.PollId, cancellationToken);

                foreach (var option in options)
                {
                    var optionDto = dto.Poll?.Options.FirstOrDefault(o => o.Id == option.Id);
                    if (optionDto != null)
                    {
                        optionDto.VoteCount = await _unitOfWork.Votes.CountAsync(vo => vo.OptionId == option.Id, cancellationToken);
                    }
                }
                dto.Poll.TotalVotes = result.Items[i].Poll?.TotalVotes ?? 0;
            }

            dto.IsFollowing = followedUserIds.Contains(entity.User?.Id);
            dto.IsLiked = likedPostIds.Contains(entity.Id);
        }

        // Get all unique tagged usernames from all posts
        var allTaggedUsernames = dtos.SelectMany(p => p.Tags).Distinct().ToList();
        if (allTaggedUsernames.Any())
        {
            var taggedUsers = await _unitOfWork.Users.GetTaggedUsersAsync(allTaggedUsernames);
            var taggedUsersDict = taggedUsers.ToDictionary(u => u.UserName, u => _mapper.Map<TaggedUserDto>(u));

            foreach (var post in dtos)
            {
                post.TaggedUsers = post.Tags
                    .Select(tag => taggedUsersDict.TryGetValue(tag, out var userDto) ? userDto : null)
                    .Where(u => u != null)
                    .Cast<TaggedUserDto>()
                    .ToList();
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
