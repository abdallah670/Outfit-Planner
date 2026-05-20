# Admin Layer Alignment Plan

> **Goal:** Align the frontend (Angular/TypeScript) and backend (C# .NET) admin layers so that their DTOs, API endpoints, and state management match identically.
>
> **Admin Capabilities (per business rules):**
> - User management (list, detail, ban, unban, stats)
> - Report management (list, detail, resolve)
> - Settings management (list — not update)
> - **Role management** (list roles, assign/remove/update user roles)
> - **Content management** → only DELETE posts/outfits/polls, CLOSE polls
> - **Analytics** → view dashboard, detailed, realtime
> - **Audit logs** → view
> - **Locked accounts** → view

---

## 1. BACKEND API ENDPOINTS — Fully Implemented

### 1.1 Admin Endpoints (Frontend Should Consume) ✅

| # | Method | Route | Frontend Method |
|---|---|---|---|
| 1 | GET | `/api/admin/users` | `getUsers()` |
| 2 | GET | `/api/admin/users/{userId}` | `getUserDetail()` |
| 3 | GET | `/api/admin/users/{userId}/stats` | (optional) |
| 4 | POST | `/api/admin/users/{userId}/ban` | `banUser()` |
| 5 | POST | `/api/admin/users/{userId}/unban` | `unbanUser()` |
| 6 | GET | `/api/admin/locked-accounts` | `getLockedAccounts()` |
| 7 | GET | `/api/admin/reports` | `getReports()` |
| 8 | GET | `/api/admin/reports/{reportId}` | `getReportDetail()` |
| 9 | POST | `/api/admin/reports/{reportId}/resolve` | `resolveReport()` |
| 10 | GET | `/api/admin/settings` | `getSettings()` |
| 11 | GET | `/api/admin/audit-logs` | `getAuditLogs()` |
| 12 | GET | `/api/admin/analytics/dashboard` | `getDashboardAnalytics()` |
| 13 | GET | `/api/admin/analytics/realtime` | `getRealtimeAnalytics()` |
| 14 | GET | `/api/admin/analytics/detailed` | `getDetailedAnalytics()` |
| 15 | GET | `/api/admin/roles` | `getRoles()` 🆕 |
| 16 | GET | `/api/admin/roles/users` | `getUserRoles()` 🆕 |
| 17 | GET | `/api/admin/roles/management` | `getRoleManagement()` 🆕 |
| 18 | POST | `/api/admin/roles/assign` | `assignRole()` 🆕 |
| 19 | POST | `/api/admin/roles/update` | `updateUserRole()` 🆕 |
| 20 | DELETE | `/api/admin/roles/{userId}/{role}` | `removeRole()` 🆕 |
| 21 | GET | `/api/admin/content/posts` | `getPosts()` |
| 22 | DELETE | `/api/admin/content/posts/{postId}` | `deletePost()` |
| 23 | POST | `/api/admin/content/posts/bulk` | `bulkPostOperations()` |
| 24 | GET | `/api/admin/content/polls` | `getPolls()` |
| 25 | POST | `/api/admin/content/polls/{pollId}/close` | `closePoll()` |
| 26 | DELETE | `/api/admin/content/polls/{pollId}` | `deletePoll()` |
| 27 | POST | `/api/admin/content/polls/bulk` | `bulkPollOperations()` |
| 28 | GET | `/api/admin/content/outfits` | `getOutfits()` |
| 29 | DELETE | `/api/admin/content/outfits/{outfitId}` | `deleteOutfit()` |
| 30 | POST | `/api/admin/content/outfits/bulk` | `bulkOutfitOperations()` |

### 1.2 Backend-Only Endpoints (NOT for frontend consumption)

These exist in the backend for server/internal use only:
- `POST /settings/maintenance`
- `POST /system/backup`, `POST /system/restart`, `POST /system/cache/clear`
- `GET /activities/**` (7 endpoints — user activity tracking is backend-internal)

---

## 2. FRONTEND METHODS — What to Change

### 2.1 Methods to KEEP (23 methods — already match backend) ✅

`getUsers`, `getUserDetail`, `banUser`, `unbanUser`, `getReports`, `getReportDetail`, `resolveReport`, `getSettings`, `getAuditLogs`, `getLockedAccounts`, `getDashboardAnalytics`, `getDetailedAnalytics`, `getRealtimeAnalytics`, `getPosts`, `deletePost`, `bulkPostOperations`, `getPolls`, `closePoll`, `deletePoll`, `bulkPollOperations`, `getOutfits`, `deleteOutfit`, `bulkOutfitOperations`

### 2.2 Methods to REMOVE (18 methods — no backend support or not admin capability) 🗑️

`updateSetting`, `unlockAccount`, `approvePost`, `rejectPost`, `featurePoll`, `unfeaturePoll`, `featureOutfit`, `unfeatureOutfit`, `approveOutfit`, `rejectOutfit`, `getSystemHealth`, `getSystemLogs`, `getSystemPerformance`, `setMaintenanceMode`, `createBackup`, `restartService`, `clearCache`, `exportAnalytics`

### 2.3 Methods to ADD (6 methods — role management, backend ready) 🆕

`getRoles`, `getUserRoles`, `getRoleManagement`, `assignRole`, `updateUserRole`, `removeRole`

---

## 3. DTO/ENTITY CHANGES

### 3.1 Fields to REMOVE from Frontend 🗑️
- `AdminPostDto`: `isApproved`, `status`, `approvedAt`, `approvedBy`
- `ContentFilterRequest`: `status`
- `DetailedAnalyticsDto`: `systemStats`
- `RealtimeAnalyticsDto`: `systemStats`
- All system management entities (health, logs, performance, etc.)

### 3.2 Fields to FIX
- `BulkPollOperationItem`: `postId` → `pollId`
- `BulkOutfitOperationItem`: `postId` → `outfitId`

### 3.3 Entities to ADD 🆕
- `RoleDto`, `UserRoleDto`, `RoleAssignmentRequest`, `RoleManagementDto`

---

## 4. IMPLEMENTATION TASKS

| Task | Files | Description |
|---|---|---|
| **1. Strip unused methods** | `admin.repository.ts`, `admin.repository.impl.ts`, `admin.datasource.ts`, `admin.usecases.ts` | Remove 18 methods |
| **2. Simplify entities** | `admin.entity.ts` | Remove ~10 fields, fix 2 field names, remove system entities |
| **3. Add role management** | Same 5 files + `admin.entity.ts` | Add 4 interfaces + 6 methods |
| **4. Expand NgRx** | `admin.actions.ts`, `admin.reducer.ts`, `admin.effects.ts`, `admin.selectors.ts` | Add role management actions/reducer/effects/selectors; remove system actions |
| **5. Audit UI components** | All admin page `.ts`/`.html` files | Remove feature/approve/reject buttons; add role management UI |