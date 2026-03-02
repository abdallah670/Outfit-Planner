
namespace OutfitPlanner.Application.DTOs.Outfit;

public class RecordOutfitWearDto
{
    public DateTimeOffset WornAt { get; set; } = DateTimeOffset.UtcNow;
    public string? WeatherCondition { get; set; }
    public Guid? EventId { get; set; }
}
