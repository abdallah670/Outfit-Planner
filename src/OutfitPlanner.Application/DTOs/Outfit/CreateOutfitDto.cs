namespace OutfitPlanner.Application.DTOs.Outfit;

public class CreateOutfitDto
{
    public string Name { get; set; } = string.Empty;
    public string Occasion { get; set; } = string.Empty;
    public string WeatherCondition { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public int ComfortRating { get; set; } = 5;
    public List<CreateOutfitItemDto> Items { get; set; } = new();
}

public class CreateOutfitItemDto
{
    public Guid ClothingItemId { get; set; }
    public string Role { get; set; } = string.Empty; // Primary, Secondary, Accent (ItemRole)
    public int LayeringOrder { get; set; }
    public bool IsEssential { get; set; }
}
