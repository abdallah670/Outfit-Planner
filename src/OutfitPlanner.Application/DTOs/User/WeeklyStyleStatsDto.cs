namespace OutfitPlanner.Application.DTOs.User;

public class WeeklyStyleStatsDto
{
    public List<WeeklyReportDto> WeeklyReports { get; set; } = new();
}

public class WeeklyReportDto
{
    public DateTimeOffset WeekStart { get; set; }
    public DateTimeOffset WeekEnd { get; set; }
    public bool IsCurrentWeek { get; set; }
    public string? MostWornItemName { get; set; }
    public int MostWornCount { get; set; }
    public double VarietyScore { get; set; }
    public decimal ComfortAverage { get; set; }
    public int TotalWears { get; set; }
    public string Trend { get; set; } = string.Empty;
}
