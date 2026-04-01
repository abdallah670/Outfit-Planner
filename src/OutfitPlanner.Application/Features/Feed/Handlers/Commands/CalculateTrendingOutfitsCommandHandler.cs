using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

public class CalculateTrendingOutfitsCommandHandler : IRequestHandler<CalculateTrendingOutfitsCommand, BaseCommandResponse>
{
    private readonly IOutfitRepository _outfitRepository;
    private readonly ITrendingOutfitRepository _trendingOutfitRepository;
    private readonly IValidationPollRepository _pollRepository;
    private readonly ILogger<CalculateTrendingOutfitsCommandHandler> _logger;

    public CalculateTrendingOutfitsCommandHandler(
        IOutfitRepository outfitRepository,
        ITrendingOutfitRepository trendingOutfitRepository,
        IValidationPollRepository pollRepository,
        ILogger<CalculateTrendingOutfitsCommandHandler> logger)
    {
        _outfitRepository = outfitRepository;
        _trendingOutfitRepository = trendingOutfitRepository;
        _pollRepository = pollRepository;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(CalculateTrendingOutfitsCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            var date = DateTime.UtcNow.Date;
            
            // Get all outfits 
            var outfits = await _outfitRepository.GetAllAsync();
            // Use the new abstraction-friendly method
            var polls = await _pollRepository.GetPollsForTrendingAsync();

            // Clear existing trending for today to avoid duplicates
            var existing = await _trendingOutfitRepository.GetByDateRangeAsync(date, date.AddDays(1));
            if (existing.Any())
            {
                await _trendingOutfitRepository.RemoveRangeAsync(existing);
            }

            var scoredOutfits = new List<TrendingOutfit>();

            foreach (var outfit in outfits)
            {
                // Find the auto-generated poll for this outfit
                // (In our system, every outfit has a poll where it's the primary option)
                var poll = polls.FirstOrDefault(p => p.Options.Any(o => o.OutfitId == outfit.Id));
                
                decimal score = 1.0m;
                int voteCount = 0;
                int likeCount = 0;
                int commentCount = 0;
                Guid? pollId = null;

                if (poll != null)
                {
                    pollId = poll.Id;
                    var option = poll.Options.FirstOrDefault(o => o.OutfitId == outfit.Id);
                    if (option != null)
                    {
                        voteCount = option.Votes.Count;
                        likeCount = option.Votes.Sum(v => v.Reactions.Count);
                        // We'll approximate comment count for now or handle via another include if needed
                        // For this demo, let's say every rating >= 4 counts as extra engagement
                        var highRatings = option.Votes.Count(v => v.Rating >= 4);
                        
                        score = (voteCount * 10) + (likeCount * 5) + (highRatings * 2) + 1.0m;
                    }
                }

                scoredOutfits.Add(new TrendingOutfit
                {
                    OutfitId = outfit.Id,
                    PollId = pollId,
                    TrendingScore = score,
                    VoteCount = voteCount,
                    LikeCount = likeCount,
                    CommentCount = commentCount,
                    Date = date,
                    RankPosition = 0
                });
            }

            // Rank and save
            scoredOutfits = scoredOutfits.OrderByDescending(o => o.TrendingScore).ToList();
            
            for (int i = 0; i < scoredOutfits.Count; i++)
            {
                scoredOutfits[i].RankPosition = i + 1;
                await _trendingOutfitRepository.AddAsync(scoredOutfits[i]);
            }

            response.Success = true;
            response.Message = "Trending outfits calculated successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating trending outfits");
            response.Success = false;
            response.Message = "Error calculating trending outfits";
            response.Errors.Add(ex.Message);
        }

        return response;
    }
}
