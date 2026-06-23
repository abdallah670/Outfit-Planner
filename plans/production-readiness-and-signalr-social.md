# Production Readiness & SignalR Social Community Plan

This plan covers two major areas:
1. **SignalR real-time hub for Social Community** (new feed posts, live comment/reaction counts)
2. **Production-readiness fixes** across security, deployment, monitoring, and testing

---

## Part 1: SignalR Social Hub

### Backend Changes

#### [CREATE] `SocialHub.cs` — `OutfitPlanner.Infrastructure/Services/SocialHub.cs`
- New SignalR hub at route `/social/hub`
- Groups by follower relationships (user joins group `user_{userId}`)
- Methods:
  - `JoinFeed()` — client calls on init, adds to their personal feed group
  - `NewPost(string postId, string json)` — server pushes to followers' groups
  - `CommentUpdate(string postId, int newCount)` — server pushes live comment count
  - `ReactionUpdate(string postId, int newCount)` — server pushes live reaction count

#### [CREATE] `SocialHubService.cs` / `ISocialHubService.cs`
- Abstraction interface in Application.Contracts.Infrastructure
- Implementation wrapping `IHubContext<SocialHub>`
- Methods:
  - `NotifyNewPostAsync(postOwnerId, postData)` — push to followers
  - `NotifyCommentUpdateAsync(postId, commentCount)`
  - `NotifyReactionUpdateAsync(postId, reactionCount)`

#### [MODIFY] `FeedController.cs`
- Inject `ISocialHubService`
- After `CreatePost`, call `_socialHub.NotifyNewPostAsync(userId, postDto)`
- After `AddComment`, call `_socialHub.NotifyCommentUpdateAsync(postId, newCount)`
- After `AddReaction` / `RemoveReaction`, call `_socialHub.NotifyReactionUpdateAsync(postId, newCount)`

#### [MODIFY] `AddPostCommentCommandHandler.cs`
- After successful save, return updated comment count so controller can push it

#### [MODIFY] `AddPostReactionCommandHandler.cs`
- After successful save, return updated reaction count so controller can push it

#### [MODIFY] `DependencyInjection.cs`
- Register `ISocialHubService` / `SocialHubService`

#### [MODIFY] `Program.cs`
- Add `app.MapHub<SocialHub>("/social/hub")`

### Frontend Changes

#### [CREATE] `social-hub.service.ts`
- Angular service connecting to `/social/hub` with JWT auth
- Methods: `connect(token)`, `disconnect()`, `joinFeed()`
- Events listened: `NewPost`, `CommentUpdate`, `ReactionUpdate`
- Dispatches NgRx actions on each event

#### [MODIFY] Feed NgRx state
- Add effect that listens for real-time `newPost` event → prepend to feed
- Add effect for `commentUpdate` / `reactionUpdate` → update post in state

#### [MODIFY] UI Components
- Feed component: subscribe to real-time posts via store selectors
- Post detail / comments: live counter updates without page refresh

### Verification
- Open two browser tabs logged in as different users
- User A creates a post → User B sees it appear in feed without refresh
- User A comments → live comment count update on User B's view
- User A reacts → live reaction count update

---

## Part 2: Production Readiness

### 🔴 Critical (Phase 1 — security)

| # | Issue | Location | Fix |
|---|-------|----------|-----|
| 1 | **`environment.prod.ts` missing** | `src/outfit-planner-ui/environments/` | Create with production API URL pointing to MonsterASP backend |
| 2 | **CORS allows any origin** (`_ => true`) | `Program.cs` | Lock to Netlify frontend domain + MonsterASP API domain |
| 3 | **Connection string** uses Windows Auth | `appsettings.json` | Switch to SQL Server with user/password auth (as required by MonsterASP) |

#### 1. Create `environment.prod.ts`
- **File**: `src/outfit-planner-ui/src/environments/environment.prod.ts`
- **Content**: `apiUrl: "https://your-api.monsterasp.com"`, `production: true`, no API keys
- **Validation**: `ng build --configuration production` succeeds

#### 2. Lock CORS to production domain
- **File**: `src/OutfitPlanner.Api/Program.cs`
- **Change**: Replace `SetIsOriginAllowed(_ => true)` with explicit origins: `https://your-frontend.netlify.app`, `https://your-api.monsterasp.com`
- **Validation**: CORS returns 403 for unauthorized origins in integration test

#### 3. Connection string for MonsterASP SQL Server
- **File**: `appsettings.json`
- **Change**: Use SQL Server auth (Server, Database, User ID, Password) — set via env variables in MonsterASP dashboard
- **Validation**: `dotnet run` reads from env variable `ConnectionStrings__DefaultConnection`

#### 4. Update vulnerable NuGet packages
- AutoMapper 12.0.1 → 13.0.1+
- SixLabors.ImageSharp 3.1.6 → 3.1.7+
- Newtonsoft.Json 11.0.1 → 13.0.3+
- **Validation**: `dotnet list package --vulnerable` returns empty

### 🟡 High Priority (Phase 2 — deployment)

| # | Issue | Location | Fix |
|---|-------|----------|-----|
| 7 | **No MonsterASP deployment config** | root | Add `web.config` for IIS, ensure .NET 10 runtime, set env variables in dashboard |
| 8 | **No CI/CD pipeline** | root | Add GitHub Actions workflow (build, test, deploy to MonsterASP via FTP/WebDeploy) |
| 9 | **Rate limiter defined but NOT applied** to endpoints | `Program.cs` | Add `[EnableRateLimiting("Api")]` / `("Auth")` to controllers |
| 10 | **No Netlify deployment config** | root | Add `netlify.toml` for SPA routing + build settings |

#### 7. MonsterASP backend deployment setup
- Create `web.config` in `src/OutfitPlanner.Api/` with:
  - ASP.NET Core hosting module config
  - Rewrite rules for HTTPS
  - Environment variable passthrough
- **MonsterASP dashboard setup**:
  - Set ASP.NET Core version to .NET 10
  - Add env variables: `JWT_KEY`, `ConnectionStrings__DefaultConnection`, `Authentication:Google:ClientId`, `Authentication:Google:ClientSecret`, etc.
  - Enable WebSocket support for SignalR
  - Set application path to `/api`
- **Validation**: API responds at `https://your-api.monsterasp.com/swagger`

#### 8. CI/CD pipeline
- Create `.github/workflows/deploy.yml`
  - **Backend**: On push to main → `dotnet build` → `dotnet publish` → FTP/WebDeploy to MonsterASP
  - **Frontend**: On push to main → `npm run build -- --configuration production` → deploy to Netlify
- Create `.github/workflows/ci.yml`
  - Trigger: PR to main
  - Steps: restore, build, run `dotnet test`, lint
- **Validation**: PR creates automated build check, push to main deploys to both platforms

#### 9. Apply rate limiting to controllers
- **File**: All controller classes
- **Change**: Add `[EnableRateLimiting("Api")]` / `[EnableRateLimiting("Auth")]` / `[EnableRateLimiting("Feed")]`
- **Validation**: Sending 100 requests/sec → 429 Too Many Requests

#### 10. Netlify frontend deployment config
- Create `netlify.toml` in project root with:
  ```toml
  [build]
    base = "src/outfit-planner-ui"
    publish = "dist/outfit-planner-ui/browser"
    command = "npm run build -- --configuration production"

  [[redirects]]
    from = "/*"
    to = "/index.html"
    status = 200
  ```
- Add `_redirects` file in `src/outfit-planner-ui/src/` for redundancy
- **Validation**: After deploy, navigating to any route directly (e.g. `/profile`) works without 404

### 🟡 Medium Priority (Phase 3 — data integrity & monitoring)

| # | Issue | Location | Fix |
|---|-------|----------|-----|
| 11 | **Soft delete** — data is hard-deleted | `AppDbContext.cs` | Add `IsDeleted` property + global query filter |
| 12 | **Audit logging** — no audit trail on entity changes | Persistence layer | Add `CreatedBy`, `UpdatedBy`, `UpdatedAt` tracking |
| 13 | **Health checks are minimal** — just `/health` | `Program.cs` | Add DB health check + external API health checks |
| 14 | **Hangfire dashboard auth** — verify `HangfireAuthorizationFilter` | `Program.cs` | Ensure admin-only access in production |
| 15 | **No PWA/service worker** despite config being referenced | Frontend | Complete `ngsw-config.json`, register SW in `app.module` |
| 16 | **Lazy loading** — check all routes for lazy modules | `app-routing.module.ts` | Ensure all feature routes use `loadChildren` |
| 17 | **Test coverage is thin** (only 7 test files, no social/feed tests) | `tests/` | Add unit tests for social features, notification hub |
| 18 | **No frontend tests** at all | `src/outfit-planner-ui` | Add at least smoke tests for main components |



#### 11. Enhanced health checks
- Add `AddDbContextCheck<AppDbContext>()` for DB connectivity
- Add health check for external APIs (Weather, LLM)
- Expose at `/health/ready` (liveness) and `/health/startup`
- **Validation**: `curl /health` returns JSON with all component statuses

#### 12. Hangfire dashboard authorization
- **File**: `src/OutfitPlanner.Api/Middleware/HangfireAuthorizationFilter.cs`
- **Change**: Ensure it restricts to Admin role only
- **Validation**: Non-admin user gets 403 at `/hangfire`

#### 13. PWA / Service Worker
- Complete `ngsw-config.json` (cache strategy for API + static assets)
- Register service worker in `app.module.ts`
- Add web manifest (`manifest.json`) link in `index.html`
- **Validation**: Lighthouse PWA audit passes

#### 14. Lazy loading audit
- **File**: `app-routing.module.ts`
- **Change**: Ensure all feature routes use `loadChildren: () => import(...)`
- Check: AI assistant, Social feed, Admin panels
- **Validation**: `ng build --configuration production` separates chunks per feature

#### 14. Social feature unit tests
- **New files**:
  - `tests/OutfitPlanner.Application.UnitTests/Social/AddPostCommentCommandHandlerTests.cs`
  - `tests/OutfitPlanner.Application.UnitTests/Social/AddPostReactionCommandHandlerTests.cs`
- **Validation**: `dotnet test` passes with >80% coverage on new files

#### 18. Frontend smoke tests
- Add Jest/Karma test for: navbar rendering, feed list display, login form
- **Validation**: `ng test` passes with no failures

---

## Implementation Order

| Phase | Priority | Items | Estimated Effort |
|-------|----------|-------|------------------|
| Phase 1 | 🔴 Critical | 1–6 (security + packages) | 2–3 days |
| Phase 2 | 🟡 High | 7–10 (deployment MonsterASP + Netlify + CI/CD) | 2–3 days |
| Phase 3 | 🟡 Medium | 11–16 (data integrity + monitoring) | 4–5 days |
| Phase 4 | 🟢 Nice-to-have | 17–18 (tests) | 2–3 days |