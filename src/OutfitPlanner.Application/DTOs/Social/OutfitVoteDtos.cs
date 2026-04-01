namespace OutfitPlanner.Application.DTOs.Social;

public class OutfitVoteResultDto
{
    public Guid OutfitId { get; set; }
    public int VoteCount { get; set; }
    public bool UserHasVoted { get; set; }
}

public class CreateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class VoteCommentDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<VoteReactionDto> Reactions { get; set; } = new();
}

public class VoteReactionDto
{
    public string UserId { get; set; } = string.Empty;
    public string ReactionType { get; set; } = string.Empty;
}

public class ReactionRequest
{
    public string ReactionType { get; set; } = string.Empty;
}

public class OutfitEngagementDto
{
    public int VoteCount { get; set; }
    public int CommentCount { get; set; }
    public int ReactionCount { get; set; }
    public bool UserHasVoted { get; set; }
    public string? UserReaction { get; set; }
}

public class TrendingOutfitDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string OutfitName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Occasion { get; set; }
    public int Likes { get; set; }
    public int VoteCount { get; set; }
    public int CommentCount { get; set; }
    public int Rank { get; set; }
    public Guid VoteId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
