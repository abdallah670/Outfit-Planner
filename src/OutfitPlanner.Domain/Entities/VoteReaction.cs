namespace OutfitPlanner.Domain.Entities;

/// <summary>
/// Represents a reaction (like, love, insightful) on a vote
/// </summary>
public class VoteReaction : BaseEntity
{
    public Guid VoteId { get; set; }
    public Vote Vote { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    
    /// <summary>
    /// 1 = Like, 2 = Love, 3 = Insightful
    /// </summary>
    public ReactionType ReactionType { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public enum ReactionType
{
    Like = 1,
    Love = 2,
    Insightful = 3
}
