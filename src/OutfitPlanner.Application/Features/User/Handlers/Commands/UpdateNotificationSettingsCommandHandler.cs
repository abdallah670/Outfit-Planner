using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class UpdateNotificationSettingsCommandHandler : IRequestHandler<UpdateNotificationSettingsCommand, BaseCommandResponse>
{
    private readonly INotificationSettingsRepository _notificationSettingsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNotificationSettingsCommandHandler(
        INotificationSettingsRepository notificationSettingsRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationSettingsRepository = notificationSettingsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _notificationSettingsRepository.GetByUserIdAsync(request.UserId);
        
        if (settings == null)
        {
            // Create new notification settings
            settings = new NotificationSettings
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                DailyOutfitSuggestion = request.Settings.DailyOutfitSuggestion,
                WeeklyStyleReport = request.Settings.WeeklyStyleReport,
                WeatherAlerts = request.Settings.WeatherAlerts,
                NewFeatures = request.Settings.NewFeatures,
                SocialNotifications = request.Settings.SocialNotifications,
                PushNotificationsEnabled = request.Settings.PushNotificationsEnabled,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _notificationSettingsRepository.AddAsync(settings);
        }
        else
        {
            // Update existing notification settings
            settings.DailyOutfitSuggestion = request.Settings.DailyOutfitSuggestion;
            settings.WeeklyStyleReport = request.Settings.WeeklyStyleReport;
            settings.WeatherAlerts = request.Settings.WeatherAlerts;
            settings.NewFeatures = request.Settings.NewFeatures;
            settings.SocialNotifications = request.Settings.SocialNotifications;
            settings.PushNotificationsEnabled = request.Settings.PushNotificationsEnabled;
            settings.UpdatedAt = DateTime.UtcNow;
            await _notificationSettingsRepository.UpdateAsync(settings);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BaseCommandResponse
        {
            Success = true,
            Message = "Notification settings updated successfully"
        };
    }
}
