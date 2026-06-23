# Notification Implementation Plan

> Goal: Close all 10 notification gaps from `notification-gap-analysis.md` and add real-time SignalR delivery.

---

## Architecture

```
User Action (e.g. Follow, Comment, Like)
    → Command Handler
        → CreateNotificationCommand
            → NotificationCreated domain event
                → SignalR NotificationHub pushes to recipient
                → Persist to DB (Notifications table)
                    → Frontend receives via hub OR polls REST
```

---

## Phase 1 — Backend Notification Triggers

### 1.1 Social Notifications (P0)

#### Follow
- **File**: `src/OutfitPlanner.Application/Features/User/Handlers/Commands/FollowUserCommandHandler.cs`
- **Trigger**: After `Follow` entity saved
- **Notify**: `followedUserId`
- **Payload**:
  - Title: "New Follower"
  - Message: "{FollowerName} started following you."
  - ActionUrl: "/Social/MyFollowers/{followedUserId}"
  - Type: Social

#### Like / Reaction
- **File**: `src/OutfitPlanner.Application/Features/Feed/Handlers/Commands/AddPostReactionCommandHandler.cs`
- **Trigger**: After `PostReaction` saved
- **Notify**: `post.UserId` (skip if self-like)
- **Payload**:
  - Title: "New like on your outfit"
  - Message: "{ReactorName} liked your \"{PostCaption}\"."
  - ActionUrl: "/Social/outfits/{postId} or /Social/polls/{pollId} depending on post type"
  - Type: Social

#### Comment
- **File**: `src/OutfitPlanner.Application/Features/Feed/Handlers/Commands/AddPostCommentCommandHandler.cs`
- **Status**: Handler **missing** — must be created
- **Trigger**: After `PostComment` saved
- **Notify**: `post.UserId` (skip if self-comment)
- **Payload**:
  - Title: "Comment on \"{PostCaption}\""
  - Message: "{CommenterName} commented: \"{Content}\""
  - ActionUrl: "/Social/outfits/{postId} or /Social/polls/{pollId} depending on post type"
  - Type: Social

### 1.2 Reminder Notifications (P1)

#### Calendar Event Reminders
- **New file**: `src/OutfitPlanner.Application/Features/Notifications/Services/CalendarReminderService.cs`
- **Trigger**: Background job (Hangfire/Cron) runs every hour
- **Logic**:
  - Query `CalendarEvents` where `EventDate` is today or tomorrow
  - For "today" events → notify: "You scheduled \"{Title}\" for today. Did you wear it?"
  - For "tomorrow" events → notify: "You have \"{Title}\" scheduled for tomorrow at {StartTime}"
- **Type**: Reminder

### 1.3 System Notifications (P1-P2)

#### Wear Milestone
- **File**: `src/OutfitPlanner.Application/Features/ClothingItems/Handlers/Commands/RecordWearCommandHandler.cs`
- **Trigger**: After `WearEvent` saved
- **Logic**:
  - Count wears for `ClothingItemId` in current month
  - If count ∈ {10, 25, 50, 100} → send notification
  - Title: "Wear Count Update"
  - Message: "You've worn your \"{ItemName}\" {count} times this month!"
  - ActionUrl: "/wardrobe/{itemId}"
  - Type: System

#### Weekly Style Report
- **New file**: `src/OutfitPlanner.Application/Features/Notifications/Services/WeeklyReportService.cs`
- **Trigger**: Background job — every Monday 08:00
- **Logic**:
  - Aggregate last week wears per user
  - Most worn item, outfit variety score, comfort avg
  - Title: "Weekly Style Report Ready"
  - Message: "You wore {item} {n} times last week. Your style: {trend}."
  - ActionUrl: "/profile/stats"
  - Type: System

#### Login / Device Detection
- **New entity**: `LoginHistory` (device, browser, ip, location, timestamp)
- **New repo**: `ILoginHistoryRepository`
- **Trigger**: In `AuthController.Login` (or `LoginHandler`) after successful auth
- **Logic**:
  - Compare current device fingerprint with last 3 logins
  - If new device → notify
  - Title: "Account Security"
  - Message: "New login detected from {Browser} on {OS}."
  - ActionUrl: "/settings/security"
  - Type: System

### 1.4 Weather Notifications (P2)

- **New file**: `src/OutfitPlanner.Application/Features/Notifications/Services/WeatherAlertService.cs`
- **Trigger**: Background job — every 6 hours
- **Logic**:
  - For each user with location saved → fetch forecast
  - If rain probability > 60% → notify "Rain Forecast Alert"
  - If temp < 10°C → notify "Cold Weather Alert"
  - Type: Weather

---

## Phase 2 — SignalR Real-Time Delivery

### 2.1 Infrastructure

#### Notification Hub
- **New file**: `src/OutfitPlanner.Infrastructure/Services/NotificationHub.cs`
- **Lib**: `Microsoft.AspNetCore.SignalR`
- **Methods**:
  - `Subscribe(string userId)` — add connection to user group
  - `Unsubscribe(string userId)` — remove on disconnect
  - `PushNotification(string userId, NotificationDto notification)` — server calls this from handlers
- **Endpoint**: `/notifications/hub`

#### Integration in CreateNotificationCommandHandler
- After saving notification to DB, call `_notificationHub.PushNotification(userId, dto)`
- Requires injecting `IHubContext<NotificationHub>` or a wrapper interface

### 2.2 Frontend Client

#### Notification Hub Service
- **New file**: `src/outfit-planner-ui/src/app/core/services/notification-hub.service.ts`
- **Lib**: `@microsoft/signalr`
- **Methods**:
  - `connect(userId)` — starts connection with access token
  - `onReceive(callback)` — listens for `ReceiveNotification`
  - `disconnect()`
- **NgRx**: Dispatch `AiActions.notificationReceived` (or new action in Notifications feature)

#### UI Updates
- Navbar badge: subscribe to `notificationsCount$` selector, show unread count
- Toast: transient popup when new SignalR notification arrives
- Ping sound (optional)

---

## Phase 3 — Supporting Changes

### 3.1 Entities Needed

#### LoginHistory
```csharp
public class LoginHistory : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;      // e.g. "Chrome"
    public string OS { get; set; } = string.Empty;           // e.g. "MacOS"
    public string IPAddress { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;    // e.g. "Cairo, EG"
    public DateTimeOffset LoggedInAt { get; set; }
}
```

### 3.2 Repositories Needed

- `ILoginHistoryRepository : IGenericRepository<LoginHistory>`
  - `Task<IEnumerable<LoginHistory>> GetRecentAsync(string userId, int count = 3)`

### 3.3 New Commands/Queries

- `CreateNotificationCommand` — already exists per gap analysis
- `GetNotificationsQuery` — exists
- `MarkNotificationReadCommand` — exists

### 3.4 DTOs / Extensions

- `NotificationDto` — map from entity for SignalR (Id, Title, Message, Type, ActionUrl, CreatedAt, IsRead)

---

## Implementation Order

| Step | Task | Files | Priority |
|------|------|-------|----------|
| 1 | Add notification triggers to Follow handler | `FollowUserCommandHandler.cs` | P0 |
| 2 | Add notification triggers to Reaction handler | `AddPostReactionCommandHandler.cs` | P0 |
| 3 | **Create** `AddPostCommentCommandHandler` with notification | new file | P0 |
| 4 | Add wear milestone check in `RecordWearCommandHandler` | existing handler | P1 |
| 5 | Create `LoginHistory` entity + repo + migration | domain + persistence | P1 |
| 6 | Add login detection in `AuthController` | controller | P1 |
| 7 | Create `CalendarReminderService` + register in DI | new service | P1 |
| 8 | Create `WeatherAlertService` + register in DI | new service | P2 |
| 9 | Create `WeeklyReportService` + register in DI | new service | P2 |
| 10 | Add `NotificationHub` SignalR hub | infrastructure | P1 |
| 11 | Wire `CreateNotificationCommandHandler` to call hub | handler | P1 |
| 12 | Frontend `NotificationHubService` (Angular) | UI service | P1 |
| 13 | Frontend navbar badge + toast UI | components | P1 |
| 14 | Register Hangfire/cron jobs for reminders, weather, weekly | Program.cs | P2 |

---

## Risks / Decisions

- **SignalR scale**: If multi-instance, need Redis backplane. acceptable for single-instance dev.
- **Login tracking**: Requires new DB table. Acceptable schema change.
- **Background jobs**: Hangfire recommended if already referenced; else use `IHostedService` + cron expressions.
- **Frontend real-time**: SignalR requires WebSocket support in hosting environment (IIS/nginx config).

---

## Files Changed Summary

| File | Change |
|------|--------|
| `FollowUserCommandHandler.cs` | + `CreateNotificationCommand` dispatch |
| `AddPostReactionCommandHandler.cs` | + notification dispatch |
| `AddPostCommentCommandHandler.cs` | **create** with notification |
| `RecordWearCommandHandler.cs` | + milestone check + notification |
| `AuthController.cs` | + `LoginHistory` save + new device detection |
| `CreateNotificationCommandHandler.cs` | + SignalR hub call |
| `NotificationHub.cs` | **create** SignalR hub |
| `LoginHistory.cs` | **create** entity |
| `ILoginHistoryRepository.cs` | **create** repo |
| `LoginHistoryRepository.cs` | **create** impl |
| `CalendarReminderService.cs` | **create** |
| `WeatherAlertService.cs` | **create** |
| `WeeklyReportService.cs` | **create** |
| `notification-hub.service.ts` | **create** frontend hub client |
| Navbar component | + badge + toast |
| Notifications NgRx | + real-time action + reducer case |