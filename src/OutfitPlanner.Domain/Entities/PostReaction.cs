using System;
using OutfitPlanner.Domain.Enums;
namespace OutfitPlanner.Domain.Entities;

/// <summary>
/// Represents a user liking/reacting to a feed post
/// </summary>
public class PostReaction : BaseEntity
{
    public Guid PostId { get; set; }
    public FeedPost Post { get; set; } = null!;
    public ReactionType ReactionType {get;set;}
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}
