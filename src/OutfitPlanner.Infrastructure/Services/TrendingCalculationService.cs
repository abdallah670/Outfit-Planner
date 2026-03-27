using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OutfitPlanner.Infrastructure.Services;

public class TrendingCalculationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TrendingCalculationService> _logger;
    private readonly TimeSpan _runInterval = TimeSpan.FromHours(1);

    public TrendingCalculationService(IServiceProvider serviceProvider, ILogger<TrendingCalculationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Trending Calculation Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CalculateTrendingOutfitsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calculating trending outfits.");
            }

            await Task.Delay(_runInterval, stoppingToken);
        }

        _logger.LogInformation("Trending Calculation Service is stopping.");
    }

    private async Task CalculateTrendingOutfitsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _logger.LogInformation("Calculating trending outfits at {Time}", DateTimeOffset.Now);

        // 1. Get outfits created in the last 30 days
        var thresholdDate = DateTimeOffset.UtcNow.AddDays(-30);
        var outfits = await context.Outfits
            .Where(o => o.CreatedAt >= thresholdDate)
            .ToListAsync(cancellationToken);

        var trendingEntries = new List<TrendingOutfit>();
        var now = DateTime.UtcNow;

        foreach (var outfit in outfits)
        {
            // 2. Count engagement metrics
            var likeCount = await context.OutfitLikes.CountAsync(l => l.OutfitId == outfit.Id, cancellationToken);
            var commentCount = await context.OutfitComments.CountAsync(c => c.OutfitId == outfit.Id && !c.IsDeleted, cancellationToken);
            
            // Poll votes
            var poll = await context.ValidationPolls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Options.Any(o => o.OutfitId == outfit.Id), cancellationToken);
            
            int voteCount = 0;
            if (poll != null)
            {
                var optionId = poll.Options.First(o => o.OutfitId == outfit.Id).Id;
                voteCount = await context.Votes.CountAsync(v => v.OptionId == optionId, cancellationToken);
            }

            // 3. Simple Scoring Formula: (Likes * 3) + (Comments * 5) + (Votes * 1)
            // Apply time decay: Score / (HoursSinceCreation + 2)^1.5
            var ageInHours = (DateTimeOffset.UtcNow - outfit.CreatedAt).TotalHours;
            var score = (likeCount * 3.0m) + (commentCount * 5.0m) + (voteCount * 1.0m);
            var decayedScore = score / (decimal)Math.Pow(ageInHours + 2, 1.5);

            trendingEntries.Add(new TrendingOutfit
            {
                Id = Guid.NewGuid(),
                OutfitId = outfit.Id,
                PollId = poll?.Id,
                LikeCount = likeCount,
                CommentCount = commentCount,
                VoteCount = voteCount,
                TrendingScore = decayedScore,
                Date = now,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        // 4. Rank them
        var rankedEntries = trendingEntries
            .OrderByDescending(e => e.TrendingScore)
            .Select((e, index) => { e.RankPosition = index + 1; return e; })
            .Take(100)
            .ToList();

        // 5. Update Database (Replace existing for today or just add new snapshot)
        // For simplicity, we'll keep the last 24 snapshots and then prune
        context.TrendingOutfits.AddRange(rankedEntries);
        
        // 6. Prune old records (older than 7 days)
        var pruneDate = DateTime.UtcNow.AddDays(-7);
        var oldRecords = context.TrendingOutfits.Where(t => t.Date < pruneDate);
        context.TrendingOutfits.RemoveRange(oldRecords);

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Trend calculation completed. {Count} outfits ranked.", rankedEntries.Count);
    }
}
