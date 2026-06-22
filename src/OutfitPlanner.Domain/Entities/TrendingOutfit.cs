using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Domain.Entities;

/// <summary>
/// Pre-calculated trending post score for a specific date
/// </summary>
public class TrendingOutfit : BaseEntity
{
    public Guid FeedPostId { get; set; }
    public FeedPost FeedPost { get; set; } = null!;
    
    public PostType PostType { get; set; }
    
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public decimal TrendingScore { get; set; }
    public int RankPosition { get; set; }
    
    public DateTime Date { get; set; }
}