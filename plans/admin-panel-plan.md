# Admin Panel Plan for Outfit-Planner

> **Goal**: Add a comprehensive admin panel with role-based access control, user management, content moderation, analytics, and system configuration.

---

## Table of Contents
1. [Current State Analysis](#1-current-state-analysis)
2. [Architecture Overview](#2-architecture-overview)
3. [Phase 1: Role System & Backend Foundation](#3-phase-1-role-system--backend-foundation)
4. [Phase 2: Admin API Endpoints](#4-phase-2-admin-api-endpoints)
5. [Phase 3: Frontend Admin Module](#5-phase-3-frontend-admin-module)
6. [Phase 4: Admin UI Pages](#6-phase-4-admin-ui-pages)
7. [Phase 5: Advanced Admin Features](#7-phase-5-advanced-admin-features)
8. [Technology & Dependencies](#8-technology--dependencies)
9. [Project Structure](#9-project-structure)
10. [Implementation Roadmap](#10-implementation-roadmap)

---

## 1. Current State Analysis

### What Exists Today

| Feature | Status | Notes |
|---------|--------|-------|
| Authentication | ✅ Basic | Email/password + OAuth (Google/Facebook) |
| Role System | ❌ Missing | No roles, no authorization levels |
| Admin User | ❌ Fake | "admin" username exists but has zero special privileges |
| Admin Routes | ❌ Missing | No admin area on frontend or backend |
| Content Moderation | ❌ Missing | No way to flag/review inappropriate content |
| User Management | ❌ Missing | No user listing, disable, or role assignment |
| Audit Logging | ❌ Missing | No admin action tracking |
| System Settings | ❌ Missing | No feature toggles, maintenance mode |
| Analytics Dashboard | ❌ Missing | No aggregate platform metrics |
| Hangfire Dashboard | ⚠️ Partial | Exists at `/hangfire` but unprotected in production |

### Key Findings

1. **ASP.NET Core Identity** is already used (`UserManager<User>`, `IdentityUser`), so the role infrastructure is built-in — just needs to be enabled
2. **No `[Authorize(Roles = "...")]`** anywhere — all controllers use `[Authorize]` (any authenticated user)
3. **No `adminGuard`** on frontend — only `authGuard` checking `isAuthenticated()`
4. **JWT tokens** already exist — just need to add role claims
5. **Hangfire** already configured — just needs admin authorization filter updated

---

## 2. Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│                    Angular Frontend                           │
│  ┌────────────────────────────────────────────────────────┐  │
│  │              Admin Module (admin/)                      │  │
│  │  [Dashboard] [Users] [Moderation] [Analytics]          │  │
│  │  [Settings] [Audit Log] [Images] [Broadcast]           │  │
│  │  ┌──────────────┐                                      │  │
│  │  │ adminGuard    │ ← Checks JWT for "Admin" role       │  │
│  │  └──────────────┘                                      │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────┬───────────────────────────────────┘
                           │ HTTP + JWT (with role claim)
┌──────────────────────────▼───────────────────────────────────┐
│                 .NET Backend                                  │
│  ┌────────────────────────────────────────────────────────┐  │
│  │             AdminController (api/admin/)                │  │
│  │  [Authorize(Roles = "Admin")]                          │  │
│  │  ┌────────────┐ ┌────────────┐ ┌──────────────────┐   │  │
│  │  │ User Mgmt  │ │ Content    │ │ Analytics        │   │  │
│  │  │ Endpoints  │ │ Moderation  │ │ Endpoints        │   │  │
│  │  └────────────┘ └────────────┘ └──────────────────┘   │  │
│  │  ┌────────────┐ ┌────────────┐ ┌──────────────────┐   │  │
│  │  │ System     │ │ Audit Log  │ │ Seed/Image Mgmt  │   │  │
│  │  │ Settings   │ │ Endpoints  │ │ Endpoints        │   │  │
│  │  └────────────┘ └────────────┘ └──────────────────┘   │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐  │
│  │           ASP.NET Core Identity Roles                   │  │
│  │  ┌──────────┐  ┌──────────────┐  ┌──────────────────┐  │  │
│  │  │ Admin    │  │ Moderator   │  │ User (default)   │  │  │
│  │  │ Role     │  │ Role        │  │ Role             │  │  │
│  │  └──────────┘  └──────────────┘  └──────────────────┘  │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐  │
│  │           New Domain Entities                           │  │
│  │  - AuditLog (AdminAction, Timestamp, User, Details)    │  │
│  │  - SystemSetting (Key, Value, Type, Description)       │  │
│  │  - ContentReport (TargetId, ReporterId, Reason, Status)│  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

### Role Hierarchy

```
Admin ────── Full access to everything
  │
Moderator ── Content moderation, user warnings, report management
  │
User ─────── Standard application features (current default)
```

---

## 3. Phase 1: Role System & Backend Foundation

### 3.1 Domain Changes

#### New Enum: `UserRole`
```csharp
// src/OutfitPlanner.Domain/Enums/UserRole.cs
public enum UserRole
{
    User = 0,
    Moderator = 1,
    Admin = 2
}
```

#### New Entity: `AuditLog`
```csharp
// src/OutfitPlanner.Domain/Entities/AuditLog.cs
public class AuditLog : BaseEntity
{
    public string AdminUserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;       // e.g. "UserDisabled", "PostRemoved"
    public string TargetType { get; set; } = string.Empty;   // e.g. "User", "FeedPost", "Outfit"
    public string? TargetId { get; set; }
    public string? Details { get; set; }                     // JSON with before/after state
    public string? IpAddress { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    // Navigation
    public User AdminUser { get; set; } = null!;
}
```

#### New Entity: `SystemSetting`
```csharp
// src/OutfitPlanner.Domain/Entities/SystemSetting.cs
public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;           // "maintenance_mode", "max_upload_size"
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "string";              // "boolean", "number", "string", "json"
    public string? Description { get; set; }
    public string? Category { get; set; }                     // "general", "security", "features"
    public bool IsEditable { get; set; } = true;
}
```

#### New Entity: `ContentReport`
```csharp
// src/OutfitPlanner.Domain/Entities/ContentReport.cs
public class ContentReport : BaseEntity
{
    public string ReporterId { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;   // "FeedPost", "Comment", "Outfit", "User"
    public string TargetId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;       // "spam", "inappropriate", "harassment", "other"
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? ReviewedById { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? Resolution { get; set; }                  // "dismissed", "warning", "removed", "banned"
    
    // Navigation
    public User Reporter { get; set; } = null!;
    public User? ReviewedBy { get; set; }
}

public enum ReportStatus
{
    Pending = 0,
    Reviewed = 1,
    Dismissed = 2
}
```

### 3.2 Database Changes

#### New DbSets in `AppDbContext`
```csharp
public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
public DbSet<ContentReport> ContentReports => Set<ContentReport>();
```

#### New Migration
- Add `AuditLogs`, `SystemSettings`, `ContentReports` tables
- Add `IdentityUserRole<string>` configuration (ASP.NET Core Identity handles roles)

### 3.3 JWT Changes

Add role claims to JWT token generation in `IJWTService`:

```csharp
// In JWTService.cs - Add role claims to existing claims list
var roles = await _userManager.GetRolesAsync(user);
var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));
// ... add to existing claims list before token creation
```

### 3.4 DataSeeder Changes

Add role creation and assignment:

```csharp
// In SeedUsersAsync or new SeedRolesAsync
private async Task SeedRolesAsync()
{
    string[] roles = { "Admin", "Moderator", "User" };
    foreach (var roleName in roles)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
    
    // Assign Admin role to admin user
    var adminUser = await _userManager.FindByNameAsync("admin");
    if (adminUser != null && !await _userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await _userManager.AddToRoleAsync(adminUser, "Admin");
    }
    
    // Assign Moderator role to second user
    var modUser = await _userManager.FindByNameAsync("StyleMaven92");
    if (modUser != null && !await _userManager.IsInRoleAsync(modUser, "Moderator"))
    {
        await _userManager.AddToRoleAsync(modUser, "Moderator");
    }
    
    // All users get "User" role by default
    foreach (var user in await _userManager.Users.ToListAsync())
    {
        if (!await _userManager.IsInRoleAsync(user, "User"))
        {
            await _userManager.AddToRoleAsync(user, "User");
        }
    }
}
```

### 3.5 Seed System Default Settings
```csharp
var defaultSettings = new List<SystemSetting>
{
    new() { Key = "app.maintenance_mode", Value = "false", Type = "boolean", 
            Description = "Put the app in maintenance mode", Category = "general" },
    new() { Key = "app.registration_open", Value = "true", Type = "boolean",
            Description = "Allow new user registrations", Category = "security" },
    new() { Key = "app.max_upload_size_mb", Value = "10", Type = "number",
            Description = "Maximum file upload size in MB", Category = "features" },
    new() { Key = "app.require_email_verification", Value = "false", Type = "boolean",
            Description = "Require email verification before using the app", Category = "security" },
    new() { Key = "content.auto_moderate", Value = "true", Type = "boolean",
            Description = "Automatically flag potentially inappropriate content", Category = "features" },
    new() { Key = "trending.calculation_interval_hours", Value = "24", Type = "number",
            Description = "How often trending is recalculated", Category = "features" },
};
```

---

## 4. Phase 2: Admin API Endpoints

### 4.1 New AdminController

```csharp
// src/OutfitPlanner.Api/Controllers/AdminController.cs
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    // ===== DASHBOARD =====
    // GET  /api/admin/dashboard            → Platform overview metrics
    // GET  /api/admin/dashboard/charts     → Chart data for analytics
    
    // ===== USER MANAGEMENT =====
    // GET    /api/admin/users              → List all users (paginated, filterable)
    // GET    /api/admin/users/{id}         → Get user details
    // PUT    /api/admin/users/{id}/role    → Change user role
    // POST   /api/admin/users/{id}/disable → Disable user account
    // POST   /api/admin/users/{id}/enable  → Re-enable user account
    // DELETE /api/admin/users/{id}         → Soft-delete / purge user
    
    // ===== CONTENT MODERATION =====
    // GET    /api/admin/reports            → List content reports
    // GET    /api/admin/reports/{id}       → Get report details
    // POST   /api/admin/reports/{id}/resolve → Resolve a report
    // GET    /api/admin/flagged-content    → List recently flagged content
    
    // ===== ANALYTICS =====
    // GET    /api/admin/analytics/users    → User registration trends
    // GET    /api/admin/analytics/outfits  → Outfit creation trends
    // GET    /api/admin/analytics/engagement → Feed/poll engagement metrics
    // GET    /api/admin/analytics/wardrobe → Wardrobe item statistics
    
    // ===== SYSTEM SETTINGS =====
    // GET    /api/admin/settings           → List all settings
    // PUT    /api/admin/settings/{key}     → Update a setting
    // POST   /api/admin/settings           → Create a new setting
    
    // ===== AUDIT LOG =====
    // GET    /api/admin/audit-log          → List audit log entries (paginated)
    // GET    /api/admin/audit-log/{id}     → Get audit entry details
    
    // ===== IMAGE MANAGEMENT =====
    // GET    /api/admin/images             → List uploaded images
    // DELETE /api/admin/images/{id}        → Remove an image
    // POST   /api/admin/images/cleanup     → Remove orphaned images
    
    // ===== TRENDING =====
    // POST   /api/admin/trending/trigger   → Manually trigger trending calculation
    // POST   /api/admin/trending/override  → Override trending rankings
    
    // ===== SEED MANAGEMENT =====
    // POST   /api/admin/seed/trigger       → Re-run data seeding
    // GET    /api/admin/seed/status        → Check seed status
    
    // ===== NOTIFICATIONS (Broadcast) =====
    // POST   /api/admin/notifications/broadcast → Send notification to all users
}
```

### 4.2 New DTOs

```csharp
// src/OutfitPlanner.Application/DTOs/Admin/

// Dashboard
public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveToday { get; set; }
    public int TotalOutfits { get; set; }
    public int TotalClothingItems { get; set; }
    public int TotalFeedPosts { get; set; }
    public int PendingReports { get; set; }
    public int TotalPolls { get; set; }
    public double AvgOutfitsPerUser { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; }
}

// Users
public class UserListItemDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? Name { get; set; }
    public List<string> Roles { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLogin { get; set; }
    public int OutfitCount { get; set; }
    public int ClothingItemCount { get; set; }
}

public class UpdateUserRoleDto
{
    public string Role { get; set; }  // "Admin", "Moderator", "User"
}

// Reports
public class ContentReportDto
{
    public Guid Id { get; set; }
    public string TargetType { get; set; }
    public string TargetId { get; set; }
    public string Reason { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
    public string ReporterUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ResolveReportDto
{
    public string Resolution { get; set; }  // "dismissed", "warning", "removed", "banned"
    public string? AdminNote { get; set; }
}

// Settings
public class SystemSettingDto
{
    public string Key { get; set; }
    public string Value { get; set; }
    public string Type { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsEditable { get; set; }
}

public class UpdateSettingDto
{
    public string Value { get; set; }
}

// Audit Log
public class AuditLogDto
{
    public Guid Id { get; set; }
    public string AdminUserName { get; set; }
    public string Action { get; set; }
    public string TargetType { get; set; }
    public string? TargetId { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

// Broadcast
public class BroadcastNotificationDto
{
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; } = NotificationType.System;
    public string? ActionUrl { get; set; }
}
```

### 4.3 Authorization

#### Backend: Role-based Authorization Attribute
```csharp
// On AdminController
[Authorize(Roles = "Admin")]
// On specific endpoints for Moderators
[Authorize(Roles = "Admin,Moderator")]
```

#### Hangfire Dashboard Authorization (Update Existing Filter)
```csharp
// src/OutfitPlanner.Api/Middleware/HangfireAuthorizationFilter.cs
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        // Check if user is authenticated
        if (!httpContext.User.Identity?.IsAuthenticated ?? false)
            return false;
        // Check if user is Admin
        return httpContext.User.IsInRole("Admin");
    }
}
```

---

## 5. Phase 3: Frontend Admin Module

### 5.1 Admin Guard

```typescript
// src/outfit-planner-ui/src/app/core/guards/admin-guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { map, take } from 'rxjs/operators';

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  return authService.user$.pipe(
    take(1),
    map(user => {
      if (user?.roles?.includes('Admin') || user?.roles?.includes('Moderator')) {
        return true;
      }
      router.navigate(['/']);
      return false;
    })
  );
};
```

### 5.2 JWT Role Parsing

Update the auth service to extract roles from JWT:

```typescript
// In AuthService - add to token parsing logic
private parseUserFromToken(token: string): User | null {
  const decoded = this.jwtDecode(token);
  const roles = decoded['role'] 
    ? (Array.isArray(decoded['role']) ? decoded['role'] : [decoded['role']])
    : ['User'];
  
  return {
    id: decoded['uid'],
    username: decoded['sub'],
    email: decoded['email'],
    roles: roles,
    // ... existing fields
  };
}
```

### 5.3 Admin Layout

```typescript
// src/outfit-planner-ui/src/app/presentation/layouts/admin-layout/
// AdminLayoutComponent - dedicated layout with:
// - Left sidebar with admin navigation
// - Top header with admin branding
// - Content area
// - Different from the main app layout
```

### 5.4 Admin Routes

```typescript
// In app.routes.ts - add admin route group
{
  path: 'admin',
  canActivate: [authGuard, adminGuard],
  component: AdminLayoutComponent,
  children: [
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: 'dashboard', component: AdminDashboardComponent },
    { path: 'users', component: AdminUsersComponent },
    { path: 'users/:id', component: AdminUserDetailComponent },
    { path: 'moderation', component: AdminModerationComponent },
    { path: 'moderation/:id', component: AdminReportDetailComponent },
    { path: 'analytics', component: AdminAnalyticsComponent },
    { path: 'settings', component: AdminSettingsComponent },
    { path: 'audit-log', component: AdminAuditLogComponent },
    { path: 'images', component: AdminImagesComponent },
    { path: 'trending', component: AdminTrendingComponent },
    { path: 'seed', component: AdminSeedManagementComponent },
    { path: 'broadcast', component: AdminBroadcastComponent },
  ]
}
```

### 5.5 Admin NgRx State

```typescript
// src/outfit-planner-ui/src/app/core/state/admin/

// admin.actions.ts
export class LoadAdminDashboard { static readonly type = '[Admin] Load Dashboard'; }
export class LoadAdminDashboardSuccess { constructor(public dashboard: AdminDashboard) {} }

export class LoadUsers { constructor(public params: UserListParams) {} }
export class LoadUsersSuccess { constructor(public users: UserListItem[], public total: number) {} }
export class UpdateUserRole { constructor(public userId: string, public role: string) {} }
export class ToggleUserStatus { constructor(public userId: string, public enable: boolean) {} }

export class LoadReports { constructor(public status?: string) {} }
export class ResolveReport { constructor(public reportId: string, public resolution: string) {} }

export class LoadSettings {}
export class UpdateSetting { constructor(public key: string, public value: string) {} }

export class LoadAuditLog { constructor(public page: number, public pageSize: number) {} }
export class BroadcastNotification { constructor(public notification: BroadcastPayload) {} }

// admin.reducer.ts
interface AdminState {
  dashboard: AdminDashboard | null;
  users: UserListItem[];
  usersTotal: number;
  usersLoading: boolean;
  reports: ContentReport[];
  reportsLoading: boolean;
  settings: SystemSetting[];
  settingsLoading: boolean;
  auditLog: AuditLogEntry[];
  auditLogTotal: number;
  loading: boolean;
  error: string | null;
}

// admin.selectors.ts
export const selectAdminDashboard = (state: AppState) => state.admin.dashboard;
export const selectUsers = (state: AppState) => state.admin.users;
export const selectReports = (state: AppState) => state.admin.reports;
export const selectSettings = (state: AppState) => state.admin.settings;
export const selectAdminLoading = (state: AppState) => state.admin.loading;
```

### 5.6 Admin Data Layer

```typescript
// src/outfit-planner-ui/src/app/data/datasources/admin.datasource.ts
@Injectable({ providedIn: 'root' })
export class AdminDataSource {
  constructor(private http: HttpClient) {}
  
  getDashboard(): Observable<AdminDashboard> { ... }
  getUsers(params: UserListParams): Observable<PaginatedResult<UserListItem>> { ... }
  getUser(id: string): Observable<UserDetail> { ... }
  updateUserRole(userId: string, role: string): Observable<void> { ... }
  toggleUserStatus(userId: string, enable: boolean): Observable<void> { ... }
  getReports(status?: string): Observable<ContentReport[]> { ... }
  resolveReport(reportId: string, resolution: ResolveReportDto): Observable<void> { ... }
  getSettings(): Observable<SystemSetting[]> { ... }
  updateSetting(key: string, value: string): Observable<void> { ... }
  getAuditLog(page: number, pageSize: number): Observable<PaginatedResult<AuditLogEntry>> { ... }
  broadcastNotification(payload: BroadcastPayload): Observable<void> { ... }
  triggerTrendingCalculation(): Observable<void> { ... }
  triggerSeed(): Observable<void> { ... }
  getImageList(): Observable<ImageInfo[]> { ... }
  deleteImage(id: string): Observable<void> { ... }
  cleanupImages(): Observable<CleanupResult> { ... }
}
```

---

## 6. Phase 4: Admin UI Pages

### 6.1 Admin Dashboard Page

```
┌─────────────────────────────────────────────────────────────┐
│  🔧 Admin Dashboard                                          │
│                                                               │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐   │
│  │ 245       │ │ 38       │ │ 1,024    │ │ 12          │   │
│  │ Users     │ │ Active   │ │ Outfits  │ │ Pending     │   │
│  │           │ │ Today    │ │          │ │ Reports     │   │
│  └──────────┘ └──────────┘ └──────────┘ └──────────────┘   │
│                                                               │
│  📊 User Registrations (Last 30 Days)                        │
│  ████████████████░░░░░░░░░░░░░░░░░░  60 this month           │
│                                                               │
│  📊 Outfit Creation Trends                                   │
│  ██████████████████████░░░░░░░░░░░░  120 this month          │
│                                                               │
│  Recent Activity (Last 5 admin actions)                       │
│  • Disabled user "spammer123"                     2m ago      │
│  • Updated system setting "max_upload_size"       15m ago     │
│  • Resolved report #42 - removed post             1h ago      │
│  • Changed user "StyleMaven92" to Moderator       2h ago      │
│  • Triggered trending calculation                  3h ago      │
└─────────────────────────────────────────────────────────────┘
```

### 6.2 User Management Page

```
┌─────────────────────────────────────────────────────────────┐
│  👥 User Management                                          │
│  [🔍 Search users...]  [Role: ▼ All]  [Status: ▼ Active]    │
│                                                               │
│  ┌────┬────────────┬────────────┬──────────┬──────┬────────┐│
│  │ #  │ Username   │ Email      │ Roles    │ Items│ Actions ││
│  ├────┼────────────┼────────────┼──────────┼──────┼────────┤│
│  │ 1  │ admin      │ admin@..   │ 👑 Admin │ 14   │ [---]  ││
│  │ 2  │ StyleMaven │ style@..   │ 🛡️ Mod   │ 22   │ [---]  ││
│  │ 3  │ Fashionista│ alex@..    │ 👤 User  │ 7    │ [---]  ││
│  │ 4  │ spamer123  │ spam@..    │ 👤 User  │ 0    │ [---]  ││
│  └────┴────────────┴────────────┴──────────┴──────┴────────┘│
│                                                               │
│  [1] [2] [3] ... [▶]   Total: 6 users                        │
│                                                               │
│  ── User Detail Modal ──                                      │
│  ┌──────────────────────────────────────┐                    │
│  │ User: StyleMaven92                    │                    │
│  │ Email: stylemaven92@example.com       │                    │
│  │ Current Role: 🛡️ Moderator           │                    │
│  │ [▼ Change Role to: User/Moderator/Admin] [Save]          │
│  │ [🔴 Disable Account]  [⚠️ Force Logout]                   │
│  │                                                           │
│  │ Stats: 22 items, 15 outfits, 8 polls, 34 posts           │
│  │ Joined: 30 days ago                                       │
│  └──────────────────────────────────────┘                    │
└─────────────────────────────────────────────────────────────┘
```

### 6.3 Content Moderation Page

```
┌─────────────────────────────────────────────────────────────┐
│  🚩 Content Moderation                                       │
│  [Status: ▼ Pending]  [Type: ▼ All]                          │
│                                                               │
│  ┌────┬──────────┬───────────┬──────────┬───────┬─────────┐ │
│  │ #  │ Type     │ Reason    │ Reporter │ Date  │ Actions │ │
│  ├────┼──────────┼───────────┼──────────┼───────┼─────────┤ │
│  │ 1  │ FeedPost │ Spam      │ admin    │ 2m ago│ [View]  │ │
│  │ 2  │ Comment  │ Harassmnt │ StyleM.. │ 1h ago│ [View]  │ │
│  │ 3  │ Outfit   │ Inappropr │ Fashion. │ 3h ago│ [View]  │ │
│  │ 4  │ User     │ Fake Acc  │ admin    │ 5h ago│ [View]  │ │
│  └────┴──────────┴───────────┴──────────┴───────┴─────────┘ │
│                                                               │
│  ── Report Detail Modal ──                                    │
│  ┌─────────────────────────────────────────────┐             │
│  │ Target: Feed Post #142                       │             │
│  │ Content: "..." (preview)                     │             │
│  │ Reporter: admin (Reason: Spam)               │             │
│  │ Description: "Multiple identical posts"      │             │
│  │                                              │             │
│  │ [✅ Dismiss] [⚠️ Warn User] [🗑️ Remove]     │             │
│  │ [🔨 Ban User]                               │             │
│  └─────────────────────────────────────────────┘             │
└─────────────────────────────────────────────────────────────┘
```

### 6.4 Analytics Page

```
┌─────────────────────────────────────────────────────────────┐
│  📊 Platform Analytics                                       │
│  [📅 Last 7 Days ▼]  [Apply]                                 │
│                                                               │
│  ┌──────────────────┐  ┌──────────────────┐                 │
│  │  📈 User Growth   │  │  👕 Outfit Trends │                 │
│  │  ██│██│██│██│░░  │  │  ██│██│██│██│██  │                 │
│  │  M  T  W  T  F   │  │  M  T  W  T  F   │                 │
│  └──────────────────┘  └──────────────────┘                 │
│                                                               │
│  ┌──────────────────┐  ┌──────────────────┐                 │
│  │  💬 Engagement    │  │  📊 Category      │                 │
│  │  ██│██│░░│██│░░  │  │  ▓▓▓▓▓▓▒▒▒▒░░░░   │                 │
│  │  M  T  W  T  F   │  │  Tops Bottoms Shoes│                 │
│  └──────────────────┘  └──────────────────┘                 │
│                                                               │
│  Top Users by Outfit Count                                    │
│  1. StyleMaven92  —  22 outfits                              │
│  2. Fashionista_A —  15 outfits                              │
│  3. admin         —  14 outfits                              │
└─────────────────────────────────────────────────────────────┘
```

### 6.5 System Settings Page

```
┌─────────────────────────────────────────────────────────────┐
│  ⚙️ System Settings                                           │
│                                                               │
│  ┌─ General ──────────────────────────────────────────────┐ │
│  │ ☐ Maintenance Mode                   [🔴 Off] [Save] │ │
│  │ ☐ Allow New Registrations            [🟢 On]  [Save] │ │
│  │ Max Upload Size: [ 10 ] MB                    [Save] │ │
│  └────────────────────────────────────────────────────────┘ │
│  ┌─ Security ─────────────────────────────────────────────┐  │
│  │ ☐ Require Email Verification          [🔴 Off] [Save]  │  │
│  │ Max Login Attempts: [ 5 ]                      [Save]  │  │
│  └────────────────────────────────────────────────────────┘  │
│  ┌─ Features ──────────────────────────────────────────────┐ │
│  │ ☐ Auto Moderate Content               [🟢 On]  [Save]  │ │
│  │ ☐ Enable AI Features                  [🔴 Off] [Save]  │ │
│  │ Trending Interval: [ 24 ] hours                  [Save] │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### 6.6 Audit Log Page

```
┌─────────────────────────────────────────────────────────────┐
│  📋 Audit Log                                                │
│  [🔍 Search actions...]  [Action: ▼ All]  [Date Range: ...]  │
│                                                               │
│  ┌────┬──────────┬────────────┬──────────┬──────────┬──────┐ │
│  │ #  │ Admin    │ Action     │ Target   │ Target ID│ Date │ │
│  ├────┼──────────┼────────────┼──────────┼──────────┼──────┤ │
│  │ 1  │ admin    │ Disabled   │ User     │ spamer123│ 2m   │ │
│  │ 2  │ admin    │ Updated    │ Setting  │ max_upl..│ 15m  │ │
│  │ 3  │ admin    │ Resolved   │ Report   │ #42      │ 1h   │ │
│  └────┴──────────┴────────────┴──────────┴──────────┴──────┘ │
│                                                               │
│  [1] [2] [3] ... [▶]            Showing 10 of 47 entries     │
└─────────────────────────────────────────────────────────────┘
```

### 6.7 Additional Admin Pages

#### Image Manager
```
┌─────────────────────────────────────────────────────────────┐
│  🖼️ Uploaded Images Management                               │
│  [🔍 Filter by user/type...]  [🗑️ Cleanup Orphans]           │
│                                                               │
│  ┌────┬──────────┬──────────┬─────────┬────────┬──────────┐ │
│  │    │ Image    │ Owner    │ Type    │ Size   │ Uploaded │ │
│  ├────┼──────────┼──────────┼─────────┼────────┼──────────┤ │
│  │ 🖼 │ top-42   │ admin    │ Clothing│ 12 KB  │ 2d ago   │ │
│  │ 🖼 │ outfit.. │ StyleMa..│ Outfit  │ 45 KB  │ 1d ago   │ │
│  │ 🖼 │ profile  │ Fashion..│ Avatar  │ 8 KB   │ 5h ago   │ │
│  └────┴──────────┴──────────┴─────────┴────────┴──────────┘ │
│  [🗑️ Delete Selected]                                         │
└─────────────────────────────────────────────────────────────┘
```

#### Trending Override
```
┌─────────────────────────────────────────────────────────────┐
│  🔥 Trending Management                                      │
│  Current Trending - Week of May 4                            │
│                                                               │
│  #1. "Casual Friday Look"          Score: 95.2  [🔒 Lock]   │
│  #2. "Weekend Warrior"             Score: 88.7  [🔒 Lock]   │
│  #3. "Date Night Style"            Score: 84.1  [🔒 Lock]   │
│                                                               │
│  [🔄 Recalculate Now]  Last calculated: 2h ago               │
│                                                               │
│  ── Manual Override ──                                        │
│  Select Outfit: [▼ Search outfits...]                         │
│  Position: [ 1 ]  [Apply Override]                            │
└─────────────────────────────────────────────────────────────┘
```

#### Broadcast Notification
```
┌─────────────────────────────────────────────────────────────┐
│  📢 Broadcast Notification                                    │
│  Title: [System Maintenance Tonight]                         │
│  Message: [The app will be down from 2-4 AM for updates]     │
│  Type: [▼ System]          Action URL: [/settings]           │
│                                                               │
│  Preview:                                                     │
│  ┌────────────────────────────────────┐                      │
│  │ 🔔 System Maintenance Tonight      │                      │
│  │ The app will be down from 2-4 AM   │                      │
│  │ for updates.                       │                      │
│  └────────────────────────────────────┘                      │
│                                                               │
│  [Send to All Users (6)]  [Send to Role: ▼ Admin Only]       │
└─────────────────────────────────────────────────────────────┘
```

#### Seed Management
```
┌─────────────────────────────────────────────────────────────┐
│  🌱 Seed Data Management                                     │
│                                                               │
│  Seed Status: ✅ Seeded on 2026-05-01                        │
│                                                               │
│  Seeded Data:                                                 │
│  • 6 Users                                                    │
│  • 42 Clothing Items                                          │
│  • 8 Outfits                                                  │
│  • 5 Validation Polls                                         │
│  • 12 Feed Posts                                              │
│  • 10 Notifications                                           │
│  • 30 Calendar Events                                         │
│  • 100 Wear Events                                            │
│                                                               │
│  [🔄 Re-run Seed]  This will wipe and re-seed all data.      │
│  ⚠️ Warning: This will delete all existing data.              │
└─────────────────────────────────────────────────────────────┘
```

---

## 7. Phase 5: Advanced Admin Features

### 7.1 IP Banning System
- Track IP addresses during login
- Allow admins to ban IPs
- Block requests from banned IPs via middleware

### 7.2 Rate Limiting Dashboard
- View current rate limits per user/endpoint
- Configure rate limit thresholds
- View blocked request history

### 7.3 Backup & Export
- Database backup trigger
- Export user data (GDPR compliance)
- Export analytics as CSV/PDF

### 7.4 Webhook Management
- Configure webhooks for events (user registered, content reported)
- View webhook delivery logs
- Test webhook endpoints

### 7.5 API Key Management
- Generate API keys for third-party integrations
- Set permissions per key
- View usage statistics

### 7.6 Health Monitoring
- System health dashboard
- Database connection status
- External service status (weather API, OAuth providers)
- Hangfire job status
- AI microservice health (when deployed)

---

## 8. Technology & Dependencies

### .NET Backend (New/Updated Packages)
| Package | Purpose |
|---------|---------|
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | Already included, role support is built-in |
| — | No new packages needed for Identity roles |

### Angular Frontend (New npm Packages)
| Package | Purpose |
|---------|---------|
| @angular/cdk | Already included, used for admin tables |
| ng2-charts + chart.js | Analytics charts |
| ngx-datatable or angular-material-table | Advanced data tables with sorting, filtering |

---

## 9. Project Structure (Final)

```
Outfit-Planner/
├── src/
│   ├── OutfitPlanner.Api/
│   │   └── Controllers/
│   │       ├── AdminController.cs              ← NEW
│   │       └── AdminModerationController.cs    ← NEW (optional split)
│   │
│   ├── OutfitPlanner.Domain/
│   │   ├── Enums/
│   │   │   └── UserRole.cs                    ← NEW
│   │   └── Entities/
│   │       ├── AuditLog.cs                     ← NEW
│   │       ├── SystemSetting.cs                ← NEW
│   │       └── ContentReport.cs                ← NEW
│   │
│   ├── OutfitPlanner.Application/
│   │   └── DTOs/
│   │       └── Admin/
│   │           ├── AdminDashboardDto.cs        ← NEW
│   │           ├── UserListItemDto.cs          ← NEW
│   │           ├── ContentReportDto.cs         ← NEW
│   │           ├── SystemSettingDto.cs         ← NEW
│   │           ├── AuditLogDto.cs              ← NEW
│   │           └── BroadcastNotificationDto.cs ← NEW
│   │
│   └── outfit-planner-ui/
│       └── src/app/
│           ├── core/
│           │   ├── guards/
│           │   │   └── admin-guard.ts          ← NEW
│           │   ├── state/
│           │   │   └── admin/                  ← NEW
│           │   │       ├── admin.actions.ts
│           │   │       ├── admin.reducer.ts
│           │   │       ├── admin.effects.ts
│           │   │       └── admin.selectors.ts
│           │   └── services/
│           │       └── admin.service.ts        ← NEW
│           │
│           ├── data/
│           │   └── datasources/
│           │       └── admin.datasource.ts     ← NEW
│           │
│           ├── presentation/
│           │   ├── layouts/
│           │   │   └── admin-layout/           ← NEW
│           │   │       ├── admin-layout.component.ts
│           │   │       ├── admin-layout.component.html
│           │   │       └── admin-layout.component.scss
│           │   │
│           │   └── pages/admin/                ← NEW (all admin pages)
│           │       ├── admin-dashboard/
│           │       ├── admin-users/
│           │       ├── admin-user-detail/
│           │       ├── admin-moderation/
│           │       ├── admin-report-detail/
│           │       ├── admin-analytics/
│           │       ├── admin-settings/
│           │       ├── admin-audit-log/
│           │       ├── admin-images/
│           │       ├── admin-trending/
│           │       ├── admin-seed/
│           │       └── admin-broadcast/
│           │
│           └── domain/
│               └── entities/
│                   ├── admin.entity.ts         ← NEW
│                   └── admin-dashboard.entity.ts ← NEW
```

---

## 10. Implementation Roadmap

### Week 1: Backend — Role System + Admin API

| Day | Tasks |
|-----|-------|
| **Day 1** | Add `UserRole` enum, `AuditLog`, `SystemSetting`, `ContentReport` entities. Add `appsettings` for role config. Add EF migration for new tables. |
| **Day 2** | Update `JWTService` to include role claims in tokens. Update `AuthController` registration to assign "User" role by default. |
| **Day 3** | Update `DataSeeder` to create roles and assign them. Seed default system settings. Create `AdminController` with User Management endpoints. |
| **Day 4** | Add Content Moderation endpoints (list reports, resolve reports). Add Audit Log recording (interceptor or service). |
| **Day 5** | Add Analytics endpoints (aggregate queries). Add Settings CRUD. Add Seed/Image/Trending endpoints. Update Hangfire dashboard authorization. |

### Week 2: Frontend — Admin UI

| Day | Tasks |
|-----|-------|
| **Day 1** | Create `adminGuard`. Create `AdminLayout` component with sidebar navigation. Create NgRx admin state (actions, reducer, effects, selectors). |
| **Day 2** | Build Admin Dashboard page with overview cards and recent activity. Build Admin Users page with table, search, filter, pagination. |
| **Day 3** | Build User Detail modal (role change, disable/enable). Build Moderation page with report list. Build Report Detail modal (dismiss/warn/remove/ban). |
| **Day 4** | Build Analytics page with chart components (User growth, outfit trends, engagement). Build System Settings page with toggle switches and inputs. |
| **Day 5** | Build Audit Log page with searchable/filterable table. Build Image Manager, Trending Override, Broadcast Notification, and Seed Management pages. Add admin link to main navigation (visible only for Admin/Moderator roles). |

---

## Appendix A: Admin Navigation Structure

```
┌─ 🔧 Admin Panel ─────────────────────┐
│                                       │
│  📊 Dashboard                         │
│  👥 User Management                   │
│  🚩 Content Moderation                │
│  📈 Analytics                         │
│  ⚙️ System Settings                   │
│  📋 Audit Log                         │
│  🖼️ Image Manager                     │
│  🔥 Trending Override                 │
│  🌱 Seed Data Management              │
│  📢 Broadcast Notification            │
│                                       │
│  ──────────────────────────────────   │
│                                       │
│  [← Back to App]                      │
└───────────────────────────────────────┘
```

## Appendix B: Admin Permission Matrix

| Feature | User | Moderator | Admin |
|---------|------|-----------|-------|
| View own profile/wardrobe | ✅ | ✅ | ✅ |
| Create outfits/posts | ✅ | ✅ | ✅ |
| View admin dashboard | ❌ | ✅ | ✅ |
| View user list | ❌ | ✅ | ✅ |
| Moderate content (dismiss/warn/remove) | ❌ | ✅ | ✅ |
| Ban users | ❌ | ❌ | ✅ |
| Change user roles | ❌ | ❌ | ✅ |
| View/edit system settings | ❌ | ❌ | ✅ |
| View audit log | ❌ | ✅ | ✅ |
| Trigger seed/trending recalc | ❌ | ❌ | ✅ |
| Broadcast notifications | ❌ | ❌ | ✅ |
| Manage images | ❌ | ❌ | ✅ |
| Access Hangfire dashboard | ❌ | ❌ | ✅ |

---

*This document serves as the complete specification for adding an Admin Panel to the Outfit-Planner platform. It can be implemented incrementally, starting with the role system foundation.*