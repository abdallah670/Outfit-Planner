using MediatR;
using Microsoft.AspNetCore.Identity;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, BaseCommandResponse>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IUserStyleProfileRepository _styleProfileRepository;
    private readonly IUserPreferencesRepository _preferencesRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserProfileCommandHandler(
        UserManager<Domain.Entities.User> userManager,
        IUserStyleProfileRepository styleProfileRepository,
        IUserPreferencesRepository preferencesRepository,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _styleProfileRepository = styleProfileRepository;
        _preferencesRepository = preferencesRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        
        if (user == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);
        }

        // Update basic info
        user.Name = request.Request.Name;
        
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

        return new BaseCommandResponse
        {
            Success = true,
            Message = "Profile updated successfully",
            Id = Guid.Parse(user.Id)
        };
    }

    private async Task UpdateStyleProfile(string userId, UserStyleProfileDto dto)
    {
        var existing = await _styleProfileRepository.GetByUserIdAsync(userId);
        
        if (existing != null)
        {
            existing.Style = dto.Style;
            existing.PreferredColors = dto.PreferredColors;
            existing.FitPreferences = dto.FitPreferences;
            existing.ComfortPriority = dto.ComfortPriority;
            existing.AcceptsTrends = dto.AcceptsTrends;
            await _styleProfileRepository.UpdateAsync(existing);
        }
        else
        {
            var newProfile = new Domain.Entities.UserStyleProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Style = dto.Style,
                PreferredColors = dto.PreferredColors,
                FitPreferences = dto.FitPreferences,
                ComfortPriority = dto.ComfortPriority,
                AcceptsTrends = dto.AcceptsTrends
            };
            await _styleProfileRepository.AddAsync(newProfile);
        }
    }

    private async Task UpdatePreferences(string userId, UserPreferencesDto dto)
    {
        var existing = await _preferencesRepository.GetByUserIdAsync(userId);
        
        if (existing != null)
        {
            existing.ShareOutfitsAnonymously = dto.ShareOutfitsAnonymously;
            existing.IncludeInTrendAnalysis = dto.IncludeInTrendAnalysis;
            existing.AllowFriendRequests = dto.AllowFriendRequests;
            existing.DefaultOutfitPrivacy = dto.DefaultOutfitPrivacy;
            existing.ShowBodyMetrics = dto.ShowBodyMetrics;
            existing.AllowLocationTracking = dto.AllowLocationTracking;
            await _preferencesRepository.UpdateAsync(existing);
        }
        else
        {
            var newPreferences = new Domain.Entities.UserPreferences
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ShareOutfitsAnonymously = dto.ShareOutfitsAnonymously,
                IncludeInTrendAnalysis = dto.IncludeInTrendAnalysis,
                AllowFriendRequests = dto.AllowFriendRequests,
                DefaultOutfitPrivacy = dto.DefaultOutfitPrivacy,
                ShowBodyMetrics = dto.ShowBodyMetrics,
                AllowLocationTracking = dto.AllowLocationTracking
            };
            await _preferencesRepository.AddAsync(newPreferences);
        }
    }
}
