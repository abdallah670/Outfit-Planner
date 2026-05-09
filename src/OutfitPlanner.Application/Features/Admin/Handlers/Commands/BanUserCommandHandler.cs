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

public class BanUserCommandHandler : IRequestHandler<BanUserCommand, Result>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<BanUserCommandHandler> _logger;

    public BanUserCommandHandler(UserManager<Domain.Entities.User> userManager, IUnitOfWork unitOfWork, IMediator mediator, ILogger<BanUserCommandHandler> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<Domain.Entities.User>().GetFirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null)
            return Result.Failure("User not found");
            
        // Add banned claim
        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Banned", "true"));
        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("BanReason", request.Reason));
        if (request.Expiry.HasValue)
        {
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("BanExpiry", request.Expiry.Value.ToString("O")));
        }
        
        // Update user to reflect ban status
        await _userManager.UpdateAsync(user);
        
        // Log audit
        await _mediator.Send(new CreateAuditLogCommand(
            "User_Banned",
            $"User banned. Reason: {request.Reason}",
            request.UserId,
            user.UserName,
            "User",
            request.UserId,
            null,
            System.Text.Json.JsonSerializer.Serialize(new { reason = request.Reason, expiry = request.Expiry }),
            ""
        ), cancellationToken);
        
        await _unitOfWork.CompleteAsync();
        
        _logger.LogInformation("User {UserId} banned by admin", request.UserId);
        return Result.Success();
    }
}
