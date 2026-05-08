using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;

namespace OutfitPlanner.Application.Features.Admin.Requests.Queries;

public record GetOutfitsQuery(ContentFilterRequest Filter) : IRequest<PaginatedResult<AdminOutfitDto>>;
