using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, BaseCommandResponse>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IUserStyleProfileRepository _styleProfileRepository;
    private readonly IUserPreferencesRepository _preferencesRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(
        UserManager<Domain.Entities.User> userManager,
        IUserStyleProfileRepository styleProfileRepository,
        IUserPreferencesRepository preferencesRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _userManager = userManager;
        _styleProfileRepository = styleProfileRepository;
        _preferencesRepository = preferencesRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        
        if (user == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);
        }

        // Update basic info
        if (!string.IsNullOrEmpty(request.Request.Name))
        {
            user.Name = request.Request.Name;
        }
        
        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        try
        {
            // Update or create style profile
            if (request.Request.StyleProfile != null)
            {
                await UpdateStyleProfile(request.UserId, request.Request.StyleProfile);
            }

            // Update or create preferences
            if (request.Request.Preferences != null)
            {
                await UpdatePreferences(request.UserId, request.Request.Preferences);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user profile for {UserId}", request.UserId);
            return new BaseCommandResponse
            {
                Success = false,
                Message = "An error occurred while saving your profile. Please try again.",
                Errors = new List<string> { ex.Message }
            };
        }

        return new BaseCommandResponse
        {
            Success = true,
            Message = "Profile updated successfully",
            Id = Guid.Parse(user.Id)
        };
    }

    private async Task UpdateStyleProfile(string userId, UpdateStyleProfileDto dto)
    {
        var existing = await _styleProfileRepository.GetByUserIdAsync(userId);
        
        Guid profileId;
        
        if (existing != null)
        {
            if (dto.Style.HasValue) existing.Style = dto.Style.Value;
            if (dto.PreferredColors != null) existing.PreferredColors = dto.PreferredColors;
            if (dto.FitPreferences != null) existing.FitPreferences = dto.FitPreferences;
            if (dto.ComfortPriority.HasValue) existing.ComfortPriority = dto.ComfortPriority.Value;
            if (dto.AcceptsTrends.HasValue) existing.AcceptsTrends = dto.AcceptsTrends.Value;
            
            await _styleProfileRepository.UpdateAsync(existing);
            profileId = existing.Id;
        }
        else
        {
            var newProfile = new Domain.Entities.UserStyleProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Style = dto.Style ?? StylePreference.Classic,
                PreferredColors = dto.PreferredColors ?? new List<string>(),
                FitPreferences = dto.FitPreferences ?? "Regular",
                ComfortPriority = dto.ComfortPriority ?? 50,
                AcceptsTrends = dto.AcceptsTrends ?? false
            };
            await _styleProfileRepository.AddAsync(newProfile);
            profileId = newProfile.Id;
        }
    }

    private async Task UpdatePreferences(string userId, UpdateUserPreferencesDto dto)
    {
        var existing = await _preferencesRepository.GetByUserIdAsync(userId);
        
        if (existing != null)
        {
            if (dto.ShareOutfitsAnonymously.HasValue) existing.ShareOutfitsAnonymously = dto.ShareOutfitsAnonymously.Value;
            if (dto.IncludeInTrendAnalysis.HasValue) existing.IncludeInTrendAnalysis = dto.IncludeInTrendAnalysis.Value;
            if (dto.AllowFriendRequests.HasValue) existing.AllowFriendRequests = dto.AllowFriendRequests.Value;
            if (dto.DefaultOutfitPrivacy.HasValue) existing.DefaultOutfitPrivacy = dto.DefaultOutfitPrivacy.Value;
            if (dto.ShowBodyMetrics.HasValue) existing.ShowBodyMetrics = dto.ShowBodyMetrics.Value;
            if (dto.AllowLocationTracking.HasValue) existing.AllowLocationTracking = dto.AllowLocationTracking.Value;
            
            await _preferencesRepository.UpdateAsync(existing);
        }
        else
        {
            var newPreferences = new Domain.Entities.UserPreferences
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ShareOutfitsAnonymously = dto.ShareOutfitsAnonymously ?? false,
                IncludeInTrendAnalysis = dto.IncludeInTrendAnalysis ?? true,
                AllowFriendRequests = dto.AllowFriendRequests ?? true,
                DefaultOutfitPrivacy = dto.DefaultOutfitPrivacy ?? PrivacyLevel.Private,
                ShowBodyMetrics = dto.ShowBodyMetrics ?? false,
                AllowLocationTracking = dto.AllowLocationTracking ?? false
            };
            await _preferencesRepository.AddAsync(newPreferences);
        }
    }
}
