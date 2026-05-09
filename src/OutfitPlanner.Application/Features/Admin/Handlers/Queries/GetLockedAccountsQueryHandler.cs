using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.DTOs;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Application;
using OutfitPlanner.Domain.Entities;
using User = OutfitPlanner.Domain.Entities.User;

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
        var lockedUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>()
            .GetQueryable()
            .Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow)
            .Select(u => new LockedAccountDto(
                Guid.Parse(u.Id),
                u.UserName,
                u.Email,
                u.LockoutEnd!.Value,
                u.LockoutEnd.Value - DateTimeOffset.UtcNow
            ))
            .ToListAsync(cancellationToken);
            
        return lockedUsers;
    }
}
