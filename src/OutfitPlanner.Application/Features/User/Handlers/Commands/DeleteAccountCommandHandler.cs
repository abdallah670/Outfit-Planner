using MediatR;
using Microsoft.AspNetCore.Identity;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using AppUser = OutfitPlanner.Domain.Entities.User;
using DeleteAccountCommand = OutfitPlanner.Application.Features.User.Requests.Commands.DeleteAccountCommand;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;

    public DeleteAccountCommandHandler(
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<BaseCommandResponse> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();
        
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        
        if (user == null)
        {
            response.Success = false;
            response.Message = "User not found";
            return response;
        }

        IAsyncDisposable? transaction = null;
        
        try
        {
            // Begin transaction to ensure atomic deletes
            transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            // Delete related data first (in case of foreign key constraints)
            // Delete clothing items
            var clothingItems = await _unitOfWork.ClothingItems.FindAsync(c => c.UserId == request.UserId);
            foreach (var item in clothingItems)
            {
                await _unitOfWork.ClothingItems.RemoveAsync(item);
            }

            // Delete outfits
            var outfits = await _unitOfWork.Outfits.FindAsync(o => o.UserId == request.UserId);
            foreach (var outfit in outfits)
            {
                await _unitOfWork.Outfits.RemoveAsync(outfit);
            }

            // Delete wear events
            var wearEvents = await _unitOfWork.WearEvents.FindAsync(w => w.UserId == request.UserId);
            foreach (var wearEvent in wearEvents)
            {
                await _unitOfWork.WearEvents.RemoveAsync(wearEvent);
            }

            // Delete app preferences
            var preferences = await _unitOfWork.AppPreferences.FindAsync(p => p.UserId == request.UserId);
            foreach (var pref in preferences)
            {
                await _unitOfWork.AppPreferences.RemoveAsync(pref);
            }

            // Delete notification settings
            var notificationSettings = await _unitOfWork.NotificationSettings.FindAsync(ns => ns.UserId == request.UserId);
            foreach (var ns in notificationSettings)
            {
                await _unitOfWork.NotificationSettings.RemoveAsync(ns);
            }

            // Delete notifications
            var notifications = await _unitOfWork.Notifications.FindAsync(n => n.UserId == request.UserId);
            foreach (var notification in notifications)
            {
                await _unitOfWork.Notifications.RemoveAsync(notification);
            }

            // Delete the user using UserManager
            var result = await _userManager.DeleteAsync(user);
            
            if (result.Succeeded)
            {
                // Save all changes atomically
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                // Commit the transaction by disposing it
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
                
                response.Success = true;
                response.Message = "Account deleted successfully";
            }
            else
            {
                // Rollback by disposing without committing
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
                
                response.Success = false;
                response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
            }
        }
        catch (Exception ex)
        {
            // Rollback on any error
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }
            
            response.Success = false;
            response.Message = $"Error deleting account: {ex.Message}";
        }

        return response;
    }
}
