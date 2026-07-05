namespace OutfitPlanner.Application.Contracts.Infrastructure;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
