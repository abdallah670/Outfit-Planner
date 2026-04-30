# Outfit Planner – Social Module Backend Integration Plan

> **Commit reviewed:** `4417d90` – *refactor(feed): migrate to cursor pagination and remove post endpoints*  
> **Goal:** Connect the existing frontend **Social** module to the newly built backend APIs (Feed, Polls, Trending, Follow) without renaming any frontend "social" artifacts.

---

## 1. Current State

### 1.1 Backend APIs (already implemented)

| Controller | Route | Method | Description |
|---|---|---|---|
| `FeedController` | `/api/feed` | `GET` | Cursor-paginated feed posts (`FeedPostDto`) |
| | `/api/feed/{id}` | `GET` | Single feed post |
| | `/api/feed/{id}` | `DELETE` | Delete post |
| | `/api/feed/{id}/heart` | `POST` | Add heart reaction |
| | `/api/feed/{id}/heart` | `DELETE` | Remove heart reaction |
| | `/api/feed/{id}/comments` | `GET` | Cursor-paginated comments |
| | `/api/feed/{id}/comments` | `POST` | Add comment |
| | `/api/feed/comments/{commentId}` | `DELETE` | Delete comment |
| `OutfitPostsController` | `/api/outfitposts` | `POST` | Create outfit post |
| | `/api/outfitposts/{id}` | `GET` | Get outfit post |
| | `/api/outfitposts/{id}` | `PUT` | Update outfit post |
| | `/api/outfitposts/{id}` | `DELETE` | Delete outfit post |
| `PollsController` | `/api/polls` | `GET` | Get my polls |
| | `/api/polls/{id}` | `GET` | Get poll by ID |
| | `/api/polls` | `POST` | Create poll |
| | `/api/polls/{id}` | `PUT` | Update poll |
| | `/api/polls/{id}` | `DELETE` | Delete poll |
| | `/api/polls/{id}/vote` | `POST` | Vote on poll |
| | `/api/polls/{id}/close` | `POST` | Close poll |
| | `/api/polls/recent-poll` | `GET` | Most voted active poll (+ comments) – allows anonymous |
| `TrendingController` | `/api/trending/outfits` | `GET` | Trending outfits (offset pagination) |
| | `/api/trending/calculate` | `POST` | Trigger recalculation |
| `UserController` | `/api/user/users/{id}/followers` | `GET` | Followers (cursor pagination) |
| | `/api/user/users/{id}/following` | `GET` | Following (cursor pagination) |
| | `/api/user/users/{id}/follow` | `POST` | Follow user |
| | `/api/user/users/{id}/follow` | `DELETE` | Unfollow user |
| | `/api/user/users/{id}/is-following` | `GET` | Check follow status |

### 1.2 Frontend Architecture (already implemented)

The frontend **already has** a fully wired Clean Architecture + NgRx setup for social features:

#### Feed Layer (NEW – already scaffolded)
- `domain/entities/feed-post.entity.ts` – `FeedPost`, `PostType`, `Visibility`, `CursorPagedResult`
- `domain/entities/post-comment.entity.ts` – `PostComment`, `PostCommentsResponse`
- `domain/repositories/feed.repository.ts` – `FeedRepository` interface
- `data/datasources/feed.datasource.ts` – `FeedDataSource` → calls `/Feed` (needs URL fix)
- `data/repositories/feed.repository.impl.ts` – `FeedRepositoryImpl`
- `domain/usecases/feed.usecases.ts` – `FeedUseCases`
- `core/state/feed/feed.actions.ts` – `FeedActions` (load, react, comment, delete)
- `core/state/feed/feed.reducer.ts` – `feedFeature` with cursor pagination support
- `core/state/feed/feed.effects.ts` – `FeedEffects` wired to `FeedUseCases`
- **Registered in** `app.config.ts` ✅

#### Social Layer (legacy name kept)
- `domain/entities/validation-poll.entity.ts` – `ValidationPoll`, `PollOption`, `CastVoteRequest`, etc.
- `domain/entities/social-engagement.entity.ts` – `TrendingOutfit`, `VoteComment`, etc.
- `domain/repositories/social.repository.ts` – `SocialRepository` interface
- `data/datasources/social.datasource.ts` – `SocialDataSource`
- `data/repositories/social.repository.impl.ts` – `SocialRepositoryImpl`
- `domain/usecases/social.usecases.ts` – `SocialUseCases`
- `core/state/social/social.actions.ts` – `SocialActions`
- `core/state/social/social.reducer.ts` – `socialFeature`
- `core/state/social/social.effects.ts` – `SocialEffects`
- `core/state/social/social.selectors.ts` – selectors
- **Registered in** `app.config.ts` ✅

### 1.3 Pages & Components

| Page | File | Current Data Source |
|---|---|---|
| `/social` | `pages/social/social.component.ts` | `SocialActions.loadPolls()` + `loadTrending()` |
| `/social/feed` | `pages/community-feed/community-feed.component.ts` | **NEW:** `FeedActions.loadPosts()` ✅ |
| `/social/create` | `pages/create-poll/create-poll.component.ts` | `SocialActions.createPoll()` |
| `/social/polls/:id` | `pages/social/poll-detail/poll-detail.component.ts` | `SocialActions.loadPollById()` + `vote()` |
| `/social/trending` | `pages/social/trending-outfits/trending-outfits.component.ts` | `SocialActions.loadTrending()` |

---

## 2. Gap Analysis

### 2.1 API URL Mismatches

| Frontend DataSource | Current URL | Correct Backend URL | Status |
|---|---|---|---|
| `FeedDataSource` | `/Feed` | `/api/feed` | ❌ Wrong – missing `api/` prefix |
| `SocialDataSource.getPolls()` | `/Polls/polls` | `/api/polls` | ❌ Wrong – double "polls" segment |
| `SocialDataSource.getPollById()` | `/Polls/polls/${id}` | `/api/polls/${id}` | ❌ Wrong |
| `SocialDataSource.createPoll()` | `/Polls/polls` | `/api/polls` | ❌ Wrong |
| `SocialDataSource.vote()` | `/Polls/polls/…` | `/api/polls/…` | ❌ Wrong |
| `SocialDataSource.updatePoll()` | `/Polls/polls/…` | `/api/polls/…` | ❌ Wrong |
| `SocialDataSource.deletePoll()` | `/Polls/polls/…` | `/api/polls/…` | ❌ Wrong |
| `SocialDataSource.closePoll()` | `/Polls/polls/…` | `/api/polls/…` | ❌ Wrong |
| `SocialDataSource.getTrendingOutfits()` | `/Trending/outfits` | `/api/trending/outfits` | ❌ Wrong – missing `api/` prefix |
| `SocialDataSource.reactToVote()` | `/Feed/…/heart` | `/api/feed/…/heart` | ⚠️ Works only if `Feed` baseUrl is fixed |
| `SocialDataSource.addVoteComment()` | `/Feed/…/comments` | `/api/feed/…/comments` | ⚠️ Same as above |
| `SocialDataSource.getVoteComments()` | `/Feed/…/comments` | `/api/feed/…/comments` | ⚠️ Same as above |

### 2.2 DTO Shape Mismatches

#### Trending Outfits
- **Frontend expects:** `TrendingOutfitDto` with `outfitId`, `outfitImageUrl`, `userName`, `userAvatarUrl`, `likeCount`, `commentCount`, `trendingScore`, `rankPosition`, `voteId`, `createdAt`
- **Backend returns:** `PagedResult<TrendingDataDto>` where `TrendingDataDto` has `Trends: List<TrendItemDto>` and `TrendItemDto` has `Id`, `Title`, `Description`, `Category`, `PopularityScore`, `TrendingSince`
- **Action needed:** Map `TrendItemDto` → `TrendingOutfit` or update backend to return the expected shape.

#### Polls
- **Frontend expects:** `ValidationPollDto` with `id`, `userId`, `question`, `context`, `expiresAt`, `status`, `options`, `totalVotes`, `createdAt`
- **Backend returns:** Same shape ✅ (verified)

#### Feed Posts
- **Frontend expects:** `FeedPostDto` with `id`, `userId`, `userName`, `userAvatarUrl`, `postType`, `outfitId`, `outfit`, `pollId`, `poll`, `caption`, `visibility`, `likeCount`, `commentCount`, `userReaction`, `createdAt`
- **Backend returns:** Same shape ✅ (verified)

### 2.3 Missing Features in Frontend

| Feature | Backend API | Frontend Status |
|---|---|---|
| **Follow / Unfollow** | `POST/DELETE /api/user/users/{id}/follow` | ❌ No UI, no state, no data source |
| **Followers list** | `GET /api/user/users/{id}/followers` | ❌ No UI, no state |
| **Following list** | `GET /api/user/users/{id}/following` | ❌ No UI, no state |
| **Create outfit post** | `POST /api/outfitposts` | ❌ No UI page |
| **Update outfit post** | `PUT /api/outfitposts/{id}` | ❌ No UI |
| **Delete outfit post** | `DELETE /api/outfitposts/{id}` | ❌ No UI |

---

## 3. Endpoint Mapping (Canonical)

### 3.1 Feed (keep `feed` state name, point to `/api/feed`)

```typescript
// feed.datasource.ts
private readonly apiUrl = `${environment.baseUrl}/api/feed`;

GET    ${apiUrl}?cursor=&pageSize=&visibility=&sortBy=&postType=  →  getFeedPosts
GET    ${apiUrl}/${id}                                           →  getPostById
DELETE ${apiUrl}/${id}                                           →  deletePost
POST   ${apiUrl}/${id}/heart                                     →  addReaction
DELETE ${apiUrl}/${id}/heart                                     →  removeReaction
GET    ${apiUrl}/${id}/comments?cursor=&pageSize=                →  getComments
POST   ${apiUrl}/${id}/comments                                  →  addComment
DELETE ${apiUrl}/comments/${commentId}                           →  deleteComment
```

### 3.2 Polls (keep `social` state name, point to `/api/polls`)

```typescript
// social.datasource.ts
private readonly socialApiUrl = `${environment.baseUrl}/api/polls`;

GET    ${socialApiUrl}                                           →  getPolls
GET    ${socialApiUrl}/${id}                                     →  getPollById
POST   ${socialApiUrl}                                           →  createPoll
PUT    ${socialApiUrl}/${id}                                     →  updatePoll
DELETE ${socialApiUrl}/${id}                                     →  deletePoll
POST   ${socialApiUrl}/${id}/vote                                →  vote
POST   ${socialApiUrl}/${id}/close                               →  closePoll
GET    ${socialApiUrl}/recent-poll                               →  getRecentPollWithComments
```

### 3.3 Trending (keep `social` state name, point to `/api/trending`)

```typescript
// social.datasource.ts
private readonly trendingApiUrl = `${environment.baseUrl}/api/trending`;

GET ${trendingApiUrl}/outfits?page=&pageSize=                   →  getTrendingOutfits
```

### 3.4 Engagement (mapped through Feed endpoints)

Reactions and comments in the frontend's "social engagement" methods should now call the **Feed** endpoints (since the backend moved them there):

```typescript
// social.datasource.ts – engagement methods
private readonly feedApiUrl = `${environment.baseUrl}/api/feed`;

POST   ${feedApiUrl}/${voteId}/heart        →  reactToVote (renamed param: postId)
POST   ${feedApiUrl}/${voteId}/comments     →  addVoteComment
GET    ${feedApiUrl}/${voteId}/comments     →  getVoteComments
```

> ⚠️ **Note:** `likeVoteComment` (`/comments/{id}/like`) does **not** exist in the backend. It may need to be removed or a backend endpoint added.

### 3.5 Follow (NEW layer – suggested `follow` state)

```typescript
// follow.datasource.ts (NEW)
private readonly apiUrl = `${environment.baseUrl}/api/user`;

GET    ${apiUrl}/users/${id}/followers?cursor=&pageSize=         →  getFollowers
GET    ${apiUrl}/users/${id}/following?cursor=&pageSize=         →  getFollowing
POST   ${apiUrl}/users/${id}/follow                              →  followUser
DELETE ${apiUrl}/users/${id}/follow                              →  unfollowUser
GET    ${apiUrl}/users/${id}/is-following                        →  isFollowing
```

---

## 4. Implementation Phases

### Phase 1 – Fix URLs (Critical)
**Goal:** Make existing calls actually reach the backend.

1. **`feed.datasource.ts`**
   - Change `apiUrl` from `${environment.baseUrl}/Feed` → `${environment.baseUrl}/api/feed`

2. **`social.datasource.ts`**
   - Change `socialApiUrl` from `${environment.baseUrl}/Polls` → `${environment.baseUrl}/api/polls`
   - Change `trendingApiUrl` from `${environment.baseUrl}/Trending` → `${environment.baseUrl}/api/trending`
   - Change `voteEngagementApiUrl` from `${environment.baseUrl}/Feed` → `${environment.baseUrl}/api/feed`
   - Fix all `.get<ValidationPollDto[]>(…)` calls to remove duplicated `/polls` segment (e.g., `${socialApiUrl}/polls` → `${socialApiUrl}`)

3. **`feed.datasource.ts` – `mapPost()`**
   - Add safe mapping for `postType` numeric enum from backend (`0` → `PostType.OutfitPost`, `1` → `PostType.PollPost`)
   - Map `userReaction` string field correctly

4. **`social.datasource.ts` – `getTrendingOutfits()`**
   - Update response mapping to handle `PagedResult<TrendingDataDto>` shape from backend
   - Map `TrendItemDto` fields → `TrendingOutfit` or negotiate backend change

### Phase 2 – Wire Feed into Community Feed Page
**Goal:** Make `/social/feed` display real feed posts.

1. **`community-feed.component.ts`** – already uses `FeedActions` ✅  
   Ensure the template (`community-feed.component.html`) renders:
   - Outfit posts (image, caption, like/comment counts)
   - Poll posts (question, options, vote button)
   - Heart reaction toggle
   - Comment section toggle

2. Add **infinite scroll** or "Load More" button using `nextCursor` from `feedFeature`.

### Phase 3 – Wire Polls & Trending into Social Page
**Goal:** Make `/social` display real polls and trending outfits.

1. **`social.component.ts`**
   - Keep `SocialActions.loadPolls()` for featured poll & poll list
   - Keep `SocialActions.loadTrending()` for trending outfits
   - Update `vote()` to dispatch correct action
   - Remove or redirect dead methods (`reactToVote`, `addVoteComment`) to Feed actions if targeting a post

2. **`trending-outfits.component.ts`**
   - Verify `loadTrending` response shape after Phase 1 fix

### Phase 4 – Add Follow System (NEW)
**Goal:** Enable follow/unfollow on user profiles.

1. Create **`domain/entities/follow.entity.ts`**
   - `FollowerDto`, `FollowingDto`, `CursorPagedResult<T>`

2. Create **`domain/repositories/follow.repository.ts`**
   - `FollowRepository` interface

3. Create **`data/datasources/follow.datasource.ts`**
   - Calls `/api/user/users/…` endpoints

4. Create **`data/repositories/follow.repository.impl.ts`**

5. Create **`domain/usecases/follow.usecases.ts`**

6. Create **NgRx Follow state:**
   - `core/state/follow/follow.actions.ts`
   - `core/state/follow/follow.reducer.ts`
   - `core/state/follow/follow.effects.ts`
   - `core/state/follow/follow.selectors.ts`

7. Register in `app.config.ts`

8. Add follow button to **Profile page** (`pages/profile/profile.component.ts`)

### Phase 5 – Outfit Post Creation (Optional)
**Goal:** Allow users to share outfits to the feed.

1. Add "Share to Feed" button on **Outfit Detail** page
2. Dispatch new action → call `POST /api/outfitposts`
3. Add simple outfit post card to feed rendering

---

## 5. Files to Modify / Create

### Modify (URL fixes + wiring)
- `src/outfit-planner-ui/src/app/data/datasources/feed.datasource.ts`
- `src/outfit-planner-ui/src/app/data/datasources/social.datasource.ts`
- `src/outfit-planner-ui/src/app/presentation/pages/community-feed/community-feed.component.html`
- `src/outfit-planner-ui/src/app/presentation/pages/social/social.component.ts`
- `src/outfit-planner-ui/src/app/presentation/pages/social/social.component.html`
- `src/outfit-planner-ui/src/app/presentation/pages/social/trending-outfits/trending-outfits.component.ts`

### Create (Follow system)
- `src/outfit-planner-ui/src/app/domain/entities/follow.entity.ts`
- `src/outfit-planner-ui/src/app/domain/repositories/follow.repository.ts`
- `src/outfit-planner-ui/src/app/data/datasources/follow.datasource.ts`
- `src/outfit-planner-ui/src/app/data/repositories/follow.repository.impl.ts`
- `src/outfit-planner-ui/src/app/domain/usecases/follow.usecases.ts`
- `src/outfit-planner-ui/src/app/core/state/follow/follow.actions.ts`
- `src/outfit-planner-ui/src/app/core/state/follow/follow.reducer.ts`
- `src/outfit-planner-ui/src/app/core/state/follow/follow.effects.ts`
- `src/outfit-planner-ui/src/app/core/state/follow/follow.selectors.ts`

### Register
- `src/outfit-planner-ui/src/app/app.config.ts` – add `followReducer`, `FollowEffects`, `followRepositoryProvider`

---

## 6. Naming Convention Rules

To satisfy the requirement **"do not change name of social in front, keep it"**:

| Concept | Frontend Name | Maps To Backend |
|---|---|---|
| NgRx store slice for polls & trending | `social` | `PollsController` + `TrendingController` |
| NgRx store slice for feed posts | `feed` | `FeedController` |
| NgRx store slice for follow system | `follow` (new) | `UserController` follow endpoints |
| Folder / file names | Keep `social.*` for polls/trending | — |
| Page route | Keep `/social` | — |
| DataSource class | `SocialDataSource` (polls/trending) | — |
| DataSource class | `FeedDataSource` (feed posts) | — |

> Do **not** rename `SocialDataSource`, `SocialActions`, `socialFeature`, `SocialEffects`, or any `social.*` files. Only fix their internal API URLs.

---

## 7. Verification Checklist

- [ ] `feed.datasource.ts` calls return 200 from `/api/feed`
- [ ] `social.datasource.ts` poll calls return 200 from `/api/polls`
- [ ] `social.datasource.ts` trending calls return 200 from `/api/trending/outfits`
- [ ] Community feed page renders real feed posts with outfits and polls
- [ ] Heart reaction toggles correctly on feed posts
- [ ] Comments load and post correctly on feed posts
- [ ] Poll creation works end-to-end
- [ ] Voting on a poll updates counts
- [ ] Trending outfits display correctly
- [ ] Follow/unfollow buttons work on profile pages

---

*Plan generated for commit `4417d90`. Execute Phase 1 first to unblock all other phases.*
