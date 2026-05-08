using MediatR;
using OutfitPlanner.Application.DTOs;
using OutfitPlanner.Application.DTOs.Admin;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record ExportAnalyticsCommand(string Format, DateTime? StartDate, DateTime? EndDate) : IRequest<AnalyticsExportResult>;

public record AnalyticsExportResult(
    string FileName,
    string ContentType,
    byte[] FileContents
);

public record RealtimeAnalyticsDto(
    UserEngagementMetrics UserMetrics,
    ContentMetrics ContentStats,
    SystemPerformanceMetrics SystemStats,
    DateTime LastUpdated
);
