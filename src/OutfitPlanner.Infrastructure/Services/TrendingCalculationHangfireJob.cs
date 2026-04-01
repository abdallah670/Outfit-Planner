using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Features.Social.Requests.Commands;

namespace OutfitPlanner.Infrastructure.Services;

/// <summary>
/// Hangfire job for calculating daily trending outfits
/// </summary>
public class TrendingCalculationHangfireJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<TrendingCalculationHangfireJob> _logger;

    public TrendingCalculationHangfireJob(IMediator mediator, ILogger<TrendingCalculationHangfireJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Calculates trending outfits - called by Hangfire scheduler
    /// </summary>
    public async Task CalculateTrendingAsync()
    {
        try
        {
            _logger.LogInformation("Hangfire: Starting daily trending outfit calculation at {Time}", DateTime.UtcNow);

            var result = await _mediator.Send(new CalculateTrendingOutfitsCommand());

            if (result.Success)
            {
                _logger.LogInformation("Hangfire: Daily trending outfit calculation completed successfully");
            }
            else
            {
                _logger.LogWarning("Hangfire: Daily trending outfit calculation failed: {Message}", result.Message);
                throw new Exception($"Trending calculation failed: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hangfire: Error calculating daily trending outfits");
            throw;
        }
    }
}
