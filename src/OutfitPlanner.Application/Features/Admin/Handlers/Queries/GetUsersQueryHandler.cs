using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Application;
using OutfitPlanner.Domain.Entities;
using User = OutfitPlanner.Domain.Entities.User;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedResult<AdminUserDto>>
{
    private readonly UserManager<OutfitPlanner.Domain.Entities.User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(UserManager<OutfitPlanner.Domain.Entities.User> userManager, IUnitOfWork unitOfWork, ILogger<GetUsersQueryHandler> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResult<AdminUserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable();
        
        // Apply search filter
        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            query = query.Where(u => 
                u.UserName!.Contains(request.Filter.Search) || 
                u.Email!.Contains(request.Filter.Search) ||
                u.Name!.Contains(request.Filter.Search));
        }
        
        // Apply role filter (using Roles JSON array in IdentityUser)
        if (!string.IsNullOrEmpty(request.Filter.Role))
        {
            // Assuming Roles stored as JSON array in IdentityUser.Roles (if custom)
            // For now, use UserManager to check roles after fetching - not efficient
            // This is a placeholder; ideally you'd join with UserRoles table
        }
        
        // Apply status filter
        switch (request.Filter.Status?.ToLower())
        {
            case "locked":
                query = query.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);
                break;
            case "banned":
                // Assuming Banned claim stored as JSON in Claims (if used)
                // query = query.Where(u => EF.Functions.JsonContains(u.Claims, "\"Banned\":\"true\""));
                break;
            case "active":
                query = query.Where(u => 
                    (u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow));
                break;
        }
        
        var total = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .ToListAsync(cancellationToken);
        
        var userDtos = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;
            var isBanned = claims.Any(c => c.Type == "Banned" && c.Value == "true");
            
            userDtos.Add(new AdminUserDto(
                user.Id,
                user.UserName ?? "Unknown",
                user.Email ?? "Unknown",
                user.Name,
                roles.ToList(),
                isLocked,
                isBanned,
                user.CreatedAt
            ));
        }
        
        return new PaginatedResult<AdminUserDto>(userDtos, total, request.Filter.Page, request.Filter.PageSize);
    }
}

