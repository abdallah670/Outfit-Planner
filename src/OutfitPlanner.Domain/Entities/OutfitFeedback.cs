using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Domain.Entities;

public class OutfitFeedback : BaseEntity
{
    public Guid OutfitId { get; set; }
    public Outfit Outfit { get; set; } = null!;
    
    public string ReviewerId { get; set; } = string.Empty;
    public User Reviewer { get; set; } = null!;
    
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; } = string.Empty;
    public bool IsPrivate { get; set; } = true;
}
