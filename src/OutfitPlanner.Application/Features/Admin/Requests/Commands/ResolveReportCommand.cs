using MediatR;
using OutfitPlanner.Application.Common;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record ResolveReportCommand(Guid ReportId, string Resolution, bool TakeAction) : IRequest<Result>;
