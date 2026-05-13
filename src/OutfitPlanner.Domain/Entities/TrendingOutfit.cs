namespace OutfitPlanner.Domain.Entities;

/// <summary>
/// Pre-calculated trending outfit score for a specific date
/// </summary>
public class TrendingOutfit : BaseEntity
{
    public Guid OutfitId { get; set; }
    public Outfit Outfit { get; set; } = null!;
  
    public Guid? PollId { get; set; }
    public ValidationPoll? Poll { get; set; }
    
    public int VoteCount { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public decimal TrendingScore { get; set; }
    public int RankPosition { get; set; }
    
    public DateTime Date { get; set; }
}
