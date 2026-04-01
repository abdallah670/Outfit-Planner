using System;
using System.Collections.Generic;

namespace OutfitPlanner.Domain.Entities;

public class VoteComment : BaseEntity
{
    public Guid VoteId { get; set; }
    public Vote Vote { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public Guid? ParentCommentId { get; set; }
    public VoteComment? ParentComment { get; set; }
    public ICollection<VoteComment> Replies { get; set; } = new List<VoteComment>();

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<VoteCommentLike> Likes { get; set; } = new List<VoteCommentLike>();
}
