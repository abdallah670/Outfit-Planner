using MediatR;
using OutfitPlanner.Application.DTOs.Admin;

namespace OutfitPlanner.Application.Features.Admin.Requests.Queries;

public record GetUserDetailQuery(string UserId) : IRequest<AdminUserDetailDto>;
