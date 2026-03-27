# Fix Calendar Event Edit Time Issue

## Current Status

- [x] Analyzed files and identified root cause
- [x] User confirmed plan
- [x] Frontend: Updated formatTimeForApi to return object format
- [x] Frontend: Updated UpdateCalendarEventRequest type
- [x] Frontend: Fixed calendar.component.ts dispatch update action
- [x] Backend: Added TimeSpanConverter for JSON deserialization
- [x] Backend: Registered converters in Program.cs

## Steps to Complete

### 1. Frontend NgRx Actions/Effects

- [x] Check/create `updateCalendarEvent` action in `src/outfit-planner-ui/src/app/core/state/calendar/calendar.actions.ts` ✅ Exists
- [x] Implement effect in `calendar.effects.ts` to call backend PUT /api/calendar/events/{id} ✅ Exists, calls wearEventUseCases.updateCalendarEvent
- [x] Add selector for updated events if needed ✅ Not needed

### 2. Frontend Component Fixes

- [x] **Step 1**: Update `calendar.component.ts` - dispatch updateCalendarEvent on edit modal success (fix TODO)
- [x] **Step 2**: Add time validation in `edit-event-modal.component.ts/html`

### 3. Backend Verification

- [x] Read `CalendarController.cs` - confirm PUT endpoint for update
- [x] Check/create `UpdateCalendarEventCommand` + Handler
- [x] Verify TimeSpan parsing for startTime/endTime - Added TimeSpanConverter

### 4. Testing

- [ ] Test full edit flow: EventDetailsModal → EditEventModal → backend update → time preserved
- [ ] Verify in calendar view
