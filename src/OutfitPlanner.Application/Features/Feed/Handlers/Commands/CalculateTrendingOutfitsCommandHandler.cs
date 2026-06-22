using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

public class CalculateTrendingOutfitsCommandHandler : IRequestHandler<CalculateTrendingOutfitsCommand, BaseCommandResponse>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly ITrendingOutfitRepository _trendingOutfitRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CalculateTrendingOutfitsCommandHandler> _logger;

    public CalculateTrendingOutfitsCommandHandler(
        IFeedPostRepository feedPostRepository,
        ITrendingOutfitRepository trendingOutfitRepository,
        IUnitOfWork unitOfWork,
        ILogger<CalculateTrendingOutfitsCommandHandler> logger)
    {
        _feedPostRepository = feedPostRepository;
        _trendingOutfitRepository = trendingOutfitRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(CalculateTrendingOutfitsCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            var date = DateTime.UtcNow.Date;
            
            // Get all feed posts (both outfit and poll posts)
            var feedPosts = await _feedPostRepository.GetAllAsync();

            // Clear existing trending for today to avoid duplicates
            var existing = await _trendingOutfitRepository.GetByDateRangeAsync(date, date.AddDays(1));
            if (existing.Any())
            {
                await _trendingOutfitRepository.RemoveRangeAsync(existing);
            }

            var scoredPosts = new List<TrendingOutfit>();

            foreach (var feedPost in feedPosts)
            {
                // Skip posts that are neither outfit nor poll posts
                if (feedPost.PostType == PostType.Outfit && feedPost.OutfitId == null)
                    continue;
                if (feedPost.PostType == PostType.Poll && feedPost.PollId == null)
                    continue;

                var score = CalculateTrendingScore(feedPost);

                scoredPosts.Add(new TrendingOutfit
                {
                    FeedPostId = feedPost.Id,
                    PostType = feedPost.PostType,
                    TrendingScore = score,
                    LikesCount = feedPost.LikesCount,
                    CommentsCount = feedPost.CommentsCount,
                    Date = date,
                    RankPosition = 0
                });
            }

            // Rank and save
            scoredPosts = scoredPosts.OrderByDescending(o => o.TrendingScore).ToList();
            
            for (int i = 0; i < scoredPosts.Count; i++)
            {
                scoredPosts[i].RankPosition = i + 1;
                await _trendingOutfitRepository.AddAsync(scoredPosts[i]);
            }

            // Persist all changes to the database
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Success = true;
            response.Message = "Trending posts calculated successfully";
            
            _logger.LogInformation("Calculated trending for {Count} posts", scoredPosts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating trending posts");
            response.Success = false;
            response.Message = "Error calculating trending posts";
            response.Errors.Add(ex.Message);
        }

        return response;
    }

    private decimal CalculateTrendingScore(FeedPost feedPost)
    {
        // Unified engagement scoring for both outfit and poll posts
        // Votes on polls create PostReactions, so they're counted in Reactions.Count
        var reactionCount = feedPost.LikesCount;  // Likes or votes
        var CommentsCount = feedPost.CommentsCount;  // Comments count
        
        // Scoring weights:
        // - Reactions (likes/votes): 5 points each
        // - Comments: 2 points each
        decimal engagementScore = (reactionCount * 5) + (CommentsCount * 2);
        
        // Time decay factor (half-life of 24 hours)
        // Newer posts get higher scores
        var hoursSincePosted = (DateTime.UtcNow - feedPost.CreatedAt).TotalHours;
        var timeDecayFactor = Math.Pow(0.5, hoursSincePosted / 24);
        
        // Final score with base boost
        return (engagementScore * (decimal)timeDecayFactor) + 1.0m;
    }
}
