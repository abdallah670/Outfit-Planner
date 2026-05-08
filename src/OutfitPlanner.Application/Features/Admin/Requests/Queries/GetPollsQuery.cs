using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;

namespace OutfitPlanner.Application.Features.Admin.Requests.Queries;

public record GetPollsQuery(ContentFilterRequest Filter) : IRequest<PaginatedResult<AdminPollDto>>;
