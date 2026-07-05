namespace OutfitPlanner.Infrastructure.Services;

public class SystemClock : OutfitPlanner.Application.Contracts.Infrastructure.IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
