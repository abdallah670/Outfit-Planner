using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Domain.Entities;

public class Follow : BaseEntity
{
    public string FollowerId { get; set; } = string.Empty;
    public User? Follower { get; set; }
    
    public string FollowingId { get; set; } = string.Empty;
    public User? Following { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
