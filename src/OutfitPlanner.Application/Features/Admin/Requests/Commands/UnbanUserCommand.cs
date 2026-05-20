using MediatR;
using OutfitPlanner.Application.Common;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record UnbanUserCommand(string UserId) : IRequest<Result>;
