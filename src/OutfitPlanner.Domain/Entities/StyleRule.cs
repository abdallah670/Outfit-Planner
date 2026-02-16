using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Domain.Entities;

public class StyleRule : BaseEntity
{
    public Guid UserStyleProfileId { get; set; }
    // public UserStyleProfile UserStyleProfile { get; set; } = null!; // Avoid circular ref if not needed, or add

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Logic for rule? E.g., "No black with brown"
    // For now, storing as simple properties or maybe json criteria if complex
    // Adhering to simple entity structure first.
    public string CriteriaJson { get; set; } = "{}"; 
}
