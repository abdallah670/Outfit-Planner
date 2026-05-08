# User Profile Backend Integration Plan

## Goal
Implement backend endpoints to display a user's public profile (like `Design\UserProfile.png`) including:
- Basic info (name, profile picture, join date)
- Stats (wardrobe items, outfits, total wears)
- Style profile summary
- **Followers count** and **Following count**

## Current State

### Existing Endpoints
- `GET /api/user/users/{id}/followers` - paginated list (has `totalCount`)
- `GET /api/user/users/{id}/following` - paginated list (has `totalCount`)
- `POST/DELETE /api/user/users/{id}/follow` - follow/unfollow

### Missing
- `GET /api/user/users/{id}/profile` - public profile data

---

## Solution

### 1. Create DTO

**File:** `OutfitPlanner.Application/DTOs/User/PublicUserProfileDto.cs`

```csharp
using System.Text.Json.Serialization;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.User;

/// <summary>
/// Publicly viewable user profile (non-sensitive)
/// </summary>
public class PublicUserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Stats
    public int WardrobeItemCount { get; set; }
    public int OutfitCount { get; set; }
    public int TotalWears { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }

    // Style Profile (optional summary)
    public PublicUserStyleProfileDto? StyleProfile { get; set; }
}

public class PublicUserStyleProfileDto
{
    public StylePreference Style { get; set; }
    public List<string> PreferredColors { get; set; } = new();
    public string? FitPreferences { get; set; }
    public int ComfortPriority { get; set; }
    public bool AcceptsTrends { get; set; }
}
```

---

### 2. Query & Handler

**File:** `OutfitPlanner.Application/Features/User/Requests/Queries/GetPublicUserProfileQuery.cs`

```csharp
using MediatR;

namespace OutfitPlanner.Application.Features.User.Requests.Queries;

public class GetPublicUserProfileQuery : IRequest<PublicUserProfileDto?>
{
    public string UserId { get; set; } = string.Empty;
}
```

**File:** `OutfitPlanner.Application/Features/User/Handlers/Queries/GetPublicUserProfileQueryHandler.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Identity;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class GetPublicUserProfileQueryHandler(
    UserManager<User> userManager,
    IFollowRepository followRepository) : IRequestHandler<GetPublicUserProfileQuery, PublicUserProfileDto?>
{
    public async Task<PublicUserProfileDto?> Handle(GetPublicUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null) return null;

        // Get counts (efficient queries)
        var wardrobeItemCount = await userManager.Users
            .Where(u => u.Id == request.UserId)
            .SelectMany(u => u.WardrobeItems)
            .CountAsync();

        var outfitCount = await userManager.Users
            .Where(u => u.Id == request.UserId)
            .SelectMany(u => u.Outfits)
            .CountAsync();

        var totalWears = await userManager.Users
            .Where(u => u.Id == request.UserId)
            .SelectMany(u => u.Outfits)
            .SumAsync(o => o.TimesWorn);

        var followersCount = await followRepository.GetFollowersCountAsync(request.UserId);
        var followingCount = await followRepository.GetFollowingCountAsync(request.UserId);

        // Style profile (if exists)
        PublicUserStyleProfileDto? styleProfile = null;
        if (user.StylePreference != null)
        {
            styleProfile = new PublicUserStyleProfileDto
            {
                Style = user.StylePreference.Style,
                PreferredColors = user.StylePreference.PreferredColors ?? new(),
                FitPreferences = user.StylePreference.FitPreferences,
                ComfortPriority = user.StylePreference.ComfortPriority,
                AcceptsTrends = user.StylePreference.AcceptsTrends
            };
        }

        return new PublicUserProfileDto
        {
            Id = user.Id,
            Name = user.UserName ?? string.Empty,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CreatedAt = user.CreatedAt,
            WardrobeItemCount = wardrobeItemCount,
            OutfitCount = outfitCount,
            TotalWears = totalWears,
            FollowersCount = followersCount,
            FollowingCount = followingCount,
            StyleProfile = styleProfile
        };
    }
}
```

---

### 3. Follow Repository Count Methods

**Add to `OutfitPlanner.Domain/Contracts/Repositories/IFollowRepository.cs`:**

```csharp
Task<int> GetFollowersCountAsync(string userId);
Task<int> GetFollowingCountAsync(string userId);
```

**Implement in `OutfitPlanner.Persistence/Repositories/FollowerRepository.cs`:**

```csharp
public async Task<int> GetFollowersCountAsync(string userId)
{
    return await _dbContext.Follows
        .Where(f => f.FollowingId == userId)
        .CountAsync();
}

public async Task<int> GetFollowingCountAsync(string userId)
{
    return await _dbContext.Follows
        .Where(f => f.FollowerId == userId)
        .CountAsync();
}
```

---

### 4. Controller Endpoint

Add to `OutfitPlanner.Api/Controllers/UserController.cs`:

```csharp
/// <summary>
/// Get public profile information for any user (non-sensitive)
/// </summary>
[HttpGet("users/{userId}/profile")]
[AllowAnonymous]
public async Task<ActionResult<PublicUserProfileDto?>> GetPublicProfile(string userId)
{
    try
    {
        var query = new GetPublicUserProfileQuery { UserId = userId };
        var profile = await _mediator.Send(query);

        if (profile == null)
            return NotFound(new { message = "User not found" });

        return Ok(profile);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving public profile for user {UserId}", userId);
        return StatusCode(500, new { message = "Failed to retrieve profile" });
    }
}
```

---

### 5. Database Indexes (Important for Performance)

Ensure `Follow` table has indexes:

```sql
CREATE INDEX IX_Follow_FollowingId ON Follow (FollowingId);
CREATE INDEX IX_Follow_FollowerId ON Follow (FollowerId);
```

---

## Frontend Integration

### SocialDataSource Extension

```typescript
getPublicProfile(userId: string): Observable<PublicUserProfileDto> {
  return this.http.get<PublicUserProfileDto>(
    `${environment.baseUrl}/api/user/users/${userId}/profile`
  );
}
```

Add to `SocialRepository` and `SocialUseCases`.

### ProfileComponent Usage

When route contains `userId` param (viewing another user):

```typescript
// Load public profile
this.store.dispatch(SocialActions.checkFollowStatus({ userId }));
this.socialRepository.getPublicProfile(userId).subscribe(profile => {
  this.publicProfile = profile; // display in template
});
```

Template displays:
- Name, profile picture, join date
- Stats: `{{publicProfile.WardrobeItemCount}}` items, `{{publicProfile.OutfitCount}}` outfits, `{{publicProfile.TotalWears}}` wears
- Followers: `{{publicProfile.FollowersCount}}`, Following: `{{publicProfile.FollowingCount}}`
- Style: `{{publicProfile.StyleProfile?.Style}}`, colors, comfort, etc.
- Follow/Unfollow button (bound to `isFollowing` from store)

---

## Testing Checklist

- [ ] `GET /api/user/users/{userId}/profile` returns 200 with all public fields
- [ ] Returns 404 when user doesn't exist
- [ ] Email, Preferences, LastLogin are NOT included
- [ ] Followers/Following counts are accurate
- [ ] Join date formatted correctly
- [ ] Indexes exist on Follow table for fast count queries
- [ ] Frontend displays profile without errors

---

## Implementation Order (2-3 hours)

1. Create `PublicUserProfileDto.cs`
2. Add `IFollowRepository.GetFollowersCountAsync` & implementation
3. Create `GetPublicUserProfileQuery` & Handler
4. Add endpoint to `UserController`
5. Run integration tests
6. Frontend: Add `getPublicProfile` to Social layer
7. Frontend: Update `ProfileComponent` to fetch and show public profile
8. Verify with design mockup

---

## Notes

- The existing `UserProfileDto` is for the **current user's full profile** (includes Email, Preferences). We need a separate **public** DTO for other users.
- Follow counts are already efficiently queryable via the Follow table.
- The endpoint should be `[AllowAnonymous]` because profile pages are public.
- Consider caching popular profiles if performance becomes an issue.

---

**End of User Profile Backend Integration Plan**
