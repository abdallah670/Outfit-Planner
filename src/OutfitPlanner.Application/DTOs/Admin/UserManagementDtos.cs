namespace OutfitPlanner.Application.Features.Admin.DTOs;

public record AdminUserDto(
    string Id, 
    string UserName, 
    string Email, 
    string Name, 
    List<string> Roles, 
    bool IsLocked, 
    bool IsBanned, 
    DateTimeOffset CreatedAt
);

public record AdminUserDetailDto(
    AdminUserDto User, 
    int OutfitCount, 
    int PostCount, 
    int CommentCount, 
    List<AuditLogDto> RecentActivity
);

public record UserFilterRequest(
    string? Search = null, 
    string? Role = null, 
    string? Status = null, 
    int Page = 1, 
    int PageSize = 20
);

public record LockedAccountDto(
    string UserId,
    string? UserName,
    string? Email,
    DateTimeOffset LockoutEnd,
    TimeSpan TimeRemaining
);

public record BanUserRequest(string Reason, DateTimeOffset? Expiry);
public record ChangeRoleRequest(string NewRole);
