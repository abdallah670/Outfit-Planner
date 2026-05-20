using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application;
using OutfitPlanner.Application.DTOs.Admin;
using Result = OutfitPlanner.Application.Common.Result;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;

public class UnbanUserCommandHandler : IRequestHandler<UnbanUserCommand, Result>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<UnbanUserCommandHandler> _logger;

    public UnbanUserCommandHandler(UserManager<Domain.Entities.User> userManager, IUnitOfWork unitOfWork, IMediator mediator, ILogger<UnbanUserCommandHandler> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result> Handle(UnbanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<Domain.Entities.User>().GetFirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null)
            return Result.Failure("User not found");
            
        // Remove banned claims
        var claims = await _userManager.GetClaimsAsync(user);
        var bannedClaims = claims.Where(c => c.Type == "Banned" || c.Type == "BanReason" || c.Type == "BanExpiry").ToList();
        
        foreach (var claim in bannedClaims)
        {
            await _userManager.RemoveClaimAsync(user, claim);
        }
        
        await _userManager.UpdateAsync(user);
        
        // Log audit
        await _mediator.Send(new CreateAuditLogCommand(
            "User_Unbanned",
            "User unbanned by admin",
            "System",
            "System",
            "User",
            request.UserId,
            null,
            null,
            ""
        ), cancellationToken);
        
        await _unitOfWork.CompleteAsync();
        
        _logger.LogInformation("User {UserId} unbanned by admin", request.UserId);
        return Result.Success();
    }
}
