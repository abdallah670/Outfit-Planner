using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Infrastructure.Services;

public class CalendarReminderService
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CalendarReminderService> _logger;
    private readonly IClock _clock;

    public CalendarReminderService(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        ILogger<CalendarReminderService> logger,
        IClock clock)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _clock = clock;
    }

    public async Task SendRemindersAsync()
    {
        try
        {
            _logger.LogInformation("Hangfire: Starting calendar reminder job at {Time}", _clock.UtcNow);

            var today = _clock.UtcNow.Date;
            var startOfTomorrow = today.AddDays(1);
            var endOfDayAfterTomorrow = startOfTomorrow.AddDays(2);

            var events = await _unitOfWork.CalendarEvents.GetByDateRangeAsync(
                new DateTimeOffset(today, TimeSpan.Zero),
                new DateTimeOffset(endOfDayAfterTomorrow, TimeSpan.Zero));

            if (!events.Any())
            {
                _logger.LogInformation("Hangfire: No calendar events found for today or tomorrow");
                return;
            }

            var eventsByUser = events.GroupBy(e => e.UserId);

            foreach (var userGroup in eventsByUser)
            {
                var userId = userGroup.Key;
                var todayEvents = userGroup.Where(e => e.EventDate.Date == today.Date).ToList();
                var tomorrowEvents = userGroup.Where(e => e.EventDate.Date == startOfTomorrow.Date).ToList();

                foreach (var calendarEvent in todayEvents)
                {
                    await ProcessReminderAsync(userId, calendarEvent.Id, calendarEvent.Title, "Today");
                }

                foreach (var calendarEvent in tomorrowEvents)
                {
                    var timeText = calendarEvent.StartTime.HasValue
                        ? calendarEvent.StartTime.Value.ToString(@"hh\:mm")
                        : "an unspecified time";
                    await ProcessReminderAsync(userId, calendarEvent.Id, calendarEvent.Title, "Tomorrow", timeText);
                }
            }

            _logger.LogInformation("Hangfire: Calendar reminder job completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hangfire: Error in calendar reminder job");
            throw;
        }
    }

    private async Task ProcessReminderAsync(string userId, Guid calendarEventId, string title, string reminderType, string? timeText = null)
    {
        try
        {
            var alreadySent = await _unitOfWork.SentReminders.ExistsAsync(
                userId, calendarEventId, reminderType, _clock.UtcNow);

            if (alreadySent)
            {
                return;
            }

            string message;
            if (reminderType == "Today")
            {
                message = $"You scheduled \"{title}\" for today. Did you wear it?";
            }
            else
            {
                message = timeText != null
                    ? $"You have \"{title}\" scheduled for tomorrow at {timeText}"
                    : $"You have \"{title}\" scheduled for tomorrow";
            }

            await _mediator.Send(new CreateNotificationCommand
            {
                UserId = userId,
                Request = new CreateNotificationDto
                {
                    Type = NotificationType.Reminder,
                    Title = reminderType == "Today" ? "Calendar Reminder" : "Upcoming Event",
                    Message = message,
                    ActionUrl = "/calendar"
                }
            });

            await _unitOfWork.SentReminders.AddAsync(new SentReminder
            {
                UserId = userId,
                CalendarEventId = calendarEventId,
                ReminderType = reminderType,
                SentAt = _clock.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("User {UserId} not found while sending calendar reminder, skipping", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send calendar reminder for user {UserId}, event {EventId}", userId, calendarEventId);
        }
    }
}
