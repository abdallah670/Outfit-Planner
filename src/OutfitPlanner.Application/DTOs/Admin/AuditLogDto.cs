namespace OutfitPlanner.Application.DTOs.Admin;

public class AuditLogStatistics
{
    public int TotalLogs { get; set; }
    public int UniqueUsers { get; set; }
    public List<string> TopActions { get; set; } = new();
    public Dictionary<int, int> LogsByHour { get; set; } = new();
}

public class AuditLogAnalytics
{
    public int TotalRecentLogs { get; set; }
    public int RecentUsers { get; set; }
    public Dictionary<string, int> ActionBreakdown { get; set; } = new();
    public List<string> TopUsers { get; set; } = new();
}

public class AuditLogTrends
{
    public Dictionary<DateTime, int> DailyTrends { get; set; } = new();
    public Dictionary<string, Dictionary<DateTime, int>> ActionTrends { get; set; } = new();
}
