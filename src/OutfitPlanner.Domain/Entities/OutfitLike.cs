using System;

namespace OutfitPlanner.Domain.Entities;

/// <summary>
/// Represents a user liking an outfit
/// </summary>
public class OutfitLike : BaseEntity
{
    public Guid OutfitId { get; set; }
    public Outfit Outfit { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}
