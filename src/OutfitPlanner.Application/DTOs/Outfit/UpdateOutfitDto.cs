namespace OutfitPlanner.Application.DTOs.Outfit;

public class UpdateOutfitDto
{
    public string? Name { get; set; }
    public string? Occasion { get; set; }
    public string? WeatherCondition { get; set; }
    public string? Season { get; set; }
    public int? ComfortRating { get; set; }
    public int? StyleRating { get; set; }
    public List<CreateOutfitItemDto>? Items { get; set; }
    public string? ImageUrl { get; set; }
}
