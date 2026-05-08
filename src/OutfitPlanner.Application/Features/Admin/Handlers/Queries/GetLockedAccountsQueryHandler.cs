using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Admin.DTOs;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetLockedAccountsQueryHandler : IRequestHandler<GetLockedAccountsQuery, List<LockedAccountDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetLockedAccountsQueryHandler> _logger;

    public GetLockedAccountsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetLockedAccountsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<LockedAccountDto>> Handle(GetLockedAccountsQuery request, CancellationToken cancellationToken)
    {
        var lockedUsers = await _unitOfWork.Repository<User>()
            .GetQueryable(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow)
            .Select(u => new LockedAccountDto(
                u.Id,
                u.UserName!,
                u.Email!,
                u.LockoutEnd!.Value,
                "Account locked due to security policy"
            ))
            .ToListAsync(cancellationToken);
            
        return lockedUsers.ToList();
    }
}
