using MediatR;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Application;
using Result = OutfitPlanner.Application.Common.Result;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;


public class ExportAnalyticsCommandHandler : IRequestHandler<ExportAnalyticsCommand, AnalyticsExportResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExportAnalyticsCommandHandler> _logger;

    public ExportAnalyticsCommandHandler(IUnitOfWork unitOfWork, ILogger<ExportAnalyticsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AnalyticsExportResult> Handle(ExportAnalyticsCommand request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get analytics data
        var analyticsQuery = new GetDetailedAnalyticsQuery(new AnalyticsFilterRequest(startDate, endDate));
        var analyticsHandler = new GetDetailedAnalyticsQueryHandler(_unitOfWork);
        var analytics = await analyticsHandler.Handle(analyticsQuery, cancellationToken);

        byte[] fileContents;
        string contentType;
        string fileName;

        switch (request.Format.ToLower())
        {
            case "csv":
                (fileContents, contentType, fileName) = ExportToCsv(analytics);
                break;
            case "json":
                (fileContents, contentType, fileName) = ExportToJson(analytics);
                break;
            case "pdf":
                (fileContents, contentType, fileName) = ExportToPdf(analytics);
                break;
            default:
                throw new ArgumentException($"Unsupported export format: {request.Format}");
        }

        return new AnalyticsExportResult(fileName, contentType, fileContents);
    }

    private (byte[] contents, string contentType, string fileName) ExportToCsv(DetailedAnalyticsDto analytics)
    {
        var csv = new StringBuilder();
        
        // Headers
        csv.AppendLine("Category,Metric,Value,Change %");
        
        // User metrics
        csv.AppendLine($"Users,Total Users,{analytics.UserMetrics.TotalUsers},0");
        csv.AppendLine($"Users,Active Users,{analytics.UserMetrics.ActiveUsers},0");
        csv.AppendLine($"Users,New Users,{analytics.UserMetrics.NewUsers},{analytics.UserMetrics.UserGrowthRate:F2}");
        
        // Content metrics
        csv.AppendLine($"Content,Total Outfits,{analytics.ContentStats.TotalOutfits},0");
        csv.AppendLine($"Content,Total Posts,{analytics.ContentStats.TotalPosts},0");
        csv.AppendLine($"Content,Total Polls,{analytics.ContentStats.TotalPolls},0");
        csv.AppendLine($"Content,Total Likes,{analytics.ContentStats.TotalLikes},0");
        csv.AppendLine($"Content,Engagement Rate,{analytics.ContentStats.EngagementRate:F2},0");
        
        // System metrics
        csv.AppendLine($"System,CPU Usage,{analytics.SystemStats.CpuUsage:F2},0");
        csv.AppendLine($"System,Memory Usage,{analytics.SystemStats.MemoryUsage},0");
        csv.AppendLine($"System,Response Time,{analytics.SystemStats.ResponseTime:F2},0");

        var contents = Encoding.UTF8.GetBytes(csv.ToString());
        return (contents, "text/csv", $"analytics_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private (byte[] contents, string contentType, string fileName) ExportToJson(DetailedAnalyticsDto analytics)
    {
        var json = JsonSerializer.Serialize(analytics, new JsonSerializerOptions { WriteIndented = true });
        var contents = Encoding.UTF8.GetBytes(json);
        return (contents, "application/json", $"analytics_{DateTime.UtcNow:yyyyMMdd}.json");
    }

    private (byte[] contents, string contentType, string fileName) ExportToPdf(DetailedAnalyticsDto analytics)
    {
        // For PDF export, you would typically use a library like iTextSharp or PdfSharp
        // For now, we'll create a simple text-based representation
        var pdfContent = new StringBuilder();
        
        pdfContent.AppendLine("Analytics Report");
        pdfContent.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        pdfContent.AppendLine();
        
        pdfContent.AppendLine("User Metrics");
        pdfContent.AppendLine($"Total Users: {analytics.UserMetrics.TotalUsers}");
        pdfContent.AppendLine($"Active Users: {analytics.UserMetrics.ActiveUsers}");
        pdfContent.AppendLine($"New Users: {analytics.UserMetrics.NewUsers}");
        pdfContent.AppendLine($"Growth Rate: {analytics.UserMetrics.UserGrowthRate:F2}%");
        pdfContent.AppendLine();
        
        pdfContent.AppendLine("Content Metrics");
        pdfContent.AppendLine($"Total Outfits: {analytics.ContentStats.TotalOutfits}");
        pdfContent.AppendLine($"Total Posts: {analytics.ContentStats.TotalPosts}");
        pdfContent.AppendLine($"Total Polls: {analytics.ContentStats.TotalPolls}");
        pdfContent.AppendLine($"Total Likes: {analytics.ContentStats.TotalLikes}");
        pdfContent.AppendLine($"Engagement Rate: {analytics.ContentStats.EngagementRate:F2}%");
        pdfContent.AppendLine();
        
        pdfContent.AppendLine("System Performance");
        pdfContent.AppendLine($"CPU Usage: {analytics.SystemStats.CpuUsage:F2}%");
        pdfContent.AppendLine($"Memory Usage: {analytics.SystemStats.MemoryUsage / 1024 / 1024}MB");
        pdfContent.AppendLine($"Response Time: {analytics.SystemStats.ResponseTime:F2}ms");

        var contents = Encoding.UTF8.GetBytes(pdfContent.ToString());
        return (contents, "text/plain", $"analytics_{DateTime.UtcNow:yyyyMMdd}.txt");
    }
}
