# Outfit-Planner: Remaining Platform Items — Implementation Plan

> **Date:** 2026-05-23  
> **Scope:** All items from `platform-comprehensive-review.md` not covered by AI, Support, or Admin plans  
> **Covers:** Auth gaps, Production hardening, Frontend quality, Testing, Architecture improvements  
> **Source:** Sections 3.1, 6, 7, and Phase 5 from the comprehensive review

---

## Table of Contents

1. [Authentication & Authorization Gaps](#1-authentication--authorization-gaps)
2. [Backend Production Hardening](#2-backend-production-hardening)
3. [Frontend Quality & UX](#3-frontend-quality--ux)
4. [Testing Infrastructure](#4-testing-infrastructure)
5. [Architecture Improvements](#5-architecture-improvements)
6. [Implementation Roadmap](#6-implementation-roadmap)
7. [File Inventory](#7-file-inventory)

---

## 1. Authentication & Authorization Gaps

### Current State

| Feature | Status | Details |
|---------|--------|---------|
| **Role System** | ✅ Complete (Phase 1) | Admin/Moderator/User roles exist |
| **Role Claims in JWT** | ✅ Complete | JWT carries role claims |
| **Admin Guard** | ✅ Complete | Frontend admin guard exists |
| **Email Verification** | ✅ Complete| Registration doesn't require email confirmation |
| **Password Reset Flow** | ✅ Complete| "Forgot Password" not implemented |
| **Account Lockout** | ❌ Not started | No brute-force protection |

### 1.1 Email Verification Flow

**Backend:**

| Step | Detail |
|------|--------|
| **Registration** | After user registers, generate email verification token (GUID) |
| **Email Sending** | Send verification email with link: `{frontendUrl}/auth/verify-email?token={token}&userId={id}` |
| **Verification Endpoint** | `POST /api/auth/verify-email` → `{ token, userId }` → marks email as confirmed |
| **Email Service** | Uses existing `EmailService.cs` — may need HTML template support |
| **Expiration** | Token expires after 24 hours |
| **Resend** | `POST /api/auth/resend-verification` → sends new token |

**Frontend:**

| Page | Detail |
|------|--------|
| **Registration Success** | Show "Please check your email to verify your account" |
| **VerifyEmailComponent** (`/auth/verify-email`) | Reads token from query params, calls backend, shows success/error |
| **Resend Button** | On login page: "Didn't receive verification email? Resend" |
| **Unverified Restriction** | Unverified users can log in but get a banner: "Please verify your email" |

**Files to Add/Modify:**

| File | Change |
|------|--------|
| `AuthController.cs` | Add `POST /verify-email`, `POST /resend-verification` |
| `VerifyEmailCommand.cs` | New CQRS handler |
| `ResendVerificationCommand.cs` | New CQRS handler |
| `EmailService.cs` | Add HTML email template support |
| `verify-email.component.ts` | Frontend verification page |
| `verify-email.component.html` | Frontend verification template |
| `app.routes.ts` | Add `/auth/verify-email` route |
| `login.html` | Add resend verification link |

### 1.2 Password Reset Flow

**Backend:**

| Step | Detail |
|------|--------|
| **Request** | `POST /api/auth/forgot-password` → `{ email }` → generates reset token, sends email |
| **Token** | Secure random token stored in DB with expiration (1 hour) |
| **Reset** | `POST /api/auth/reset-password` → `{ email, token, newPassword }` → validates and resets |

**Frontend:**

| Page | Detail |
|------|--------|
| **ForgotPasswordComponent** (`/auth/forgot-password`) | Email input → success message |
| **ResetPasswordComponent** (`/auth/reset-password`) | Token from URL params → new password form |
| **Login Page** | Add "Forgot Password?" link |

**Files to Add/Modify:**

| File | Change |
|------|--------|
| `AuthController.cs` | Add `POST /forgot-password`, `POST /reset-password` |
| `ForgotPasswordCommand.cs` | New handler |
| `ResetPasswordCommand.cs` | New handler |
| `forgot-password.component.ts` | Frontend page |
| `forgot-password.component.html` | Frontend template |
| `reset-password.component.ts` | Frontend page (already exists as placeholder) |
| `reset-password.component.html` | Frontend template |
| `login.html` | Add "Forgot Password?" link |

### 1.3 Account Lockout

**Backend (ASP.NET Identity Built-in):**

| Step | Detail |
|------|--------|
| **Configure** | In `Program.cs`: `services.Configure<IdentityOptions>(options => { options.Lockout... })` |
| **Max Attempts** | 5 failed login attempts before lockout |
| **Lockout Duration** | 15 minutes default (configurable) |
| **Auto-Unlock** | Built-in — unlocks after duration expires |
| **Admin Unlock** | `POST /api/admin/unlock-account/{userId}` (covered in admin plan) |

**Files to Modify:**

| File | Change |
|------|--------|
| `Program.cs` | Add Identity lockout configuration |
| `appsettings.json` | Add `Lockout.MaxFailedAttempts`, `Lockout.DefaultLockoutMinutes` |

---

## 2. Backend Production Hardening

### 2.1 Rate Limiting

**Implementation:** Use ASP.NET Core 7+ built-in rate limiting middleware.

```csharp
// In Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Api", config =>
    {
        config.PermitLimit = 100;           // 100 requests
        config.Window = TimeSpan.FromMinutes(1);  // per minute
        config.QueueLimit = 0;
    });
    
    options.AddFixedWindowLimiter("Auth", config =>
    {
        config.PermitLimit = 5;             // 5 attempts
        config.Window = TimeSpan.FromMinutes(15);  // per 15 minutes
        config.QueueLimit = 0;
    });
});

app.UseRateLimiter();
```

**Endpoint Policies:**

| Policy | Endpoints | Limit |
|--------|-----------|-------|
| `Api` | All API endpoints | 100 req/min |
| `Auth` | Login, Register, ForgotPassword | 5 req/15min |
| `Feed` | Social feed, trending | 200 req/min |

**Files to Modify:**

| File | Change |
|------|--------|
| `Program.cs` | Add rate limiter configuration |
| `appsettings.json` | Add `RateLimiting` settings section |

### 2.2 Request Validation — FluentValidation

**Implementation:** Add FluentValidation globally via MediatR pipeline behavior.

```csharp
// Install: dotnet add package FluentValidation.DependencyInjectionExtensions

// In DependencyInjection.cs
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any()) return await next();
        
        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
        
        if (failures.Count != 0)
            throw new ValidationException(failures);
        
        return await next();
    }
}
```

**Example Validator:**

```csharp
public class CreateOutfitCommandValidator : AbstractValidator<CreateOutfitCommand>
{
    public CreateOutfitCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Occasion).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Outfit must have at least one item");
    }
}
```

**Files to Add:**

| File | Purpose |
|------|---------|
| `ValidationBehavior.cs` | MediatR pipeline behavior |
| `Validators/` directory | One validator per command (12-15 validators for core commands) |

### 2.3 Caching Layer

**Implementation:** Use `IMemoryCache` with configurable TTL.

```csharp
// In DependencyInjection.cs
services.AddMemoryCache();

// CacheService.cs
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheSettings _settings;
    
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null)
    {
        if (_cache.TryGetValue(key, out T? cached)) return cached;
        
        var result = await factory();
        _cache.Set(key, result, ttl ?? TimeSpan.FromMinutes(_settings.DefaultTtlMinutes));
        return result;
    }
    
    public void Remove(string key) => _cache.Remove(key);
    public void RemoveByPattern(string pattern) { /* iterate keys by pattern */ }
}
```

**Caching Plan:**

| Data | Cache Key | TTL | Invalidate On |
|------|-----------|-----|---------------|
| Trending outfits | `trending:outfits` | 15 min | New outfit created |
| Feed posts | `feed:user:{userId}:page:{page}` | 5 min | New post/comment |
| Weather | `weather:{city}` | 30 min | — |
| Wardrobe | `wardrobe:user:{userId}` | 5 min | Item added/deleted |

**Files to Add:**

| File | Purpose |
|------|---------|
| `Services/CacheService.cs` | Cache service implementation |
| `Contracts/ICacheService.cs` | Cache service interface |
| `Program.cs` (modify) | Register cache service |



**Implementation:** Use Hangfire recurring jobs for maintenance tasks.

```csharp
// In Program.cs
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AdminOnlyAuthorizationFilter() }  // Already planned in admin
});

// RecurringJobScheduler.cs
public static class RecurringJobScheduler
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<ITrendingService>(
            "calculate-trending",
            x => x.CalculateTrendingAsync(),
            "*/15 * * * *");  // Every 15 minutes
        
        RecurringJob.AddOrUpdate<ICleanupService>(
            "cleanup-stale-data",
            x => x.CleanupAsync(),
            "0 3 * * *");  // Daily at 3 AM
        
        RecurringJob.AddOrUpdate<IEmailService>(
            "send-digest-emails",
            x => x.SendWeeklyDigestAsync(),
            "0 8 * * 1");  // Weekly on Monday
    }
}
```

**Files to Add:**

| File | Purpose |
|------|---------|
| `BackgroundJobs/RecurringJobScheduler.cs` | Job registration |
| `BackgroundJobs/TrendingCalculationJob.cs` | Trending recalc job |
| `BackgroundJobs/DataCleanupJob.cs` | Stale data cleanup |
| `BackgroundJobs/DigestEmailJob.cs` | Weekly digest emails |

### 2.7 API Versioning

```csharp
// In Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// On controllers:
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
```

**Files to Modify:**

| File | Change |
|------|--------|
| `Program.cs` | Add API versioning config |
| All controllers | Add `[ApiVersion("1")]` and update route template |

### 2.8 Swagger Production Configuration

```csharp
// In Program.cs
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // In production, only expose if explicitly configured
    if (configuration.GetValue<bool>("Swagger:Enabled"))
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.RoutePrefix = "api/docs");
    }
}
```

**Files to Modify:**

| File | Change |
|------|--------|
| `Program.cs` | Add environment-specific Swagger config |
| `appsettings.json` | Add `Swagger.Enabled` flag |

### 2.9 Health Check Endpoint

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddCheck<CacheHealthCheck>("cache")
    .AddCheck<EmailHealthCheck>("email");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration
            })
        });
    }
});
```

**Files to Modify:**

| File | Change |
|------|--------|
| `Program.cs` | Add health checks |
| `HealthChecks/CacheHealthCheck.cs` | Cache connectivity check |
| `HealthChecks/EmailHealthCheck.cs` | Email service check |

---

## 3. Frontend Quality & UX

### 3.1 PWA Support

**Implementation:** Use Angular PWA schematics.

```bash
ng add @angular/pwa
```

**Creates/Modifies:**

| File | Purpose |
|------|---------|
| `ngsw-config.json` | Service worker configuration |
| `manifest.webmanifest` | PWA manifest for install prompt |
| `src/main.ts` | Register service worker |
| `src/index.html` | Add meta tags, theme-color |
| `assets/icons/` | App icons (192x192, 512x512) |

**Caching Strategy:**

| Asset | Strategy |
|-------|----------|
| API calls (wardrobe, outfits) | `NetworkFirst` with 60s timeout |
| Images | `CacheFirst` with 50-item LRU cache |
| App shell (HTML/CSS/JS) | `StaleWhileRevalidate` |
| Fonts/icons | `CacheFirst` |

### 3.2 i18n/Localization

**Implementation:** Use `@angular/localize` package.

```bash
ng add @angular/localize
```

**Setup:**

| Step | Detail |
|------|--------|
| **Install** | `ng add @angular/localize` |
| **Configure** | `angular.json` → add `"localize"` option |
| **Locales** | Start with English (`en`) + Arabic (`ar`) |
| **Extract** | `ng extract-i18n` → generates `messages.xlf` |
| **Translate** | Create `messages.ar.xlf` for Arabic |

**Component Usage:**

```typescript
// In component
import { TranslateService } from '@ngx-translate/core';

// Template
// <h1 i18n="Welcome headline">Welcome to Outfit Planner</h1>
// <p i18n="Description text">Plan your outfits with AI assistance</p>
```

| File | Purpose |
|------|---------|
| `messages.xlf` | English translation source |
| `messages.ar.xlf` | Arabic translations |
| `angular.json` | Localize config |
| `app.config.ts` | Register locale |

### 3.3 Dark Mode

**Implementation:** CSS custom properties + Angular service.

```scss
// styles.scss
:root {
  --bg-primary: #ffffff;
  --bg-secondary: #f5f5f5;
  --text-primary: #1a1a1a;
  --text-secondary: #666666;
  --border-color: #e0e0e0;
  --card-bg: #ffffff;
}

[data-theme="dark"] {
  --bg-primary: #1a1a2e;
  --bg-secondary: #16213e;
  --text-primary: #e0e0e0;
  --text-secondary: #a0a0a0;
  --border-color: #2a2a4a;
  --card-bg: #1f1f3d;
}
```

**Files to Add/Modify:**

| File | Purpose |
|------|---------|
| `services/theme.service.ts` | Theme toggle service |
| `components/theme-toggle.component.ts` | Dark mode toggle button |
| `styles.scss` | CSS custom properties for theming |
| `app.config.ts` | Provide ThemeService |

### 3.4 Accessibility (a11y)

**Checklist:**

| Item | Implementation |
|------|----------------|
| **ARIA Labels** | Add `[attr.aria-label]` to all interactive elements |
| **Keyboard Navigation** | Ensure all actions work with Tab + Enter |
| **Focus Management** | Use `FocusTrap` in modals, manage focus on route change |
| **Screen Reader** | Add `aria-live` regions for dynamic updates |
| **Color Contrast** | Verify all text meets WCAG AA (4.5:1 ratio) |
| **Skip Links** | Add "Skip to main content" link |
| **Form Labels** | Ensure all inputs have associated `<label>` elements |
| **Alt Text** | All images must have descriptive `alt` attributes |

**Tools:**

| Tool | Purpose |
|------|---------|
| `@angular/cdk/a11y` | Focus monitoring, LiveAnnouncer |
| Chrome Lighthouse | Automated a11y audit |
| axe DevTools | In-depth a11y testing |

### 3.5 Loading Skeletons

**Implementation:** Replace all "Loading..." text with skeleton placeholders.

```html
<!-- Before -->
<div *ngIf="isLoading">Loading...</div>

<!-- After -->
<div *ngIf="isLoading" class="skeleton">
  <div class="skeleton__card" *ngFor="let _ of [1,2,3]">
    <div class="skeleton__image"></div>
    <div class="skeleton__line skeleton__line--short"></div>
    <div class="skeleton__line skeleton__line--medium"></div>
  </div>
</div>
```

```scss
.skeleton {
  &__card { padding: 16px; border-radius: 8px; }
  &__image { width: 100%; height: 200px; background: #e0e0e0; border-radius: 8px; margin-bottom: 12px; }
  &__line { height: 14px; background: #e0e0e0; border-radius: 4px; margin-bottom: 8px; }
  &__line--short { width: 40%; }
  &__line--medium { width: 70%; }
  
  // Animation
  background: linear-gradient(90deg, #e0e0e0 25%, #f0f0f0 50%, #e0e0e0 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
}

@keyframes shimmer {
  0% { background-position: -200% 0; }
  100% { background-position: 200% 0; }
}
```

**Affected Components (priority order):**
1. WardrobeDashboardComponent
2. CommunityFeedComponent
3. OutfitsDashboardComponent
4. SocialComponent (hub)
5. TrendingOutfitsComponent
6. MyPollsComponent
7. NotificationsCenterComponent

### 3.6 Error Boundaries

**Implementation:** Use Angular `ErrorHandler` for global error catching.

```typescript
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  constructor(private toast: ToastService) {}
  
  handleError(error: any): void {
    // Log to server
    console.error('Unhandled error:', error);
    
    // Show user-friendly toast
    this.toast.show('Something went wrong. Please try again.', 'error');
    
    // Don't re-throw — prevents full page crash
  }
}

// In app.config.ts
providers: [
  { provide: ErrorHandler, useClass: GlobalErrorHandler }
]
```

### 3.7 Performance Improvements

| Item | Implementation | Effort |
|------|----------------|--------|
| **TrackBy in ngFor** | Add `trackBy` functions to all `*ngFor` loops | 🟢 Easy |
| **Debounced Search** | Use `debounceTime(300)` in global search input | 🟢 Easy |
| **CDK Virtual Scroll** | Use `<cdk-virtual-scroll-viewport>` for wardrobe lists | 🟡 Medium |
| **Image Lazy Loading** | Add `loading="lazy"` on all `<img>` tags | 🟢 Easy |
| **Blur-Up Images** | Show blurred low-res placeholder while full image loads | 🟡 Medium |
| **Responsive Images** | Use `srcset` for different screen sizes | 🟡 Medium |

### 3.8 NgRx Improvements

| Item | Implementation |
|------|----------------|
| **ngrx/router-store** | Connect router to NgRx DevTools for route-based debugging |
| **State Consolidation** | Merge `outfit-posts` state into `feed` state (duplicate concern) |
| **Signals Standardization** | Use Angular Signals for UI state, Observables for HTTP |
| **State Persistence** | Save auth token + user preferences to localStorage via meta-reducer |

---

## 4. Testing Infrastructure

### 4.1 Backend Unit Tests

**Project:** `tests/OutfitPlanner.Application.UnitTests/`

**Priority Test Targets (order of importance):**

| Priority | Handler/Service | Test Cases |
|----------|----------------|------------|
| 🔴 Critical | `Auth/LoginHandler` | Valid login, invalid password, locked account, unverified email |
| 🔴 Critical | `Auth/RegisterHandler` | Valid registration, duplicate email, weak password |
| 🔴 Critical | `Outfits/CreateOutfitHandler` | Valid outfit, empty items, duplicate name |
| 🟡 High | `Feed/GetUserFeedHandler` | Pagination, cursor-based navigation, empty feed |
| 🟡 High | `Polls/VoteHandler` | Valid vote, duplicate vote on closed poll |
| 🟡 High | `Polls/CreatePollHandler` | Valid poll, no options, expired date |
| 🟡 Medium | `Trending/CalculateTrendingHandler` | Trending calculation, empty data |
| 🟡 Medium | `User/FollowHandler` | Follow, unfollow, self-follow, already following |

**Setup:**

```bash
# Already exists — just needs test files
cd tests/OutfitPlanner.Application.UnitTests
dotnet add package xunit
dotnet add package Moq
dotnet add package FluentAssertions
```

### 4.2 Backend Integration Tests

**Project:** `tests/OutfitPlanner.Application.IntegrationTests/`

**Setup:**

```bash
cd tests/OutfitPlanner.Application.IntegrationTests
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package xunit
```

**Test Scenarios:**

| Test | What It Validates |
|------|-------------------|
| `AuthFlowTests` | Register → Login → Refresh → Logout |
| `WardrobeCRUDTests` | Create → Get → Update → Delete clothing item |
| `OutfitCRUDTests` | Create outfit with items → Get → Update → Delete |
| `FeedTests` | Create post → Add comment → React → Delete |
| `PollTests` | Create poll → Vote → Close → Comments |
| `AdminTests` | Admin-only endpoints require Admin role |

### 4.3 Frontend Unit Tests

**Setup:**

```bash
# If using Jest
npm install --save-dev jest @angular-builders/jest

# If using Jasmine (already configured with Angular)
ng test
```

**Priority Test Targets:**

| Component | Test Cases |
|-----------|------------|
| **Auth components** | Login form validation, register validation, token storage |
| **Wardrobe components** | Item list rendering, filter behavior, add/edit form validation |
| **Outfit builder** | Item selection, combination logic, save flow |
| **Feed components** | Post rendering, comment input, reaction toggle |
| **NgRx state modules** | Reducer tests, selector tests, effect tests (using marble testing) |

---

## 5. Architecture Improvements

### 5.1 CQRS Separation Audit

**Current Issue:** Some MediatR handlers mix query and command logic.

**Files to Audit:**

| Handler | Issue | Fix |
|---------|-------|-----|
| `GetTodaysPickHandler` | Reads data + may have side effects | Ensure read-only |
| `CreateOutfitHandler` | Creates outfit + may query data | Separate into two handlers if needed |

**Rule:** Query handlers should never call `_repository.UpdateAsync()` or `_repository.AddAsync()`. Command handlers should never return cached data.

### 5.2 Result Pattern

**Implementation:** Replace exception throwing for business logic failures with `Result<T>`.

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}

// Usage in handler
public async Task<Result<OutfitDto>> Handle(CreateOutfitCommand request, CancellationToken ct)
{
    if (string.IsNullOrEmpty(request.Name))
        return Result<OutfitDto>.Failure("Outfit name is required");
    
    var outfit = await _outfitRepo.AddAsync(mapped);
    return Result<OutfitDto>.Success(_mapper.Map<OutfitDto>(outfit));
}
```

### 5.3 Options Pattern

**Implementation:** Use strongly-typed settings classes with `IOptions<T>`.

Current settings files exist at `src/OutfitPlanner.Application/Models/`:
- ✅ `AISettings.cs`
- ✅ `CacheSettings.cs`
- ✅ `BackupSettings.cs`
- ✅ `MaintenanceSettings.cs`
- ✅ `ServiceManagementSettings.cs`
- ✅ `UserActivitySettings.cs`
- ✅ `BackgroundRemovalSettings.cs`

**To Verify:** All these are registered in `Program.cs` via `services.Configure<T>()`.

### 5.4 Frontend Architecture Standardization

| Area | Current | Target |
|------|---------|--------|
| **DI Pattern** | Mix of constructor injection + `inject()` | Use `inject()` consistently in newer components |
| **State Mgmt** | Overly granular (outfit-posts separate from feed) | Merge `outfit-posts` state into `feed` |
| **Reactivity** | Mix of signals + observables | Signals for UI state, Observables for HTTP |
| **Router State** | Not connected to NgRx | Add `ngrx/router-store` |

---

## 6. Implementation Roadmap

### Phase A: Auth Gaps (Week 1)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Implement email verification flow (backend: token generation, email sending, verify endpoint) | 🟡 Medium |
| **Day 2** | Implement forgot password + reset password flow (backend) | 🟡 Medium |
| **Day 3** | Create VerifyEmailComponent, ForgotPasswordComponent frontend pages | 🟡 Medium |
| **Day 4** | Wire up ResetPasswordComponent. Add lockout configuration in Program.cs | 🟡 Medium |
| **Day 5** | End-to-end test all auth flows. Add resend verification to login page | 🟡 Medium |

### Phase B: Backend Hardening (Weeks 2-3)

| Week | Day | Tasks | Effort |
|------|-----|-------|--------|
| **W2** | **Day 1** | Add rate limiting middleware with policies for API, Auth, Feed | 🟡 Medium |
| **W2** | **Day 2** | Install FluentValidation, create ValidationBehavior, add validators for top 5 commands | 🟡 Medium |
| **W2** | **Day 3** | Add IMemoryCache wrapper service. Cache trending, feed, and wardrobe queries | 🟡 Medium |
| **W2** | **Day 4** | Install Serilog, configure console + file sinks, enrich with correlation IDs | 🟡 Medium |
| **W2** | **Day 5** | Add ExceptionHandlingMiddleware. Add health check endpoint | 🟡 Medium |
| **W3** | **Day 1** | Configure Hangfire recurring jobs (trending calc, data cleanup, email digests) | 🟡 Medium |
| **W3** | **Day 2** | Add API versioning to controllers | 🟡 Medium |
| **W3** | **Day 3** | Configure Swagger for production (disable in prod unless explicitly enabled) | 🟢 Easy |
| **W3** | **Day 4** | Audit CQRS handlers for separation concerns. Fix any mixed handlers | 🟡 Medium |
| **W3** | **Day 5** | Ensure all models use Options Pattern with `IOptions<T>` injection | 🟢 Easy |

### Phase C: Frontend Quality (Weeks 4-5)

| Week | Day | Tasks | Effort |
|------|-----|-------|--------|
| **W4** | **Day 1** | Add PWA support via `ng add @angular/pwa`. Configure service worker caching | 🟡 Medium |
| **W4** | **Day 2** | Implement dark mode: CSS custom properties, ThemeService, toggle component | 🟡 Medium |
| **W4** | **Day 3** | Add i18n setup via `@angular/localize`. Extract messages, create Arabic translation stubs | 🟡 Medium |
| **W4** | **Day 4** | Add GlobalErrorHandler. Implement loading skeletons on top 5 pages | 🟡 Medium |
| **W4** | **Day 5** | Add TrackBy to all ngFor loops. Add debounceTime(300) to search input | 🟢 Easy |
| **W5** | **Day 1** | Add accessibility: ARIA labels, keyboard navigation, focus management | 🟡 Medium |
| **W5** | **Day 2** | Add CDK Virtual Scroll to wardrobe list and feed list. Add image lazy loading | 🟡 Medium |
| **W5** | **Day 3** | Connect ngrx/router-store. Merge outfit-posts state into feed state | 🟡 Medium |
| **W5** | **Day 4** | Standardize Signals vs Observables usage across components | 🟡 Medium |
| **W5** | **Day 5** | Add auth state persistence. Add responsive design fixes | 🟡 Medium |

### Phase D: Testing (Week 6)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Set up test infrastructure (Moq, xUnit, FluentAssertions, WebApplicationFactory) | 🟢 Easy |
| **Day 2** | Write backend unit tests for Auth handlers (Login, Register, RefreshToken) | 🟡 Medium |
| **Day 3** | Write backend unit tests for Outfit and Feed handlers | 🟡 Medium |
| **Day 4** | Write backend integration tests for critical user flows | 🔴 Hard |
| **Day 5** | Set up frontend component tests for auth components + wardrobe components | 🟡 Medium |

---

## 7. File Inventory

### New Files to Create

| Layer | Count | Files |
|-------|-------|-------|
| **Backend Middleware** | 2 | `ExceptionHandlingMiddleware.cs`, `ValidationBehavior.cs` |
| **Backend Services** | 2 | `CacheService.cs`, `RecurringJobScheduler.cs` |
| **Backend Background Jobs** | 3 | `TrendingCalculationJob.cs`, `DataCleanupJob.cs`, `DigestEmailJob.cs` |
| **Backend Health Checks** | 2 | `CacheHealthCheck.cs`, `EmailHealthCheck.cs` |
| **Backend Validators** | 8 | Validators for CreateUser, CreateOutfit, CreatePost, CreatePoll, Vote, AddComment, Login, Register |
| **Backend Auth Commands** | 4 | `VerifyEmailCommand.cs`, `ResendVerificationCommand.cs`, `ForgotPasswordCommand.cs`, `ResetPasswordCommand.cs` |
| **Frontend Pages** | 3 | `verify-email.component`, `forgot-password.component`, (reset-password already exists) |
| **Frontend Services** | 2 | `ThemeService`, `GlobalErrorHandler` |
| **Frontend Components** | 2 | `ThemeToggleComponent`, Loading skeleton components |
| **Frontend Config** | 2 | `ngsw-config.json`, `manifest.webmanifest` |
| **Test Files** | 15+ | Unit tests for critical handlers, integration test scenarios |

### Existing Files to Modify

| File | Changes |
|------|---------|
| `src/OutfitPlanner.Api/Program.cs` | Add rate limiting, Serilog, health checks, API versioning, Hangfire, CORS hardening |
| `src/OutfitPlanner.Api/Controllers/AuthController.cs` | Add verify email, forgot password, reset password endpoints |
| `src/OutfitPlanner.Infrastructure/DependencyInjection.cs` | Register cache service, validators, background jobs |
| `src/OutfitPlanner.Infrastructure/Services/EmailService.cs` | Add HTML template support for verification/reset emails |
| `src/outfit-planner-ui/src/app/app.config.ts` | Register ErrorHandler, ThemeService, router-store |
| `src/outfit-planner-ui/src/app/presentation/pages/auth/login/login.html` | Add "Forgot Password?" link, resend verification |
| `src/outfit-planner-ui/src/app/presentation/pages/auth/register/register.html` | Show verification required message |
| `src/outfit-planner-ui/src/styles.scss` | Add CSS custom properties for theming, skeleton animations |
| `src/outfit-planner-ui/angular.json` | Add localize config, PWA assets |
| `src/OutfitPlanner.Api/appsettings.json` | Add RateLimiting, Swagger, Serilog, Lockout sections |

---

> **Summary: 4 Phases, 6 Weeks**
> - **Phase A** (Week 1): Auth gaps — Email verification, password reset, lockout
> - **Phase B** (Weeks 2-3): Backend hardening — Rate limiting, FluentValidation, caching, Serilog, Hangfire, health checks
> - **Phase C** (Weeks 4-5): Frontend quality — PWA, dark mode, i18n, loading skeletons, a11y, performance
> - **Phase D** (Week 6): Testing — Backend unit/integration tests, frontend component tests