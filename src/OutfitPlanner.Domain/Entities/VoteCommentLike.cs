using System;

namespace OutfitPlanner.Domain.Entities;

public class VoteCommentLike : BaseEntity
{
    public Guid CommentId { get; set; }
    public VoteComment Comment { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
