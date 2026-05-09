using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.DTOs;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetUserDetailQueryHandler : IRequestHandler<GetUserDetailQuery, AdminUserDetailDto>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserDetailQueryHandler> _logger;

    public GetUserDetailQueryHandler(UserManager<Domain.Entities.User> userManager, IUnitOfWork unitOfWork, ILogger<GetUserDetailQueryHandler> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AdminUserDetailDto> Handle(GetUserDetailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            throw new Exception("User not found");
            
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;
        var isBanned = claims.Any(c => c.Type == "Banned" && c.Value == "true");
        
        // Get user statistics
        var outfitCount = await _unitOfWork.Repository<Outfit>()
            .CountAsync(o => o.UserId == request.UserId, cancellationToken);
        var postCount = await _unitOfWork.Repository<FeedPost>()
            .CountAsync(p => p.UserId == request.UserId, cancellationToken);
        var CommentsCount = await _unitOfWork.Repository<PostComment>()
            .CountAsync(c => c.UserId == request.UserId, cancellationToken);
            
        // Get recent activity (last 10 audit logs)
        var recentActivity = await _unitOfWork.Repository<AuditLog>()
            .GetQueryable()
            .Where(a => a.UserId == request.UserId)
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .Select(a => new AuditLogDto(a.Id, a.UserName, a.Action, a.EntityType, a.Timestamp))
            .ToListAsync(cancellationToken);
        
        var adminUserDto = new AdminUserDto(
            user.Id,
            user.UserName!,
            user.Email!,
            user.Name,
            roles.ToList(),
            isLocked,
            isBanned,
            user.CreatedAt
        );
            
        return new AdminUserDetailDto(
            adminUserDto,
            outfitCount,
            postCount,
            CommentsCount,
            recentActivity
        );
    }
}
