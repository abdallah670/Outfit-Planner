namespace OutfitPlanner.Domain.Entities;

public enum ReportStatus 
{ 
    Pending, 
    InReview, 
    Resolved, 
    Dismissed 
}

public enum ReportReason 
{ 
    Spam, 
    Harassment, 
    InappropriateContent, 
    FakeAccount, 
    Other 
}

public class ContentReport : BaseEntity
{
    public string ReporterId { get; set; } = string.Empty;
    public string? ReporterUserName { get; set; }
    public string TargetUserId { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? ResolvedById { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
}
