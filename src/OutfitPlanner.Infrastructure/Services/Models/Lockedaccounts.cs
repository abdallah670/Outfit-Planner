
namespace OutfitPlanner.Infrastructure.Services.Models;

public class LockedOutUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset LockoutEnd { get; set; }
    public TimeSpan TimeRemaining { get; set; }
}