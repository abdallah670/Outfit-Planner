namespace OutfitPlanner.Application.DTOs.Feed;

/// <summary>
/// DTO for most voted poll with comments page (cursor-based pagination)
/// </summary>
public class RecentPollWithCommentsDto
{
    public ValidationPollDto? Poll { get; set; }
    public List<PostCommentDto> Comments { get; set; } = new();
    public string? CommentsNextCursor { get; set; }
    public bool CommentsHasMore { get; set; }
    public int TotalComments { get; set; }
    public int TotalVotes { get; set; }
}
