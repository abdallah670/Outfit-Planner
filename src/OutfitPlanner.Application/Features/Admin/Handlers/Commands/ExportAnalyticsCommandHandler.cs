using MediatR;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Application.Features.Admin.Handlers.Queries;
using OutfitPlanner.Application;
using static OutfitPlanner.Application.Common.Result;

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
        var analyticsLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<GetDetailedAnalyticsQueryHandler>.Instance;
        var analyticsHandler = new GetDetailedAnalyticsQueryHandler(_unitOfWork, analyticsLogger);
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

    private (byte[] contents, string contentType, string fileName) ExportToCsv(OutfitPlanner.Application.DTOs.Admin.DetailedAnalyticsDto analytics)
    {
        var csv = new StringBuilder();
        
        // Headers
        csv.AppendLine("Category,Metric,Value,Change %");
        
        // User metrics
        csv.AppendLine($"Users,Total Users,{analytics.UserMetrics.TotalUsers},0");
        csv.AppendLine($"Users,Active Users,{analytics.UserMetrics.ActiveUsers},0");
        csv.AppendLine($"Users,New Users,{analytics.UserMetrics.NewUsers},{analytics.UserMetrics.UserGrowthRate:F2}");
        
        // Content metrics
        csv.AppendLine($"Content,Total Outfits,{analytics.ContentMetrics.TotalOutfits},0");
        csv.AppendLine($"Content,Total Posts,{analytics.ContentMetrics.TotalPosts},0");
        csv.AppendLine($"Content,Total Polls,{analytics.ContentMetrics.TotalPolls},0");
        csv.AppendLine($"Content,Total Likes,{analytics.ContentMetrics.TotalLikes},0");
        

        var contents = Encoding.UTF8.GetBytes(csv.ToString());
        return (contents, "text/csv", $"analytics_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private (byte[] contents, string contentType, string fileName) ExportToJson(OutfitPlanner.Application.DTOs.Admin.DetailedAnalyticsDto analytics)
    {
        var json = JsonSerializer.Serialize(analytics, new JsonSerializerOptions { WriteIndented = true });
        var contents = Encoding.UTF8.GetBytes(json);
        return (contents, "application/json", $"analytics_{DateTime.UtcNow:yyyyMMdd}.json");
    }

    private (byte[] contents, string contentType, string fileName) ExportToPdf(OutfitPlanner.Application.DTOs.Admin.DetailedAnalyticsDto analytics)
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
        pdfContent.AppendLine($"Total Outfits: {analytics.ContentMetrics.TotalOutfits}");
        pdfContent.AppendLine($"Total Posts: {analytics.ContentMetrics.TotalPosts}");
        pdfContent.AppendLine($"Total Polls: {analytics.ContentMetrics.TotalPolls}");
        pdfContent.AppendLine($"Total Likes: {analytics.ContentMetrics.TotalLikes}");
        pdfContent.AppendLine($"Engagement Rate: {analytics.ContentMetrics.EngagementRate:F2}%");

        var contents = Encoding.UTF8.GetBytes(pdfContent.ToString());
        return (contents, "text/plain", $"analytics_{DateTime.UtcNow:yyyyMMdd}.txt");
    }
}
