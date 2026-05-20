using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record UpdateSettingCommand(string Key, string Value) : IRequest<Result>;
