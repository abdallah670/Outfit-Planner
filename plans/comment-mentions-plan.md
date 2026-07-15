# Comment @Mentions Implementation Plan

## Goal
Allow users to **mention** other users in post comments. Each comment stores a list of **`MentionedUserDto`** (userId + user name + avatar), so mentions are persisted and can be displayed by **full name** and linked to the user's **profile** without extra lookups.

---

## Key decisions
- Persist `List<MentionedUserDto> MentionedUsers` on `PostComment`, serialized into **one JSON column`. No separate mention table/join.
- `MentionedUserDto` carries `UserId`, `UserName` (the **full/display name** = `User.Name`, not the `@handle`), and `ProfilePictureUrl`. The frontend renders directly from this list → no `/profile` lookup needed for display.
- The comment `Content` still contains the `@Full Name` token so it reads naturally; `MentionedUsers` is the authoritative list for notifications + rendering.
- Replying **auto-mentions the parent comment's author** as the first mention (enforced server-side).

---

## Backend Changes

### 1. New DTO `MentionedUserDto`
File: `src/OutfitPlanner.Application/DTOs/Feed/MentionedUserDto.cs`
```csharp
namespace OutfitPlanner.Application.DTOs.Feed;

public class MentionedUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;   // full/display name (User.Name)
}
```

### 2. Add the list property to `PostComment`
File: `src/OutfitPlanner.Domain/Entities/PostComment.cs`
```csharp
public List<MentionedUserDto> MentionedUsers { get; set; } = new();
```

### 3. Store it as one JSON column (EF config)
EF Core 10 → native `.ToJson()`.
File: `src/OutfitPlanner.Persistence/Configurations/FeedPostConfiguration.cs` (in `PostCommentConfiguration`, ~line 85)
```csharp
builder.Property(x => x.MentionedUsers)
    .HasColumnType("nvarchar(max)")
    .ToJson();
```
(Add `using OutfitPlanner.Application.DTOs.Feed;` to the configuration file.)

### 4. DTOs / command
- `CreateCommentDto.cs`: `public List<MentionedUserDto> MentionedUsers { get; set; } = new();` (optional).
- `PostCommentDto.cs`: `public List<MentionedUserDto> MentionedUsers { get; set; } = new();`.
- `AddPostCommentCommand.cs`: `public List<MentionedUserDto> MentionedUsers { get; set; } = new();`.
- **Follower name fix (critical):** `FollowerDto` (`GetFollowersQuery.cs:23`) and `FollowingDto` (`GetFollowingQuery.cs`) expose `UserName` = `@handle`. Add `public string FullName { get; set; } = string.Empty;` mapped from `f.Follower?.Name` in `GetFollowersQueryHandler.cs:35` (and the following handler). The picker uses `FullName` to build the mention.

### 5. Mapping & validation
- `MappingProfile.cs:282` (`PostComment → PostCommentDto`): map `s.MentionedUsers` (it's already a `List<MentionedUserDto>`, so direct assignment).
- `AddPostCommentCommandValidator.cs`: if provided, cap count (e.g. ≤ 50) and each `UserId` non-empty.

### 6. Handler logic — `AddPostCommentCommandHandler.Handle`
After building the comment (`AddPostCommentCommandHandler.cs:48`):
- Build a deduplicated dictionary keyed by `UserId` from `request.MentionedUsers`.
- **Reply rule ("first mention = parent comment user"):** if `request.ParentCommentId.HasValue`, load `parentComment` (already at line 61) and **force-add the parent author** to the set (look up their name via `_unitOfWork.Repository<User>().GetByIdAsync(parentComment.UserId)` to get `Name` + `ProfilePictureUrl`), even if the client omitted them.
- Remove `request.UserId` (author never mentions self).
- Assign `comment.MentionedUsers = dict.Values.ToList();` before `AddAsync`.
- **Notifications:** load commenter name once (`_unitOfWork.Repository<User>().GetByIdAsync(request.UserId)`), then for each mentioned user (skip self/dupes) send `CreateNotificationCommand`:
  ```csharp
  Type = NotificationType.Social,
  Title = "New mention",
  Message = $"{commenterName} mentioned you in a comment",
  ActionUrl = post.PostType == PostType.Outfit ? $"/social/posts/{post.Id}" : $"/social/polls/{post.Id}"
  ```
  (Optional: skip the mention notification when the mentioned user is also the post owner.)
- Keep the existing post-owner notification and SignalR comment-count update unchanged.

### 7. Migration
- `dotnet ef migrations add AddCommentMentionedUsers` and apply.

---

## Frontend Changes

### 8. Models & data layer
- `domain/entities/feed.entity.ts`: add
  ```ts
  export interface MentionedUser { userId: string; userName: string; profilePictureUrl?: string; }
  ```
  and `mentionedUsers?: MentionedUser[]` to `PostComment`.
- `domain/entities/follow.entity.ts` `Follower`/`Following`: add `fullName?: string`.
- `feed.datasource.ts:120` `addComment(postId, content, parentCommentId?, mentionedUsers?)` → send `mentionedUsers` in body; map `MentionedUser` from `comment.mentionedUsers`.
- Propagate through `feed.repository.ts`, `feed.repository.impl.ts`, `FeedUseCases.addComment(...)`.
- `user.datasource.ts` `getFollowers` must map `fullName` into the `Follower` object.

### 9. Comments-modal: mention tracking
File: `comments-modal.component.ts`
- Inject `FollowUseCases`.
- Component state: `mentionedUsers = new Map<string, MentionedUser>()` (keyed by userId).
- **Reply auto-mention:** in `startReplying(comment)` (line 187) add the parent author:
  `this.mentionedUsers.set(comment.userId, { userId: comment.userId, userName: comment.userName, profilePictureUrl: comment.userAvatarUrl });`
- **@ dropdown:** on `input`, detect the trailing `@token`; when present call `followUseCases.getFollowers(currentUserId, undefined, 20, token)` → `mentionResults`.
- Selecting a follower: insert `@<follower.fullName> ` at the caret (**use `fullName`, not `userName`**), and add `this.mentionedUsers.set(follower.userId, { userId: follower.userId, userName: follower.fullName, profilePictureUrl: follower.userAvatarUrl });`.
- On `submitComment()` / `submitReply()`: pass `Array.from(this.mentionedUsers.values())` to `addComment(...)`. Reset after submit.

### 10. Comments-modal: display full name + profile link
- Rendering already works via `parseMentions()` + template (`comments-modal.component.html:51`) showing the `@Full Name` text and linking via `goToUserProfile(segment.userId)`.
- Use the authoritative `comment.mentionedUsers` for guaranteed-correct names/links: seed the client `userMap` inside `parseMentions` from `comment.mentionedUsers` (`userId → userName`) in addition to the comment tree. This makes every stored mention render with its full name and link to `/profile/:userId`, even off-tree.
- Alternatively, render stored `MentionedUser` entries as dedicated clickable chips above/within the comment. Keep the existing `@name` inline rendering as the primary approach.

### 11. HTML
File: `comments-modal.component.html`
- Add the follower mention dropdown below the new-comment textarea (line 126) and the reply textarea (line 83), shown when the `@` token is active. Each item: avatar + full name; `(click)` inserts the mention.

---

## Acceptance Criteria
- [ ] A comment persists a `MentionedUsers` JSON list of `MentionedUserDto` (userId + name + avatar) (verified in DB + `PostCommentDto.MentionedUsers`).
- [ ] Replying auto-mentions the parent comment's author (first mention) and notifies them, even if the user edits the text — enforced server-side.
- [ ] Typing `@` shows a follower picker; selecting records the userId + name and inserts `@Full Name`.
- [ ] Each mentioned user (not the author, no duplicates) receives a "mentioned you" notification.
- [ ] On display, every mention shows the user's full name and links to their profile.
- [ ] `dotnet build` + `ng build` succeed; post-owner notification and SignalR comment-count update unchanged.

## Suggested Order
1. Backend: `MentionedUserDto` + property + JSON config + migration (steps 1–3, 7)
2. Backend: command/DTOs/mapping/validation + `FullName` on follower DTOs (step 4–5)
3. Backend: handler + notifications (step 6)
4. Frontend: models + data layer (step 8)
5. Frontend: modal mention tracking + follower dropdown + display (steps 9–11)
6. Build, migrate, manual test on a post with replies.
