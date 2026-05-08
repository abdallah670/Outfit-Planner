using MediatR;
using OutfitPlanner.Application.Features.Admin.DTOs;

namespace OutfitPlanner.Application.Features.Admin.Requests.Queries;

public record GetReportDetailQuery(Guid ReportId) : IRequest<ContentReportDetailDto>;
