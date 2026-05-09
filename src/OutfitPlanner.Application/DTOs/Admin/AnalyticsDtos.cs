namespace OutfitPlanner.Application.DTOs.Admin;

public record AnalyticsDashboardDto(
    int TotalUsers, 
    int NewUsersToday, 
    int ActiveUsers,
    int TotalOutfits, 
    int TotalPosts, 
    int TotalPolls,
    int PendingReports, 
    int ResolvedReports,
    int LockedAccounts, 
    int BannedUsers
);

public record DailyMetricsDto(
    DateTime Date, 
    int NewUsers, 
    int NewOutfits, 
    int NewPosts, 
    int Logins
);
