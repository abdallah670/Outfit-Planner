using System;
using System.Collections.Generic;

namespace OutfitPlanner.Domain.Entities;

/// <summary>
/// Represents a user comment on an outfit, with support for nested replies
/// </summary>
public class OutfitComment : BaseEntity
{
    public Guid OutfitId { get; set; }
    public Outfit Outfit { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    
    public Guid? ParentCommentId { get; set; }
    public OutfitComment? ParentComment { get; set; }
    
    public string Content { get; set; } = string.Empty;
    
    public bool IsDeleted { get; set; }
    
    public ICollection<OutfitComment> Replies { get; set; } = new List<OutfitComment>();
}
