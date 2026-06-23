# Notification Gap Analysis

This document maps the seeder notification examples against actual application behavior to identify missing notification triggers.

---

## Seeder Notification Examples (10 Total)

### 1. Social Notifications

| # | Title | Message | Trigger Event | Implemented? |
|---|-------|---------|---------------|--------------|
| 1 | "New like on your outfit" | "Emma W. liked your \"Office Chic\" outfit combination." | User likes an outfit post | ❌ **NO** |
| 2 | "Comment on \"Summer Dress\"" | "Sophie commented: \"Love this color on you!...\"" | User comments on an outfit | ❌ **NO** |
| 3 | "New Follower" | "Michael T. started following you." | User follows another user | ❌ **NO** |

**Gap**: 
- `@src/OutfitPlanner.Application/Features/User/Handlers/Commands/FollowUserCommandHandler.cs:18-48` creates follow records but never sends `CreateNotificationCommand`
- `@src/OutfitPlanner.Application/Features/Feed/Handlers/Commands/AddPostReactionCommandHandler.cs` adds reactions but no notification to post owner
- `AddPostCommentCommandHandler` is **missing entirely** - command exists but no handler implements it

---

### 2. Reminder Notifications

| # | Title | Message | Trigger Event | Implemented? |
|---|-------|---------|---------------|--------------|
| 4 | "Log your outfit for today" | "You scheduled \"Weekend Casual\" for today. Did you wear it?" | Calendar event reminder | ❌ **NO** |
| 5 | "Outfit Reminder" | "You have \"Business Meeting\" scheduled for tomorrow at 2:00 PM" | Upcoming calendar event | ❌ **NO** |

**Gap**: No background job or scheduler exists to trigger reminder notifications. `@src/OutfitPlanner.Application/Features/Calendar/Handlers/Commands/ScheduleOutfitCommandHandler.cs` only creates the event, no reminder logic.

---

### 3. System Notifications

| # | Title | Message | Trigger Event | Implemented? |
|---|-------|---------|---------------|--------------|
| 6 | "Weekly Style Report Ready" | "Your style stats for last week are now available..." | Weekly report generation | ❌ **NO** |
| 7 | "Account Security" | "New login detected from Chrome on MacOS." | New device/login detected | ❌ **NO** |
| 8 | "Wear Count Update" | "You've worn your \"Blue Denim Jacket\" 10 times this month!" | Milestone wear count reached | ❌ **NO** |

**Gap**: 
- No weekly report job exists
- No login detection/tracking exists in auth flow
- `@src/OutfitPlanner.Application/Features/ClothingItems/Handlers/Commands/RecordWearCommandHandler.cs` records wear events but doesn't check for milestones

---

### 4. Weather Notifications

| # | Title | Message | Trigger Event | Implemented? |
|---|-------|---------|---------------|--------------|
| 9 | "Rain Forecast Alert" | "Rain is expected tomorrow. Don't forget your raincoat..." | Weather forecast check | ❌ **NO** |
| 10 | "Cold Weather Alert" | "Temperature dropping to 5°C tomorrow..." | Temperature threshold check | ❌ **NO** |

**Gap**: Weather service exists (`@src/OutfitPlanner.Infrastructure/Services/WeatherService.cs`) but no background job checks forecasts and sends alerts.

---

## Summary

| Category | Seeder Examples | Implemented | Gap |
|----------|-----------------|-------------|-----|
| **Social** | 3 (like, comment, follow) | 0 | 3 missing |
| **Reminder** | 2 (calendar events) | 0 | 2 missing |
| **System** | 3 (reports, security, stats) | 0 | 3 missing |
| **Weather** | 2 (alerts) | 0 | 2 missing |
| **TOTAL** | **10** | **0** | **10 missing** |

---

## Infrastructure Status

**Exists**:
- `CreateNotificationCommand` handler (`@src/OutfitPlanner.Application/Features/Notifications/Handlers/Commands/CreateNotificationCommandHandler.cs`)
- API endpoints for CRUD (`@src/OutfitPlanner.Api/Controllers/NotificationsController.cs`)
- Front-end notification service (`@src/outfit-planner-ui/src/app/core/services/notification.service.ts`)
- Notification settings management

**Missing**:
- Integration points in action handlers to call `CreateNotificationCommand`
- Background jobs for scheduled/periodic notifications (reminders, weather, reports)
- Event-driven architecture (domain events → notification handlers)
- Real-time delivery (WebSockets/push notifications)

---

## Missing Features Required for Notifications

To implement the seeder notification examples, these **underlying features don't exist** and must be created:

### 1. Login History / Device Tracking
**Required for**: "New login detected from Chrome on MacOS"

**Missing**:
- Entity to store login history (device, browser, IP, location, timestamp)
- Repository to track known devices per user
- Logic in `@src/OutfitPlanner.Api/Controllers/AuthController.cs:28-43` to detect new/unknown devices
- Comparison logic between current login and previous known devices

### 2. Wear Count Tracking
**Required for**: "You've worn your 'Blue Denim Jacket' 10 times this month!"

**Missing**:
- Aggregation query to count wears per clothing item per month
- Milestone detection (10, 20, 50 wears)
- Trigger in `@src/OutfitPlanner.Application/Features/ClothingItems/Handlers/Commands/RecordWearCommandHandler.cs` to check milestones after each wear

### 3. Weather Forecast Background Job
**Required for**: "Rain Forecast Alert", "Cold Weather Alert"

**Missing**:
- Scheduled job to check weather forecasts for user locations
- User location storage (not just weather preferences)
- Integration with existing `@src/OutfitPlanner.Infrastructure/Services/OpenWeatherMapWeatherService.cs`
- Threshold comparison logic (temperature < X, rain probability > Y)

### 4. Weekly Style Report Generation
**Required for**: "Weekly Style Report Ready"

**Missing**:
- Weekly aggregation job (Hangfire or similar)
- Statistics calculation (most worn items, outfit variety, etc.)
- Report generation service

### 5. Calendar Reminder System
**Required for**: "Log your outfit for today", "Outfit Reminder"

**Missing**:
- Background job to check calendar events at scheduled times
- Reminder trigger logic (X hours before event, on event day)
- Wear confirmation tracking (did user wear the scheduled outfit?)

### 6. Comment System (Partially Exists)
**Required for**: "Comment on 'Summer Dress'"

**Exists**:
- `PostComment` entity (`@src/OutfitPlanner.Domain/Entities/PostComment.cs`)
- `IPostCommentRepository` and implementation (`@src/OutfitPlanner.Persistence/Repositories/PostCommentRepository.cs`)
- Query handlers (`GetPostCommentsQueryHandler`, `GetRecentPollWithCommentsQueryHandler`)
- API endpoint to get comments (`@src/OutfitPlanner.Api/Controllers/FeedController.cs:134-149`)
- `AddPostCommentCommand` exists (`@src/OutfitPlanner.Application/Features/Feed/Requests/Commands/AddPostCommentCommand.cs`)

**Missing**:
- `AddPostCommentCommandHandler` - the command exists but **no handler implements it**
- Notification trigger when comment is added

---

## Key Files

**Seeder**: `@src/OutfitPlanner.Persistence/Data/DataSeeder.cs:461-578`

**Notification Handler**: `@src/OutfitPlanner.Application/Features/Notifications/Handlers/Commands/CreateNotificationCommandHandler.cs`

**Auth Controller** (login detection needed): `@src/OutfitPlanner.Api/Controllers/AuthController.cs:28-43`

**Action Handlers Missing Notification Calls**:
- `@src/OutfitPlanner.Application/Features/User/Handlers/Commands/FollowUserCommandHandler.cs`
- `@src/OutfitPlanner.Application/Features/Feed/Handlers/Commands/AddPostReactionCommandHandler.cs`
- `@src/OutfitPlanner.Application/Features/Feed/Handlers/Commands/VoteOnPollCommandHandler.cs`
- `@src/OutfitPlanner.Application/Features/Calendar/Handlers/Commands/ScheduleOutfitCommandHandler.cs`
- `@src/OutfitPlanner.Application/Features/ClothingItems/Handlers/Commands/RecordWearCommandHandler.cs`
