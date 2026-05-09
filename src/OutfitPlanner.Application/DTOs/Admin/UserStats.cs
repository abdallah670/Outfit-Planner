
namespace OutfitPlanner.Application.DTOs.Admin;

public record UserStatisticsDto(
    int TotalOutfits,
    int TotalPosts,
    int TotalComments,
    int TotalLikes,
    DateTime? LastActive,
    List<MonthlyActivityDto> MonthlyActivity
);

public record MonthlyActivityDto(
    int Month,
    int Year,
    int OutfitsCreated,
    int PostsCreated,
    int CommentsMade
);
