namespace OutfitPlanner.Application.DTOs.Admin;

public record RejectPostRequest(string Reason);

public record BulkPostOperationRequest(List<BulkPostOperationItem> Operations);

public record BulkPostOperationItem(
    Guid PostId,
    string Type, // "approve", "reject", "delete"
    string? Reason = null
);

public record ContentFilterRequest(
    string? Search = null,
    string? Status = null,
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
    string Title,
    string Content,
    List<string> Tags,
    int LikesCount,
    int CommentsCount,
    DateTime CreatedAt,
    bool IsApproved,
    PostStatus Status,
    DateTime? ApprovedAt,
    string? ApprovedBy
);

public enum PostStatus
{
    Draft = 0,
    Published = 1,
    Approved = 2,
    Rejected = 3,
    Hidden = 4
}
