using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application;
using Result = OutfitPlanner.Application.Common.Result;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;

public class ResolveReportCommandHandler : IRequestHandler<ResolveReportCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<ResolveReportCommandHandler> _logger;

    public ResolveReportCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, ILogger<ResolveReportCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result> Handle(ResolveReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _unitOfWork.Repository<ContentReport>()
            .GetFirstOrDefaultAsync(r => r.Id == request.ReportId);
            
        if (report == null)
            return Result.Failure("Report not found");
        
        var oldValues = System.Text.Json.JsonSerializer.Serialize(report);
        
        report.Status = ReportStatus.Resolved;
        report.Resolution = request.Resolution;
        report.ResolvedAt = DateTimeOffset.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        // Take action if requested
        if (request.TakeAction)
        {
            await TakeActionOnReport(report, cancellationToken);
        }
        
        // Log audit
        await _mediator.Send(new CreateAuditLogCommand(
            "",
            "",
            "Report_Resolved",
            "ContentReport",
            request.ReportId.ToString(),
            oldValues,
            System.Text.Json.JsonSerializer.Serialize(report),
            ""
        ), cancellationToken);
        
        _logger.LogInformation("Report {ReportId} resolved", request.ReportId);
        return Result.Success();
    }
    
    private async Task TakeActionOnReport(ContentReport report, CancellationToken cancellationToken)
    {
        switch (report.Reason)
        {
            case ReportReason.Harassment:
            // Ban the user
                await _mediator.Send(new BanUserCommand(report.TargetUserId, "Auto-banned due to harassment report", DateTimeOffset.UtcNow.AddDays(30)));
                break;
                
            case ReportReason.InappropriateContent:
            case ReportReason.Spam:
                // Delete the content
                await DeleteContent(report.ContentType, report.ContentId, cancellationToken);
                break;
                
            case ReportReason.FakeAccount:
                // Ban the user permanently
                await _mediator.Send(new BanUserCommand(report.TargetUserId, "Auto-banned due to fake account report", null));
                break;
        }
    }
    
    private async Task DeleteContent(string contentType, string contentId, CancellationToken cancellationToken)
    {
        switch (contentType)
        {
            case "FeedPost":
                var post = await _context.FeedPosts.FindAsync(contentId);
                if (post != null)
                    _context.FeedPosts.Remove(post);
                break;
                
            case "Poll":
                var poll = await _context.Polls.FindAsync(contentId);
                if (poll != null)
                    _context.Polls.Remove(poll);
                break;
                
            case "Comment":
                var comment = await _context.PostComments.FindAsync(contentId);
                if (comment != null)
                    _context.PostComments.Remove(comment);
                break;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}
