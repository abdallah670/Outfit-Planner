namespace OutfitPlanner.Domain.Entities;

public class UserActivity
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ActivityType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? AdditionalData { get; set; }
}

public enum ActivityType
{
    Login,
    Logout,
    PageView,
    CreateOutfit,
    EditOutfit,
    DeleteOutfit,
    LikeOutfit,
    Comment,
    Follow,
    Unfollow,
    UploadImage,
    Search,
    ViewProfile,
    EditProfile,
    AdminAction,
    Other
}
