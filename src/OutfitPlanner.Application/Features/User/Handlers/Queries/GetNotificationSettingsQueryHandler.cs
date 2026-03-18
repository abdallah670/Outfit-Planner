using MediatR;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Features.User.Requests.Queries;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class GetNotificationSettingsQueryHandler : IRequestHandler<GetNotificationSettingsQuery, NotificationSettingsDto>
{
    private readonly INotificationSettingsRepository _notificationSettingsRepository;

    public GetNotificationSettingsQueryHandler(INotificationSettingsRepository notificationSettingsRepository)
    {
        _notificationSettingsRepository = notificationSettingsRepository;
    }

    public async Task<NotificationSettingsDto> Handle(GetNotificationSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _notificationSettingsRepository.GetByUserIdAsync(request.UserId);

        if (settings == null)
        {
            // Return default settings (matches the entity default values)
            return new NotificationSettingsDto
            {
                DailyOutfitSuggestion = true,
                WeeklyStyleReport = false,
                WeatherAlerts = true,
                NewFeatures = true,
                SocialNotifications = true,
                PushNotificationsEnabled = true
            };
        }

        return new NotificationSettingsDto
        {
            DailyOutfitSuggestion = settings.DailyOutfitSuggestion,
            WeeklyStyleReport = settings.WeeklyStyleReport,
            WeatherAlerts = settings.WeatherAlerts,
            NewFeatures = settings.NewFeatures,
            SocialNotifications = settings.SocialNotifications,
            PushNotificationsEnabled = settings.PushNotificationsEnabled
        };
    }
}
