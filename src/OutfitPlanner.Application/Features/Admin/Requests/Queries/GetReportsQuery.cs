using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Features.Admin.DTOs;

namespace OutfitPlanner.Application.Features.Admin.Requests.Queries;

public record GetReportsQuery(ReportFilterRequest Filter) : IRequest<PaginatedResult<ContentReportDto>>;
