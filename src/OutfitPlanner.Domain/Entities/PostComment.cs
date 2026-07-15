using System;
using System.Collections.Generic;

namespace OutfitPlanner.Domain.Entities;

/// <summary>
/// Represents a user comment on an outfit, with support for nested replies
/// </summary>
public class PostComment : BaseEntity
{
    public Guid PostId { get; set; }
    public FeedPost Post { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    
    public Guid? ParentCommentId { get; set; }
    public PostComment? ParentComment { get; set; }
    
    public string Content { get; set; } = string.Empty;
    
    
    public int TotalReplies { get; set; }

    /// <summary>
    /// User IDs mentioned in this comment (stored as a JSON list of strings).
    /// The parent comment author is always included for replies.
    /// </summary>
    public List<string> MentionedUsers { get; set; } = new();

    public ICollection<PostComment> Replies { get; set; } = new List<PostComment>();
}
