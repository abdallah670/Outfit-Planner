# Profile Stats Page — Real API Integration Plan

> Scope: Replace stub data in `/profile/stats` with a real backend API that computes weekly style stats on-the-fly.

## Decision

| Decision | Choice |
|----------|--------|
| Endpoint location | `GET /api/user/weekly-style-stats` in `UserController` |
| Auth | Required — `[Authorize]`, user ID from `CustomClaimTypes.Uid` |
| Past reports | Computed on-the-fly (last 3 weeks) — no new DB table or migration needed |
| Frontend data loading | Component-level `HttpClient` call with loading/error signals; no NgRx feature needed |
| Timezone | UTC throughout to match existing server-side jobs |

## Backend

### New DTO
- File: `src/OutfitPlanner.Application/DTOs/User/WeeklyStyleStatsDto.cs`
- Properties:
  - `List<WeeklyReportDto> WeeklyReports` (current week first)
  - `WeeklyReportDto` contains: `WeekStart`, `WeekEnd`, `IsCurrentWeek`, `MostWornItemName`, `MostWornCount`, `VarietyScore` (0–1), `ComfortAverage` (decimal), `TotalWears`, `Trend` (string: profile style name or `"Versatile"`/`"Focused"`/`"Mixed"`)

### New Query
- File: `src/OutfitPlanner.Application/Features/User/Requests/Queries/GetWeeklyStyleStatsQuery.cs`
- `IRequest<WeeklyStyleStatsDto>`

### New Handler
- File: `src/OutfitPlanner.Application/Features/User/Handlers/Queries/GetWeeklyStyleStatsQueryHandler.cs`
- Dependencies: `UserManager<User>`, `IWearEventRepository`, `IUserStyleProfileRepository`, `IClock`
- Logic:
  1. Resolve `userId` from claims via `UserManager`
  2. For each of the last 4 week boundaries (current + 3 past):
     - `weekEnd = today 00:00 UTC - (weekOffset * 7 days)`
     - `weekStart = weekEnd - 7 days`
     - Query `WearEvents` where `UserId == userId` and `WornAt >= weekStart && WornAt < weekEnd`
     - Count `totalWears`, distinct `ClothingItemId` values → `uniqueItems`
     - `varietyScore = uniqueItems / totalWears` (guard divide-by-zero → 0)
     - `comfortAverage = average Rating` (guard empty → 0)
     - Group by `ClothingItemId`, order by count desc, take top 1 → fetch `ClothingItem.Name` for `mostWornItemName`
     - `mostWornCount = top group count`
     - `trend`: if `UserStyleProfile` exists → `Style.ToString()`; else if `varietyScore > 0.7` → `"Versatile"`, `< 0.3` → `"Focused"`, else `"Mixed"`
     - Append to list with `WeekStart = weekStart`, `WeekEnd = weekEnd.AddDays(-1)`, `IsCurrentWeek = weekOffset == 0`
  3. Sort list descending by `WeekStart`
  4. Return wrapped in `WeeklyStyleStatsDto`
- Edge cases:
  - No wear events for a week → report with zeros, empty item name
  - User has no `UserStyleProfile` → fallback trend
  - All ratings 0 → average 0

### Controller Update
- File: `src/OutfitPlanner.Api/Controllers/UserController.cs`
- Add:
  ```csharp
  [HttpGet("weekly-style-stats")]
  public async Task<ActionResult<WeeklyStyleStatsDto>> GetWeeklyStyleStats()
  ```

## Frontend

### Data Layer
- File: `src/outfit-planner-ui/src/app/data/datasources/user.datasource.ts`
- Add: `getWeeklyStyleStats(): Observable<WeeklyStyleStatsResponse>`

### Domain Entity
- File: `src/outfit-planner-ui/src/app/presentation/pages/profile-stats/profile-stats.component.ts`
- Add interfaces matching backend response shape
- Replace hardcoded `currentReport` and `pastReports` signals with:
  - `loading` signal
  - `error` signal
  - `stats` signal of typed response
- In `ngOnInit`:
  - Set `loading = true`, `error = null`
  - Call `userDataSource.getWeeklyStyleStats()`
  - Map to local signals
  - Set `loading = false`
- Keep helper methods (`getTrendBackground`, `getTrendTextColor`)
- Derive `dateRange` from first report's `WeekStart`/`WeekEnd`
- Split reports into `currentReport` = first item where `isCurrentWeek`, `pastReports` = remainder

### Template
- File: `src/outfit-planner-ui/src/app/presentation/pages/profile-stats/profile-stats.component.html`
- Add loading spinner when `loading()`
- Add error message when `error()` with retry button
- Bind real data from signals instead of stubs
- Hide `mostWornItem` section when `name` is empty

### Styles
- No changes needed; current `profile-stats.component.scss` already matches `Design/stats.html`

## Validation

| Step | Check |
|------|-------|
| Build backend | `dotnet build` succeeds |
| Build frontend | `npx ng build` succeeds |
| Manual API | `GET /api/user/weekly-style-stats` returns 200 with 4 reports |
| Manual page | `/profile/stats` loads without stub data, shows real computed stats |
| Edge case | User with 0 wears shows zeros and empty trend instead of crash |

## Files Touched

| File | Action |
|------|--------|
| `src/OutfitPlanner.Application/DTOs/User/WeeklyStyleStatsDto.cs` | Create |
| `src/OutfitPlanner.Application/Features/User/Requests/Queries/GetWeeklyStyleStatsQuery.cs` | Create |
| `src/OutfitPlanner.Application/Features/User/Handlers/Queries/GetWeeklyStyleStatsQueryHandler.cs` | Create |
| `src/OutfitPlanner.Api/Controllers/UserController.cs` | Modify |
| `src/outfit-planner-ui/src/app/data/datasources/user.datasource.ts` | Modify |
| `src/outfit-planner-ui/src/app/presentation/pages/profile-stats/profile-stats.component.ts` | Modify |
| `src/outfit-planner-ui/src/app/presentation/pages/profile-stats/profile-stats.component.html` | Modify |
