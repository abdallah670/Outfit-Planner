using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.DTOs.Admin;

public record ContentReportDto(
    Guid Id, 
    string? ReporterUserName, 
    string TargetUserId, 
    string ContentType, 
    ReportReason Reason, 
    ReportStatus Status, 
    DateTimeOffset CreatedAt
);

public record ContentReportDetailDto(
    ContentReportDto Report, 
    string? ReporterEmail, 
    string? ContentPreview, 
    string? TargetUserName,
    string? Resolution,
    DateTimeOffset? ResolvedAt
);

public record ReportFilterRequest(
    ReportStatus? Status = null, 
    string? ContentType = null, 
    int Page = 1, 
    int PageSize = 20
);

public record ResolveReportRequest(string Resolution, bool TakeAction);
public record DismissReportRequest(string Reason);
