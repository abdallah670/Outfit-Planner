using MediatR;
using OutfitPlanner.Application.Common;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record BanUserCommand(string UserId, string Reason, DateTime? Expiry = null) : IRequest<Result>;
