namespace OutfitPlanner.Application.DTOs.Admin;

public record RejectPostRequest(string Reason);

public record BulkPostOperationRequest(List<BulkPostOperationItem> Operations);
public record BulkPollOperationRequest(List<BulkPostOperationItem> Operations);
public record BulkOutfitOperationRequest(List<BulkPostOperationItem> Operations);

public record BulkPostOperationItem(
    Guid PostId,
    string Type, // "approve", "reject", "delete"
    string? Reason = null
);

public record ContentFilterRequest(
    string? Search = null,
    string? ContentType = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 25
);

public record AdminPostDto(
    Guid Id,
    string UserId,
    string UserName,
    string? Caption,
    List<string> Tags,
    int LikesCount,
    int CommentsCount,
    DateTime CreatedAt,
    OutfitPlanner.Domain.Enums.PostType PostType,
    // Optional Outfit details
    Guid? OutfitId = null,
    string? OutfitName = null,
    string? OutfitImageUrl = null,
    List<string>? ItemsImageUrls = null,
    // Optional Poll details
    Guid? PollId = null,
    string? PollQuestion = null,
    List<string>? PollOptions = null,
    List<int>? PollOptionVotes = null,
    int? TotalPollVotes = null,
    DateTime? PollExpiresAt = null
);
