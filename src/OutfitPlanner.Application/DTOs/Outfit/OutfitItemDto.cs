namespace OutfitPlanner.Application.DTOs.Outfit;

public class OutfitItemDto
{
    public Guid Id { get; set; }
    public Guid ClothingItemId { get; set; }
    public string ClothingItemName { get; set; } = string.Empty;
    public string ClothingItemImageUrl { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Primary, Secondary, Accent
    public int LayeringOrder { get; set; }
    public bool IsEssential { get; set; }
}
