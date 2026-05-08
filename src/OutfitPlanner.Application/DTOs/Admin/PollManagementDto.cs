namespace OutfitPlanner.Application.DTOs.Admin;

public record ClosePollRequest(string Reason);

public record BulkPollOperationRequest(List<BulkPollOperationItem> Operations);

public record BulkPollOperationItem(
    Guid PollId,
    string Type, // "close", "feature", "unfeature", "delete"
    string? Reason = null
);

public record AdminPollDto(
    Guid Id,
    string UserId,
    string UserName,
    string Question,
    List<string> Options,
    List<int> OptionVotes,
    int TotalVotes,
    DateTime CreatedAt,
    DateTime? EndsAt,
    PollStatus Status,
    bool IsFeatured,
    DateTime? FeaturedAt,
    string? FeaturedBy
);

public enum PollStatus
{
    Active = 0,
    Closed = 1,
    Featured = 2,
    Hidden = 3
}
