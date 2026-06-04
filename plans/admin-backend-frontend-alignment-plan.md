# Outfit-Planner: Admin Panel — Backend-Frontend Alignment Plan

> **Date:** 2026-05-23  
> **Scope:** Fix all mismatches between the admin backend API and frontend UI  
> **Status:** Backend has 30+ endpoints, Frontend has 35+ datasource calls, ~17 mismatches identified  
> **Dependencies:** Phase 1 (Auth/Roles) must be complete

---

## Table of Contents

1. [Current State Overview](#1-current-state-overview)
2. [Critical Mismatches — 404 Errors](#2-critical-mismatches--404-errors)
3. [Route & Shape Mismatches](#3-route--shape-mismatches)
4. [Missing Backend Endpoints (Frontend Needs Them)](#4-missing-backend-endpoints-frontend-needs-them)
5. [Missing Frontend Consumers (Backend Has, Frontend Doesn't Call)](#5-missing-frontend-consumers-backend-has-frontend-doesnt-call)
6. [Entity/DTO Shape Mismatches](#6-entitydto-shape-mismatches)
7. [Auth & Route Configuration Issues](#7-auth--route-configuration-issues)
8. [Complete API Surface Comparison](#8-complete-api-surface-comparison)
9. [Implementation Phases](#9-implementation-phases)
10. [File Inventory — What Needs to Change](#10-file-inventory--what-needs-to-change)

---

## 1. Current State Overview

### Backend (AdminController.cs — 753 lines, 30+ endpoints)

| Category | Endpoints | Status |
|----------|-----------|--------|
| **Locked Accounts** | `GET /locked-accounts` | ✅ Exists |
| **Analytics** | `GET /analytics/dashboard`, `GET /analytics/realtime`, `GET /analytics/detailed` | ✅ Exists |
| **Role Management** | `GET /roles`, `GET /roles/users`, `GET /roles/management`, `POST /roles/assign`, `POST /roles/update`, `DELETE /roles/{userId}/{role}` | ✅ Exists |
| **User Management** | `GET /users`, `GET /users/{id}`, `GET /users/{id}/stats`, `POST /users/{id}/ban`, `POST /users/{id}/unban` | ✅ Exists |
| **Content — Posts** | `GET /content/posts`, `DELETE /content/posts/{id}`, `POST /content/posts/bulk` | ✅ Exists |
| **Content — Polls** | `GET /content/polls`, `POST /content/polls/{id}/close`, `DELETE /content/polls/{id}`, `POST /content/polls/bulk` | ✅ Exists |
| **Content — Outfits** | `GET /content/outfits`, `DELETE /content/outfits/{id}`, `POST /content/outfits/bulk` | ✅ Exists |
| **Reports** | `GET /reports`, `GET /reports/{id}`, `POST /reports/{id}/resolve` | ✅ Exists |
| **Settings** | `GET /settings`, `POST /settings/maintenance` | ✅ Exists |
| **System Operations** | `POST /system/backup`, `POST /system/restart`, `POST /system/cache/clear` | ✅ Exists |
| **User Activity** | `GET /activities`, `GET /activities/{userId}/login-history`, `GET /activities/statistics`, `GET /activities/active-users`, `GET /activities/{userId}/session-info`, `GET /activities/analytics`, `GET /activities/trends` | ✅ Exists |
| **Audit Logs** | `GET /audit-logs`, `GET /audit-logs/{id}`, `GET /audit-logs/statistics`, `GET /audit-logs/analytics`, `GET /audit-logs/trends` | ✅ Exists |

### Frontend (admin.datasource.ts — 247 lines, ~35 calls)

| Category | Endpoint Calls | Status |
|----------|---------------|--------|
| **User Management** | `GET /users`, `GET /users/{id}`, `POST /users/{id}/ban`, `POST /users/{id}/unban` | ✅ Matches |
| **Reports** | `GET /reports`, `GET /reports/{id}`, `POST /reports/{id}/resolve` | ✅ Matches |
| **Settings** | `GET /settings`, `PUT /settings/{key}` | ⚠️ PUT doesn't exist |
| **Analytics** | `GET /analytics/dashboard`, `GET /analytics/detailed`, `GET /analytics/realtime`, `POST /analytics/export` | ⚠️ Export missing |
| **Locked Accounts** | `GET /locked-accounts`, `POST /unlock-account/{userId}` | ⚠️ Unlock missing |
| **Posts** | `GET /content/posts`, `POST /content/posts/{id}/approve`, `POST /content/posts/{id}/reject`, `DELETE /content/posts/{id}`, `POST /content/posts/bulk` | ⚠️ Approve/reject missing |
| **Polls** | `GET /content/polls`, `POST /content/polls/{id}/close`, `POST /content/polls/{id}/feature`, `POST /content/polls/{id}/unfeature`, `DELETE /content/polls/{id}`, `POST /content/polls/bulk` | ⚠️ Feature/unfeature missing |
| **Outfits** | `GET /content/outfits`, `POST /content/outfits/{id}/feature`, `POST /content/outfits/{id}/unfeature`, `POST /content/outfits/{id}/approve`, `POST /content/outfits/{id}/reject`, `DELETE /content/outfits/{id}`, `POST /content/outfits/bulk` | ⚠️ Feature/unfeature/approve/reject missing |
| **System** | `GET /system/health`, `GET /system/logs`, `GET /system/performance`, `POST /system/maintenance`, `POST /system/backup`, `POST /system/restart/{name}`, `POST /system/clear-cache` | ⚠️ Health/logs/performance/clear-cache routing mismatch |
| **Audit Logs** | `GET /audit-logs` (via generic filter) | ✅ Matches |
| **Roles** | ❌ No frontend calls at all | 🟡 Unused backend endpoints |

---

## 2. Critical Mismatches — 404 Errors

These frontend calls will **return 404** because the backend endpoints don't exist or have different routes:

| # | Frontend Call | Backend Has | Fix Needed |
|---|--------------|-------------|------------|
| 1 | `PUT /{baseUrl}/settings/{key}` | ❌ No PUT endpoint — only `GET /settings` and `POST /settings/maintenance` | Add `PUT /settings/{key}` to backend OR remove from frontend |
| 2 | `POST /{baseUrl}/unlock-account/{userId}` | ❌ No unlock endpoint | Add `POST /unlock-account/{userId}` OR remove from frontend |
| 3 | `POST /{baseUrl}/content/posts/{postId}/approve` | ❌ No approve/reject — only `DELETE` | Add approve/reject endpoints or use bulk operations |
| 4 | `POST /{baseUrl}/content/posts/{postId}/reject` | ❌ Same as above | Same as above |
| 5 | `POST /{baseUrl}/content/polls/{pollId}/feature` | ❌ No feature/unfeature | Add endpoints or use poll's `IsFeatured` property |
| 6 | `POST /{baseUrl}/content/polls/{pollId}/unfeature` | ❌ Same as above | Same as above |
| 7 | `POST /{baseUrl}/content/outfits/{outfitId}/feature` | ❌ No feature/unfeature/approve/reject — only `DELETE` | Add all 4 endpoints |
| 8 | `POST /{baseUrl}/content/outfits/{outfitId}/unfeature` | ❌ Same as above | Same as above |
| 9 | `POST /{baseUrl}/content/outfits/{outfitId}/approve` | ❌ Same as above | Same as above |
| 10 | `POST /{baseUrl}/content/outfits/{outfitId}/reject` | ❌ Same as above | Same as above |
| 11 | `GET /{baseUrl}/system/health` | ❌ No system health endpoint | Add entire system management section |
| 12 | `GET /{baseUrl}/system/logs` | ❌ No system logs endpoint | Add system logs endpoint |
| 13 | `GET /{baseUrl}/system/performance` | ❌ No system perf endpoint | Add system performance endpoint |
| 14 | `POST /{baseUrl}/system/clear-cache` (body: `{cacheKey}`) | Backend: `POST /system/cache/clear` (body: string) | Fix route to `/system/cache/clear` or change frontend |
| 15 | `POST /{baseUrl}/system/restart/{serviceName}` | Backend: `POST /system/restart` (body: string `serviceName`) | Change frontend to POST body instead of path param |
| 16 | `POST /{baseUrl}/analytics/export` | ❌ No export analytics endpoint | Add export endpoint |

---

## 3. Route & Shape Mismatches

### 3.1 Base URL Pattern

| Side | Pattern | Notes |
|------|---------|-------|
| **Backend** | `[Route("api/[controller]")]` → `/api/Admin` | Route attribute on controller class |
| **Frontend datasource** | `` `${environment.baseUrl}/Admin` `` | Depends on whether `environment.baseUrl` includes `/api` |

**Fix needed:** Verify `environment.baseUrl` value. If it's `https://example.com/api`, then frontend sends to `/api/Admin` ✅. If it's `https://example.com`, then frontend sends to `/Admin` ❌.

### 3.2 PaginatedResult Structure

| Side | Shape |
|------|-------|
| **Frontend expects** | `{ data: T[], total: number, page: number, pageSize: number }` |
| **Backend returns** | `PaginatedResult<T>` — likely same structure |

✅ **Likely matches** — but verify the backend `PaginatedResult<T>` uses property names `data`, `total`, `page`, `pageSize`.

### 3.3 AnalyticsDashboard — Field Mapping

| Frontend Entity Field | Backend DTO Field | Status |
|-----------------------|-------------------|--------|
| `totalUsers` | (likely matches) | ✅ |
| `newUsersToday` | (likely matches) | ✅ |
| `activeUsers` | (likely matches) | ✅ |
| `totalOutfits` | (likely matches) | ✅ |
| `totalPosts` | (likely matches) | ✅ |
| `totalPolls` | (likely matches) | ✅ |
| `pendingReports` | (likely matches) | ✅ |
| `resolvedReports` | (likely matches) | ✅ |
| `lockedAccounts` | (likely matches) | ✅ |
| `bannedUsers` | (likely matches) | ✅ |

### 3.4 AdminPostDto — Field Mapping

| Frontend Entity Field | Matches Backend? | Notes |
|-----------------------|------------------|-------|
| `id`, `userId`, `userName` | Likely ✅ | Standard fields |
| `postType: number` | ⚠️ Verify | Backend enum may be string |
| `pollOptions: string[]` | ⚠️ Verify | Backend may have `PollOption` objects |
| `pollOptionVotes: number[]` | ⚠️ Verify | May need to flatten from vote objects |
| `isApproved: boolean` | ⚠️ Verify | Backend may not have this field |
| `status: string` | ⚠️ Verify | Backend may use `PostStatus` enum |

### 3.5 AdminPollDto — Field Mapping

| Frontend Entity Field | Matches Backend? | Notes |
|-----------------------|------------------|-------|
| `options: string[]` | ⚠️ Verify | Backend may use `PollOption` objects with `Text` property |
| `optionVotes: number[]` | ⚠️ Verify | May need to count from `PollVote` entities |
| `isFeatured`, `featuredAt`, `featuredBy` | ⚠️ Verify | Backend `Poll` entity may not have these fields |

### 3.6 AdminOutfitDto — Field Mapping

| Frontend Entity Field | Matches Backend? | Notes |
|-----------------------|------------------|-------|
| `imageUrls: string[]` | ⚠️ Verify | Backend `Outfit` has `ImageUrl` (single string) |
| `isFeatured`, `isApproved` | ⚠️ Verify | Backend `Outfit` entity may not have these |
| `likesCount`, `commentsCount` | ⚠️ Verify | Backend may not have computed counts |

---

## 4. Missing Backend Endpoints (Frontend Needs Them)

These endpoints don't exist in the backend but the frontend calls them. **They need to be added to AdminController.cs.**

### 4.1 Content Moderation — Approve/Reject

```
POST /api/admin/content/posts/{postId}/approve    → ApprovePostCommand
POST /api/admin/content/posts/{postId}/reject      → RejectPostCommand(body: { reason })
POST /api/admin/content/outfits/{outfitId}/feature  → FeatureOutfitCommand
POST /api/admin/content/outfits/{outfitId}/unfeature→ UnfeatureOutfitCommand
POST /api/admin/content/outfits/{outfitId}/approve  → ApproveOutfitCommand
POST /api/admin/content/outfits/{outfitId}/reject   → RejectOutfitCommand(body: { reason })
POST /api/admin/content/polls/{pollId}/feature      → FeaturePollCommand
POST /api/admin/content/polls/{pollId}/unfeature    → UnfeaturePollCommand
```

### 4.2 Account Management

```
POST /api/admin/unlock-account/{userId} → UnlockAccountCommand
```

### 4.3 Settings — Individual Update

```
PUT /api/admin/settings/{key} → UpdateSettingCommand(body: { value })
```

**Note:** The backend currently only has `POST /settings/maintenance` (a global toggle). Individual settings CRUD via `PUT /settings/{key}` needs to be implemented.

### 4.4 System Management

```
GET /api/admin/system/health        → GetSystemHealthQuery
GET /api/admin/system/logs           → GetSystemLogsQuery (filtered, paginated)
GET /api/admin/system/performance    → GetSystemPerformanceQuery
POST /api/admin/analytics/export     → ExportAnalyticsCommand(body: { format, dates })
```

### 4.5 Route Alignment — System Operations

| Current Backend | Frontend Expects | Fix |
|-----------------|------------------|-----|
| `POST /system/cache/clear` (body: string?) | `POST /system/clear-cache` (body: `{cacheKey}`) | Change backend route OR change frontend route |
| `POST /system/restart` (body: string) | `POST /system/restart/{serviceName}` | Align to use path param OR body |

---

## 5. Missing Frontend Consumers (Backend Has, Frontend Doesn't Call)

These backend endpoints exist but the frontend **does not call them** through the datasource:

| Backend Endpoint | Purpose | Frontend Status |
|------------------|---------|-----------------|
| `GET /roles` | List all roles | ❌ Not called — role management UI may be incomplete |
| `GET /roles/users` | Get user-role mappings | ❌ Not called |
| `GET /roles/management` | Get role management data | ❌ Not called |
| `POST /roles/assign` | Assign role to user | ❌ Not called |
| `POST /roles/update` | Update user role | ❌ Not called |
| `DELETE /roles/{userId}/{role}` | Remove role from user | ❌ Not called |
| `GET /users/{userId}/stats` | User statistics | ❌ Not called |
| `GET /activities/*` (7 endpoints) | User activity tracking | ❌ Not called |
| `GET /audit-logs/{id}` | Audit log detail | ❌ Not called |
| `GET /audit-logs/statistics` | Audit log stats | ❌ Not called |
| `GET /audit-logs/analytics` | Audit log analytics | ❌ Not called |
| `GET /audit-logs/trends` | Audit log trends | ❌ Not called |

---

## 6. Entity/DTO Shape Mismatches

### 6.1 ContentReport

| Frontend Field | Backend Likely Has | Issue |
|----------------|-------------------|-------|
| `reporterUserName` | `ReporterId` (FK to User) | May need join query in backend |
| `targetUserId` | `ReportedUserId` | Field name may differ |
| `contentType` | `ReferenceType` | Field name mismatch |
| `reason` | `Reason` (enum) | May be enum, frontend expects string |

### 6.2 Backend PostComment Reuse for Admin

The backend `GetPostsQuery` (line 332-334) reuses `AdminPostDto` for polls and outfits by filtering with `ContentType = "Poll"` or `"Outfit"`. This means:
- **Polls** are returned as `AdminPostDto` — but frontend expects `AdminPollDto` with different fields (question, options, votes, isFeatured)
- **Outfits** are returned as `AdminPostDto` — but frontend expects `AdminOutfitDto` with different fields (name, description, imageUrls)

**Fix needed:** Create separate proper handlers/queries:
- `GetPostsQuery` → returns `PaginatedResult<AdminPostDto>` (for feed posts only)
- `GetPollsQuery` → returns `PaginatedResult<AdminPollDto>` (for polls)
- `GetOutfitsQuery` → returns `PaginatedResult<AdminOutfitDto>` (for outfits)

### 6.3 AdminOutfitDto — ImageUrl(s)

| Frontend | Backend |
|----------|---------|
| `imageUrls: string[]` | `Outfit.ImageUrl: string` (single) |
| Multiple image URLs expected | Only one image URL stored |

**Fix:** Either change frontend to expect single URL, or extend backend to support multiple images (requires DB migration).

---

## 7. Auth & Route Configuration Issues

### 7.1 Base URL Verification

Check `src/outfit-planner-ui/src/environments/environment.ts`:

```typescript
// Current (verify this):
export const environment = {
  baseUrl: 'https://your-api.com/api'  // ← Does it include '/api'?
};
```

If `baseUrl` includes `/api`, then frontend sends to:
```
https://your-api.com/api/Admin/...
```
Backend expects:
```
[Route("api/[controller]")] → /api/Admin/...
```
✅ This matches.

If `baseUrl` does **not** include `/api`:
```
https://your-api.com/Admin/...
```
❌ This does NOT match — need to add `/api` prefix in datasource.

### 7.2 Admin Guard

**File:** `src/outfit-planner-ui/src/app/core/guards/admin-guard.ts`

```typescript
// Verify this guard exists and checks for 'Admin' role
@Injectable({ providedIn: 'root' })
export class AdminGuard implements CanActivate {
  canActivate(): boolean {
    // Should check JWT token for 'Admin' role claim
    // This depends on Phase 1 (Auth/Roles) being complete
  }
}
```

### 7.3 Role Configuration — Moderator Missing

| Plan | Backend | Frontend |
|------|---------|----------|
| Admin, Moderator, User (3 roles) | Admin only checked in `[Authorize(Roles = "Admin")]` | adminGuard likely checks for `"Admin"` only |

**Issue:** The `admin-guard.ts` may need to accept both `"Admin"` and `"Moderator"` roles for admin pages, with different permission levels.

---

## 8. Complete API Surface Comparison

### Backend Endpoints: 32 total

```
GET    /api/admin/locked-accounts
GET    /api/admin/analytics/dashboard
GET    /api/admin/analytics/realtime
GET    /api/admin/analytics/detailed
GET    /api/admin/roles
GET    /api/admin/roles/users
GET    /api/admin/roles/management
POST   /api/admin/roles/assign
POST   /api/admin/roles/update
DELETE /api/admin/roles/{userId}/{role}
GET    /api/admin/users
GET    /api/admin/users/{userId}
GET    /api/admin/users/{userId}/stats
POST   /api/admin/users/{userId}/ban
POST   /api/admin/users/{userId}/unban
GET    /api/admin/content/posts
DELETE /api/admin/content/posts/{postId}
POST   /api/admin/content/posts/bulk
GET    /api/admin/content/polls
POST   /api/admin/content/polls/{pollId}/close
DELETE /api/admin/content/polls/{pollId}
POST   /api/admin/content/polls/bulk
GET    /api/admin/content/outfits
DELETE /api/admin/content/outfits/{outfitId}
POST   /api/admin/content/outfits/bulk
GET    /api/admin/reports
GET    /api/admin/reports/{reportId}
POST   /api/admin/reports/{reportId}/resolve
GET    /api/admin/settings
POST   /api/admin/settings/maintenance
POST   /api/admin/system/backup
POST   /api/admin/system/restart
POST   /api/admin/system/cache/clear
GET    /api/admin/activities
GET    /api/admin/activities/{userId}/login-history
GET    /api/admin/activities/statistics
GET    /api/admin/activities/active-users
GET    /api/admin/activities/{userId}/session-info
GET    /api/admin/activities/analytics
GET    /api/admin/activities/trends
GET    /api/admin/audit-logs
GET    /api/admin/audit-logs/{id}
GET    /api/admin/audit-logs/statistics
GET    /api/admin/audit-logs/analytics
GET    /api/admin/audit-logs/trends
```

### Frontend Datasource Calls: 35 total

```
GET    /Admin/users
GET    /Admin/users/{userId}
POST   /Admin/users/{userId}/ban
POST   /Admin/users/{userId}/unban
GET    /Admin/reports
GET    /Admin/reports/{reportId}
POST   /Admin/reports/{reportId}/resolve
GET    /Admin/settings
PUT    /Admin/settings/{key}
GET    /Admin/analytics/dashboard
GET    /Admin/analytics/detailed
GET    /Admin/analytics/realtime
POST   /Admin/analytics/export
GET    /Admin/locked-accounts
POST   /Admin/unlock-account/{userId}
GET    /Admin/audit-logs
GET    /Admin/content/posts
DELETE /Admin/content/posts/{postId}
POST   /Admin/content/posts/{postId}/approve
POST   /Admin/content/posts/{postId}/reject
POST   /Admin/content/posts/bulk
GET    /Admin/content/polls
POST   /Admin/content/polls/{pollId}/close
POST   /Admin/content/polls/{pollId}/feature
POST   /Admin/content/polls/{pollId}/unfeature
DELETE /Admin/content/polls/{pollId}
POST   /Admin/content/polls/bulk
GET    /Admin/content/outfits
POST   /Admin/content/outfits/{outfitId}/feature
POST   /Admin/content/outfits/{outfitId}/unfeature
POST   /Admin/content/outfits/{outfitId}/approve
POST   /Admin/content/outfits/{outfitId}/reject
DELETE /Admin/content/outfits/{outfitId}
POST   /Admin/content/outfits/bulk
GET    /Admin/system/health
GET    /Admin/system/logs
GET    /Admin/system/performance
POST   /Admin/system/maintenance
POST   /Admin/system/backup
POST   /Admin/system/restart/{serviceName}
POST   /Admin/system/clear-cache
```

### Mismatch Summary

| Category | Backend Has | Frontend Calls | Match | Missing Backend | Missing Frontend |
|----------|-------------|----------------|-------|-----------------|------------------|
| **Users** | 5 | 4 | `list`, `detail`, `ban`, `unban` ✅ | — | `stats` ❌ |
| **Roles** | 6 | 0 | — | — | All 6 ❌ |
| **Reports** | 3 | 3 | ✅ Full match | — | — |
| **Posts** | 3 | 5 | `list`, `delete`, `bulk` ✅ | `approve`, `reject` ❌ | — |
| **Polls** | 4 | 6 | `list`, `close`, `delete`, `bulk` ✅ | `feature`, `unfeature` ❌ | — |
| **Outfits** | 3 | 7 | `list`, `delete`, `bulk` ✅ | `feature`, `unfeature`, `approve`, `reject` ❌ | — |
| **Analytics** | 3 | 4 | `dashboard`, `detailed`, `realtime` ✅ | `export` ❌ | — |
| **Locked Accts** | 1 | 2 | `list` ✅ | `unlock` ❌ | — |
| **Settings** | 2 | 2 | `list` ✅ | `update` (PUT) ❌ | — |
| **System** | 3 | 7 | `backup`, `maintenance` ✅ | `health`, `logs`, `perf`, `restart` route, `clear-cache` route ❌ | — |
| **Activities** | 7 | 0 | — | — | All 7 ❌ |
| **Audit Logs** | 5 | 1 | `list` ✅ | — | `detail`, `stats`, `analytics`, `trends` ❌ |

---

## 9. Implementation Phases

### Phase 1: Fix 404 Errors — Add Missing Backend Endpoints (Week 1)

**Goal:** Eliminate all 404 responses from the admin API.

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Add `POST /content/posts/{postId}/approve`, `POST /content/posts/{postId}/reject` | 🟡 Medium |
| **Day 2** | Add `POST /content/polls/{pollId}/feature`, `POST /content/polls/{pollId}/unfeature` | 🟡 Medium |
| **Day 3** | Add `POST /content/outfits/{outfitId}/feature`, `unfeature`, `approve`, `reject` | 🟡 Medium |
| **Day 4** | Add `POST /unlock-account/{userId}`, `PUT /settings/{key}` | 🟡 Medium |
| **Day 5** | Add `POST /analytics/export`, align `POST /system/restart` and `POST /system/clear-cache` routes | 🟡 Medium |

### Phase 2: Fix Content Type Separation (Week 2)

**Goal:** Stop returning `AdminPostDto` for polls/outfits. Create proper separate endpoints.

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Create `GetPollsQuery` handler returning `PaginatedResult<AdminPollDto>` | 🟡 Medium |
| **Day 2** | Create `GetOutfitsQuery` handler returning `PaginatedResult<AdminOutfitDto>` | 🟡 Medium |
| **Day 3** | Add missing DTO fields (`IsFeatured`, `IsApproved`, `imageUrls[]`, etc.) to backend entities if needed | 🟡 Medium |
| **Day 4** | Add DB migration for new fields on `Poll` and `Outfit` entities (if needed) | 🟡 Medium |
| **Day 5** | Update AdminController to use new dedicated handlers. Remove the old `ContentType` filter hack | 🟡 Medium |

### Phase 3: Add Missing System Management (Week 3)

**Goal:** Implement system health, logs, and performance endpoints.

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Add `GET /system/health` — returns DB connectivity, cache status, email service health | 🟡 Medium |
| **Day 2** | Add `GET /system/logs` — query logs from database or file system with filtering/pagination | 🟡 Medium |
| **Day 3** | Add `GET /system/performance` — return CPU, memory, disk, active connections, request rate | 🟡 Medium |
| **Day 4** | Create corresponding CQRS handlers and DTOs for all 3 endpoints | 🟡 Medium |
| **Day 5** | End-to-end test: frontend → backend → response → display in admin UI | 🟡 Medium |

### Phase 4: Wire Up Missing Frontend Consumers (Week 4)

**Goal:** Connect frontend to existing backend endpoints that are currently unused.

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Add role management calls to admin datasource (list roles, assign role, remove role) | 🟡 Medium |
| **Day 2** | Create role management UI components (role list, user-role assignment modal) | 🟡 Medium |
| **Day 3** | Add user statistics endpoint call to admin datasource. Wire to user detail page | 🟡 Medium |
| **Day 4** | Add user activity monitoring calls. Create activity list UI page | 🟡 Medium |
| **Day 5** | Add audit log detail/stats/analytics calls. Wire to audit log page with enhanced UI | 🟡 Medium |

### Phase 5: Route & Auth Alignment (Week 5)

**Goal:** Ensure all routes and auth checks are consistent.

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Verify `environment.baseUrl` includes `/api` prefix. Fix if not | 🟢 Easy |
| **Day 2** | Update `adminGuard` to support both `"Admin"` and `"Moderator"` roles | 🟡 Medium |
| **Day 3** | Verify `AnalyticsDashboardDto` and `AdminPostDto` shapes match frontend expectations | 🟡 Medium |
| **Day 4** | Add DTO profile mappings in AutoMapper for all admin DTOs | 🟡 Medium |
| **Day 5** | Full end-to-end admin panel test with all pages and all API calls | 🟡 Medium |

---

## 10. File Inventory — What Needs to Change

### Backend Files to Modify

| File | Changes Needed |
|------|----------------|
| `src/OutfitPlanner.Api/Controllers/AdminController.cs` | Add 12+ new endpoints (approve/reject posts, feature polls/outfits, unlock account, update setting, system health/logs/performance, export analytics) |
| `src/OutfitPlanner.Application/Features/Admin/` | Add ~15 new CQRS handlers for new endpoints. Create separate GetPollsQuery, GetOutfitsQuery handlers |
| `src/OutfitPlanner.Application/DTOs/Admin/` | Add new DTOs or update existing ones to match frontend expectations |
| `src/OutfitPlanner.Application/Profiles/MappingProfile.cs` | Add AutoMapper profiles for all admin entities ↔ DTO mappings |

### Backend Files to Add

| File | Purpose |
|------|---------|
| `Commands/ApprovePostCommand.cs` | Approve post handler |
| `Commands/RejectPostCommand.cs` | Reject post with reason |
| `Commands/FeaturePollCommand.cs` | Feature poll handler |
| `Commands/UnfeaturePollCommand.cs` | Unfeature poll handler |
| `Commands/FeatureOutfitCommand.cs` | Feature outfit handler |
| `Commands/UnfeatureOutfitCommand.cs` | Unfeature outfit handler |
| `Commands/ApproveOutfitCommand.cs` | Approve outfit handler |
| `Commands/RejectOutfitCommand.cs` | Reject outfit handler |
| `Commands/UnlockAccountCommand.cs` | Unlock user account |
| `Commands/UpdateSettingCommand.cs` | Update individual setting |
| `Commands/ExportAnalyticsCommand.cs` | Export analytics to file |
| `Queries/GetPollsQuery.cs` | Get polls with AdminPollDto (separate from posts) |
| `Queries/GetOutfitsQuery.cs` | Get outfits with AdminOutfitDto (separate from posts) |
| `Queries/GetSystemHealthQuery.cs` | System health check |
| `Queries/GetSystemLogsQuery.cs` | System logs with filtering |
| `Queries/GetSystemPerformanceQuery.cs` | System performance metrics |

### Frontend Files to Modify

| File | Changes Needed |
|------|----------------|
| `src/outfit-planner-ui/src/app/data/datasources/admin.datasource.ts` | Fix routes for restart, clear-cache. Add role management calls. Remove unused calls if needed |
| `src/outfit-planner-ui/src/app/domain/entities/admin.entity.ts` | Verify all field names match backend DTOs. Add any missing interfaces |
| `src/outfit-planner-ui/src/app/presentation/pages/admin/content/admin-posts.component.ts` | Fix approve/reject UI if backend endpoints now exist |
| `src/outfit-planner-ui/src/app/presentation/pages/admin/content/admin-polls.component.ts` | Add feature/unfeature UI controls |
| `src/outfit-planner-ui/src/app/presentation/pages/admin/content/admin-outfits.component.ts` | Add feature/unfeature/approve/reject UI controls |
| `src/outfit-planner-ui/src/app/presentation/pages/admin/system/admin-system.component.ts` | Wire system health/logs/performance to real data |
| `src/outfit-planner-ui/src/app/presentation/pages/admin/users/admin-users.component.ts` | Add role assignment UI, unlock account button |

### Frontend Files to Add

| File | Purpose |
|------|---------|
| `src/outfit-planner-ui/src/app/presentation/pages/admin/roles/` | New role management page component |

---

> **Quick Start — Top 3 Priority Fixes:**
> 1. Add missing approve/reject/feature/unfeature endpoints to `AdminController.cs` (eliminates the most 404 errors)
> 2. Separate `GetPollsQuery` and `GetOutfitsQuery` from the reused `GetPostsQuery` (fixes wrong DTO shapes)
> 3. Verify `environment.baseUrl` pattern matches backend route attribute `[Route("api/[controller]")]`