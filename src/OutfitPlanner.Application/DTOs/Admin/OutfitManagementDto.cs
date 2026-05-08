namespace OutfitPlanner.Application.DTOs.Admin;

public record RejectOutfitRequest(string Reason);

public record BulkOutfitOperationRequest(List<BulkOutfitOperationItem> Operations);

public record BulkOutfitOperationItem(
    Guid OutfitId,
    string Type, // "feature", "unfeature", "approve", "reject", "delete"
    string? Reason = null
);

public record AdminOutfitDto(
    Guid Id,
    string UserId,
    string UserName,
    string Name,
    string Description,
    List<string> Tags,
    List<string> ImageUrls,
    int LikesCount,
    int CommentsCount,
    DateTime CreatedAt,
    bool IsFeatured,
    bool IsApproved,
    DateTime? FeaturedAt,
    string? FeaturedBy,
    DateTime? ApprovedAt,
    string? ApprovedBy
);

public enum OutfitStatus
{
    Draft = 0,
    Published = 1,
    Featured = 2,
    Hidden = 3,
    Rejected = 4
};
