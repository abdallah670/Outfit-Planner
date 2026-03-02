namespace OutfitPlanner.Application.DTOs.Outfit;

public class OutfitDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Occasion { get; set; } = string.Empty; // mapped from OccasionType
    public string WeatherCondition { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty; // mapped from Season
    public int? ComfortRating { get; set; }
    public int? StyleRating { get; set; }
    public DateTimeOffset? LastWorn { get; set; }
    public int TimesWorn { get; set; }
    public string Status { get; set; } = string.Empty; // mapped from OutfitStatus
    public List<OutfitItemDto> Items { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
}
