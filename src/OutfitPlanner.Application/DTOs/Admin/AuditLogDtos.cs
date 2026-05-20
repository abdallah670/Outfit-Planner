namespace OutfitPlanner.Application.DTOs.Admin;

public record AuditLogDto(
    Guid Id, 
    string UserName, 
    string Action, 
    string EntityType, 
    DateTimeOffset Timestamp
);

public record AuditLogDetailDto(
    Guid Id,
    string UserId,
    string UserName,
    string Action,
    string EntityType,
    string EntityId,
    string? OldValues,
    string? NewValues,
    string IpAddress,
    DateTimeOffset Timestamp
);

public record AuditLogFilterRequest(
    string? UserId = null, 
    string? Action = null, 
    DateTimeOffset? StartDate = null, 
    DateTimeOffset? EndDate = null, 
    int Page = 1, 
    int PageSize = 20
);
