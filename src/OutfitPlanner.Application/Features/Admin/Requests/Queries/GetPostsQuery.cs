using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;

namespace OutfitPlanner.Application.Features.Admin.Requests.Queries;

public record GetPostsQuery(ContentFilterRequest Filter) : IRequest<PaginatedResult<AdminPostDto>>;
