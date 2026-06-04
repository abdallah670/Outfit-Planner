namespace OutfitPlanner.Application.DTOs.Feed;

/// <summary>
/// DTO for trending outfit data
/// </summary>
public class TrendingOutfitDto
{
    public Guid Id { get; set; }
    public string? ImageUrl { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public Guid FeedPostId { get; set; }
    public string? PostType { get; set; }
    public int RankPosition { get; set; }
    public double TrendingScore { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
  
}
