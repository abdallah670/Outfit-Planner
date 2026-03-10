namespace OutfitPlanner.Application.DTOs.Social;

/// <summary>
/// DTO for trending data response
/// </summary>
public class TrendingDataDto
{
    public List<TrendItemDto> Trends { get; set; } = new();
    public List<TopPollDto> TopPolls { get; set; } = new();
    public DateTimeOffset GeneratedAt { get; set; }
}

/// <summary>
/// DTO for a single trend item
/// </summary>
public class TrendItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int PopularityScore { get; set; }
    public DateTimeOffset TrendingSince { get; set; }
}

/// <summary>
/// DTO for top poll information in trends
/// </summary>
public class TopPollDto
{
    public Guid PollId { get; set; }
    public string Question { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public double EngagementRate { get; set; }
}
