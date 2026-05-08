using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Infrastructure.Services.Models;

namespace OutfitPlanner.Infrastructure.Services;

/// <summary>
/// Background job to automatically unlock accounts after lockout period expires
/// </summary>
public class AccountUnlockBackgroundJob
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AccountUnlockBackgroundJob> _logger;

    public AccountUnlockBackgroundJob(UserManager<User> userManager, ILogger<AccountUnlockBackgroundJob> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Automatically unlocks accounts where the lockout period has expired
    /// Runs every 5 minutes via Hangfire
    /// </summary>
    public async Task AutoUnlockAccounts()
    {
        _logger.LogInformation("Starting auto-unlock accounts background job...");

        // Find all locked out users where lockout has expired
        var lockedOutUsers = _userManager.Users
            .Where(u => u.LockoutEnd != null && u.LockoutEnd <= DateTimeOffset.UtcNow)
            .ToList();

        var unlockedCount = 0;

        foreach (var user in lockedOutUsers)
        {
            try
            {
                // Reset lockout
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
                
                _logger.LogInformation("Auto-unlocked account for user {UserId} ({Email})", user.Id, user.Email);
                unlockedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-unlock account for user {UserId}", user.Id);
            }
        }

        _logger.LogInformation("Auto-unlock job completed. Unlocked {Count} accounts.", unlockedCount);
    }

    /// <summary>
    /// Manually unlocks a specific user account (for admin use)
    /// </summary>
    public async Task<bool> ManualUnlockAccount(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Manual unlock failed: User {UserId} not found", userId);
            return false;
        }

        try
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            
            _logger.LogInformation("Manually unlocked account for user {UserId} ({Email})", user.Id, user.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to manually unlock account for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Gets all currently locked out accounts (for admin dashboard)
    /// </summary>
    public async Task<List<LockedOutUserDto>> GetLockedOutAccounts()
    {
        var lockedOutUsers = _userManager.Users
            .Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow)
            .Select(u => new LockedOutUserDto
            {
                UserId = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                LockoutEnd = u.LockoutEnd!.Value,
                TimeRemaining = u.LockoutEnd.Value - DateTimeOffset.UtcNow
            })
            .ToList();

        return lockedOutUsers;
    }
}


