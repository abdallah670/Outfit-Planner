
using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;

namespace  OutfitPlanner.Application.Features.Feed.Handlers.Queries;   
public class GetFeedPostByIdQueryHandler : IRequestHandler<GetFeedPostByIdQuery, GetFeedPostByIdDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetFeedPostByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<GetFeedPostByIdDto?> Handle(GetFeedPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _unitOfWork.FeedPosts.GetByIdWithDetailsAsync(request.PostId);
        
        if (post == null)
        {
            // Try searching by PollId in case the ID provided was a PollId (e.g. from My Polls list)
            post = await _unitOfWork.FeedPosts.GetByPollIdAsync(request.PostId);
            if (post != null)
            {
                // If found by PollId, we still want the full details
                post = await _unitOfWork.FeedPosts.GetByIdWithDetailsAsync(post.Id);
            }
        }

        if (post == null) return null;


        var postDto =new GetFeedPostByIdDto
        {
            Id = post.Id,
            UserId = post.UserId,
            UserName = post.User?.UserName ?? "Unknown",
            UserAvatarUrl = post.User?.ProfilePictureUrl ?? string.Empty,
            PostType = post.PostType,
            OutfitId = post.OutfitId,
            Outfit = post.Outfit != null ? _mapper.Map<OutfitDto>(post.Outfit) : null,
            PollId = post.PollId,
            Poll = post.Poll != null ? _mapper.Map<ValidationPollDto>(post.Poll) : null,
            Caption = post.Caption,
            Visibility = post.Visibility,
            LikesCount = post.LikesCount,
            CommentsCount = post.CommentsCount,
            CreatedAt = post.CreatedAt
        };
        if (postDto.Poll != null)
        {
            postDto.Poll.TotalVotes = post.Poll?.TotalVotes ?? 0;
        }

        if(!string.IsNullOrEmpty(request.RequesterId)&&post.UserId.ToString()==request.RequesterId)
        {
            postDto.IsOwner = true;
        }
        else{
            var requesterId = request.RequesterId ?? string.Empty;
            var IsFollwed = await _unitOfWork.Follows.IsFollowingAsync(requesterId, post.UserId);
            postDto.IsFollowing = IsFollwed;
        }
        // Get user's votes with option details
        if(post.PollId.HasValue && !string.IsNullOrEmpty(request.RequesterId)&&!postDto.IsOwner)
        {  //user has only one vote per poll, so we can directly query for it
            var userVote = await _unitOfWork.Votes.GetUserVote(request.RequesterId, post.PollId.Value);

            if (userVote != null)
            {
                postDto.HasVoted = true;
                postDto.Poll.UserVotedOptionId = userVote.OptionId;
            }
            //get options with vote counts
            foreach (var option in postDto.Poll.Options)
            {
                var optionDto = postDto.Poll.Options.FirstOrDefault(o => o.Id == option.Id);
                if (optionDto != null)
                {
                    optionDto.VoteCount = await _unitOfWork.Votes.CountAsync(v => v.OptionId == option.Id, cancellationToken);
                }
            }
            
        }
      
        // Get user's liked posts
        if (!string.IsNullOrEmpty(request.RequesterId))
        {
            var IsLiked = await _unitOfWork.PostReactions.HasReactionAsync(request.PostId, request.RequesterId);
            postDto.IsLiked = IsLiked;
        }
        
       // Load comments
        var comments = await _unitOfWork.PostComments.GetCommentsByPostIdAsync(request.PostId);
        postDto.Comments = _mapper.Map<List<PostCommentDto>>(comments);
        return postDto;
    }
}