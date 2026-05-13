namespace OutfitPlanner.Application.DTOs.Feed;
public class FollowDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public DateTimeOffset FollowedAt { get; set; }
    public bool IsFollowing { get; set; } = false;
}

public class FollowersResponse
{
    public List<FollowDto> Followers { get; set; } = new();
    public int TotalCount { get; set; }
}
