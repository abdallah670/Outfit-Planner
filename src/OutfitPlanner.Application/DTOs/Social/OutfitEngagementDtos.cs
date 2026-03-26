namespace OutfitPlanner.Application.DTOs.Social;

public class OutfitEngagementSummaryDto
{
    public Guid OutfitId { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; } // For future use
    public bool UserHasLiked { get; set; }
}

public class OutfitLikeResultDto
{
    public Guid OutfitId { get; set; }
    public int LikeCount { get; set; }
    public bool UserHasLiked { get; set; }
    public bool WasNewlyLiked { get; set; }
}

public class OutfitCommentDto
{
    public Guid Id { get; set; }
    public Guid OutfitId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatarUrl { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
