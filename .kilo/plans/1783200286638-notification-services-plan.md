# Notification Background Services Implementation Plan

> Scope: Create `CalendarReminderService`, `WeeklyReportService`, and supporting infrastructure for Hangfire-scheduled notification jobs.

## Decisions Made

| Decision | Choice |
|----------|--------|
| Service location | `OutfitPlanner.Infrastructure/Services/` (matches existing Hangfire job pattern) |
| Calendar deduplication | New `SentReminder` entity + repository; checked before each send |
| Wear milestone | Already implemented in `RecordWearCommandHandler` (lines 82–104) — no further action needed |
| Weekly report trend | `{trend}` = user's `UserStyleProfile.Style` enum name (e.g. `Classic`, `Minimalist`) with fallback `"Mixed"` |
| Job timezone | UTC (`TimeZoneInfo.Utc`) to match existing jobs |
| Stats page | Separate frontend design needed for `/profile/stats`; not included here |

## Prerequisites

1. **Profile/stats page**: `WeeklyReportService` ActionUrl `/profile/stats` does not exist. Frontend must create at least a minimal route/component, or the notification will link to a dead page. Flagged as out of scope for this plan — see `plans/design.md` for the design doc pattern.

## Task 1 — SentReminder Entity & Repository

**New domain entity**
- File: `src/OutfitPlanner.Domain/Entities/SentReminder.cs`
- Properties: `UserId` (string), `CalendarEventId` (Guid?), `ReminderType` (string: `"Today"` / `"Tomorrow"`), `SentAt` (DateTimeOffset)
- Inherits `BaseEntity`

**New repository contract**
- File: `src/OutfitPlanner.Application/Contracts/Persistence/ISentReminderRepository.cs`
- Interface: `ISentReminderRepository : IGenericRepository<SentReminder>`
- Method: `Task<bool> ExistsAsync(string userId, Guid calendarEventId, string reminderType, DateTimeOffset date)`

**New repository implementation**
- File: `src/OutfitPlanner.Persistence/Repositories/SentReminderRepository.cs`
- Implements `ExistsAsync` by querying `SentReminders` where `UserId == userId`, `CalendarEventId == calendarEventId`, `ReminderType == reminderType`, and `SentAt.Date == date.Date`

**Register in DI**
- `src/OutfitPlanner.Persistence/DependencyInjection.cs`: add `services.AddScoped<ISentReminderRepository, SentReminderRepository>();`

**Add to UnitOfWork**
- `src/OutfitPlanner.Application/Contracts/Persistence/IUnitOfWork.cs`: add `ISentReminderRepository SentReminders { get; }`
- `src/OutfitPlanner.Persistence/Repositories/UnitOfWork.cs`: add property + constructor parameter

**EF Migration**
- Add migration: `AddSentReminderEntity`
- `SentReminders` table with columns: `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, `DeletedAt`, `UserId`, `CalendarEventId`, `ReminderType`, `SentAt`

## Task 2 — CalendarEventRepository Extension

**File**: `src/OutfitPlanner.Application/Contracts/Persistence/ICalendarEventRepository.cs`

Add method:
```csharp
Task<IEnumerable<CalendarEvent>> GetByUserIdAndDateRangeAsync(string userId, DateTimeOffset startDate, DateTimeOffset endDate);
```

**File**: `src/OutfitPlanner.Persistence/Repositories/CalendarEventRepository.cs`

Implement using `_dbSet.Include(e => e.WearEvent).ThenInclude(we => we!.Outfit)` with `EventDate.Date >= startDate.Date && EventDate.Date <= endDate.Date`, ordered by `EventDate` then `StartTime`.

## Task 3 — CalendarReminderService

**New file**: `src/OutfitPlanner.Infrastructure/Services/CalendarReminderService.cs`

Dependencies:
- `IMediator`
- `IUnitOfWork`
- `ILogger<CalendarReminderService>`
- `ICurrentTimeService` (new, see Task 5) OR inject `DateTimeOffset.UtcNow` directly per run

Logic:
1. `StartTime = today 00:00 UTC`, `EndTime = tomorrow 23:59:59 UTC`
2. Fetch all calendar events where `EventDate` is in `[StartTime, EndTime]` across **all users** (single query via `_unitOfWork.CalendarEvents.GetQueryable()` with date range filter)
3. Group events by `UserId`
4. For each user's events:
   a. Separate into `todayEvents` and `tomorrowEvents`
   b. For each `todayEvent`:
      - Check `SentReminder.ExistsAsync(userId, event.Id, "Today", todayDate)`
      - If not exists: send `CreateNotificationCommand` with `Type = Reminder`, message `"You scheduled \"{Title}\" for today. Did you wear it?"`, `ActionUrl = "/calendar"`. Then create `SentReminder`.
   c. For each `tomorrowEvent`:
      - Check `SentReminder.ExistsAsync(userId, event.Id, "Tomorrow", todayDate)`
      - If not exists: send `CreateNotificationCommand` with `Type = Reminder`, message `"You have \"{Title}\" scheduled for tomorrow at {StartTime}"` (if `StartTime == null`, omit time), `ActionUrl = "/calendar"`. Then create `SentReminder`.
5. Wrap in try/catch with logging. Failure for one user should not block others.

**Edge cases**:
- Recurring events: treat each occurrence as a separate event; dedup key includes `CalendarEventId`, so each occurrence in the DB gets its own reminder.
- `StartTime == null`: use `"at an unspecified time"` or omit the time clause.
- Deleted users: `CalendarEvent.UserId` may reference a non-existent user. `CreateNotificationCommandHandler` validates user existence and throws `NotFoundException`. Catch and log, skip.

## Task 4 — WeeklyReportService

**New file**: `src/OutfitPlanner.Infrastructure/Services/WeeklyReportService.cs`

Dependencies:
- `IMediator`
- `IUnitOfWork`
- `ILogger<WeeklyReportService>`

Logic:
1. `weekEnd = today 00:00 UTC`, `weekStart = weekEnd - 7 days`
2. Fetch all users: `await _unitOfWork.Users.GetAllAsync()`
3. Fetch all `WearEvent` records where `WornAt >= weekStart && WornAt < weekEnd` (single query)
4. Group wear events by `UserId` in memory
5. For each user with at least 1 event:
   a. Group events by `ClothingItemId`
   b. Find `topItemId` with highest count; fetch `ClothingItem` by id to get `Name`
   c. `mostWornCount` = count for `topItemId`
   d. `uniqueItems` = number of distinct `ClothingItemId` values
   e. `totalWears` = total event count
   f. `varietyScore` = `(double)uniqueItems / totalWears` (0.0 – 1.0)
   g. `comfortAvg` = average of `Rating` values (Rating is 1–5)
   h. Determine `{trend}`:
      - If user has `UserStyleProfile`, use `StyleProfile.Style.ToString()` (e.g., `"Classic"`)
      - Else if `varietyScore > 0.7`, use `"Versatile"`
      - Else if `varietyScore < 0.3`, use `"Focused"`
      - Else use `"Mixed"`
   i. Build message: `"You wore {itemName} {count} times last week. Your style: {trend}."`
   j. Send `CreateNotificationCommand` with `Type = System`, above title/message, `ActionUrl = "/profile/stats"`
6. Log summary (users processed, failures).

**Performance note**: Loading all users + all weekly wear events could be large. For production scale, add batching (e.g., 100 users per iteration) or a dedicated read model. Flagged as risk for large user bases.

**Edge cases**:
- User with no wear events last week: skip
- `topItemId` is null (no item associated): skip or use `"your favorite item"`
- `Rating` all zeros: `comfortAvg` is 0; handle with `DefaultIfEmpty(0).Average()`

## Task 5 — Time Abstraction

To make unit testing easier, extract current time into an interface.

**New contract**: `src/OutfitPlanner.Application/Contracts/Infrastructure/IClock.cs`
```csharp
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
```

**Implementation**: `src/OutfitPlanner.Infrastructure/Services/SystemClock.cs`
```csharp
public class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
```

Register as singleton in `src/OutfitPlanner.Infrastructure/DependencyInjection.cs`:
```csharp
services.AddSingleton<IClock, SystemClock>();
```

Both `CalendarReminderService` and `WeeklyReportService` inject `IClock` instead of calling `DateTimeOffset.UtcNow` directly.

## Task 6 — Register Services & Hangfire Jobs

**File**: `src/OutfitPlanner.Api/Program.cs`

Add `using` for new services.

Register new background services as `Transient`:
```csharp
builder.Services.AddTransient<CalendarReminderService>();
builder.Services.AddTransient<WeeklyReportService>();
```

In startup block, after existing job registrations:

```csharp
// Calendar reminder job - every hour
using (var scope = app.Services.CreateScope())
{
    var calendarJob = scope.ServiceProvider.GetRequiredService<CalendarReminderService>();
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate(
        "calendar-event-reminders",
        () => calendarJob.SendRemindersAsync(),
        "0 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
    );
    Log.Information("Hangfire recurring job 'calendar-event-reminders' scheduled to run every hour");
}

// Weekly style report job - every Monday at 08:00 UTC
using (var scope = app.Services.CreateScope())
{
    var reportJob = scope.ServiceProvider.GetRequiredService<WeeklyReportService>();
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate(
        "weekly-style-report",
        () => reportJob.GenerateWeeklyReportAsync(),
        "0 8 * * 1",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
    );
    Log.Information("Hangfire recurring job 'weekly-style-report' scheduled to run every Monday at 08:00 UTC");
}
```

## Task 7 — Verify Existing Wear Milestone

`RecordWearCommandHandler` already contains milestone logic at lines 82–104. No changes required. Verify it still compiles and behaves correctly after other plan items are merged.

## Migration & DB Considerations

- New migration: `AddSentReminderEntity`
- No schema changes to existing tables required
- `CalendarEvent.EventDate` and `WearEvent.WornAt` are already `DateTimeOffset`, so range queries align correctly with UTC comparisons

## Testing Strategy

| Test | Target |
|------|--------|
| Unit | `CalendarReminderService` — verify dedup prevents duplicate sends |
| Unit | `CalendarReminderService` — verify "Today" vs "Tomorrow" message format |
| Unit | `WeeklyReportService` — verify grouping, variety score, trend fallback |
| Unit | `SentReminderRepository.ExistsAsync` — verify date boundary logic |
| Integration | Hangfire registers and fires jobs without exception |
| Integration | `CreateNotificationCommand` still works end-to-end with real DB |

## Risks

| Risk | Mitigation |
|------|-----------|
| Large user count makes `WeeklyReportService` slow | Add batching (100 users/batch) in a follow-up |
| `/profile/stats` page missing | Create separate design doc per user request |
| Timezone mismatch for "Monday 08:00" | Using UTC to match existing jobs; consider user local TZ in future |
| Deleted users with orphaned CalendarEvents | `CreateNotificationCommandHandler` already throws `NotFoundException`; catch and log |

## Files Changed Summary

| File | Change |
|------|--------|
| `src/OutfitPlanner.Domain/Entities/SentReminder.cs` | **Create** entity |
| `src/OutfitPlanner.Application/Contracts/Persistence/ISentReminderRepository.cs` | **Create** interface |
| `src/OutfitPlanner.Application/Contracts/Infrastructure/IClock.cs` | **Create** interface |
| `src/OutfitPlanner.Persistence/Repositories/SentReminderRepository.cs` | **Create** implementation |
| `src/OutfitPlanner.Application/Contracts/Persistence/IUnitOfWork.cs` | + `SentReminders` property |
| `src/OutfitPlanner.Persistence/Repositories/UnitOfWork.cs` | + `SentReminders` ctor + property |
| `src/OutfitPlanner.Persistence/DependencyInjection.cs` | + `ISentReminderRepository` registration |
| `src/OutfitPlanner.Infrastructure/DependencyInjection.cs` | + `IClock` registration |
| `src/OutfitPlanner.Application/Contracts/Persistence/ICalendarEventRepository.cs` | + `GetByUserIdAndDateRangeAsync` |
| `src/OutfitPlanner.Persistence/Repositories/CalendarEventRepository.cs` | + `GetByUserIdAndDateRangeAsync` impl |
| `src/OutfitPlanner.Infrastructure/Services/CalendarReminderService.cs` | **Create** |
| `src/OutfitPlanner.Infrastructure/Services/WeeklyReportService.cs` | **Create** |
| `src/OutfitPlanner.Infrastructure/Services/SystemClock.cs` | **Create** |
| `src/OutfitPlanner.Api/Program.cs` | + service registrations + 2 Hangfire jobs |

## Order of Implementation

1. `IClock` contract + `SystemClock` impl + DI registration
2. `SentReminder` entity + repository + DI + UnitOfWork + migration
3. `ICalendarEventRepository` extension + impl
4. `CalendarReminderService` (depends on 1, 2, 3)
5. `WeeklyReportService` (depends on 1)
6. Register services + Hangfire jobs in `Program.cs`
7. Build + verify `RecordWearCommandHandler` milestone still compiles
