# Social Feed & Social Features Implementation Plan

**Scope:** Phase 0 (Critical Bug Fixes) + 4 Implementation Phases  
**Goal:** A fully functional social experience — feed, polls, outfit posts, follows, hearts, comments

---

## Phase 0: Critical Bug Fixes & Wiring (1-2 Days)

**Goal:** Fix broken social features so they actually work with the backend.  
**This must be done BEFORE any new features.**

### 🔴 Fix 1: Trending API URL (30 min)
| File | Current URL | Correct URL |
|------|-------------|-------------|
| `src/data/datasources/trending.datasource.ts` | `${baseUrl}/trending/outfits` | `${baseUrl}/api/trending/outfits` |

**Fix:** Change line 25 from `/trending/outfits` → `/api/trending/outfits`

### 🔴 Fix 2: Wire Up Community Feed to Real Data (4-5 hours)

**Current Problem:** `CommunityFeedComponent` dispatches `FeedActions.loadPosts()` but:
1. The feed NgRx effects may not call the correct endpoints
2. The feed HTML template may display placeholder/empty states
3. Poll data from backend may not map correctly to the feed

**Tasks:**

| Step | File | What to Fix |
|------|------|-------------|
| 1 | `src/core/state/feed/feed.effects.ts` | Verify `loadPosts$` effect calls `feedRepository.getFeedPosts()` with correct params |
| 2 | `src/core/state/feed/feed.reducer.ts` | Verify reducer handles `loadPostsSuccess` and stores posts correctly |
| 3 | `src/presentation/pages/community-feed/community-feed.component.html` | Wire the component's `filteredPosts()` signal to display real cards with images, user info, like button, comment button |
| 4 | `src/presentation/pages/community-feed/community-feed.component.scss` | Style the feed cards properly |
| 5 | **BACKEND** `src/OutfitPlanner.Api/Controllers/FeedController.cs` | Verify `GetFeed` endpoint returns the correct structure matching `FeedPost` entity |

**Test Checklist:**
- [ ] Feed page loads posts from backend
- [ ] Each post shows user avatar, username, content, image
- [ ] Filter tabs (All/Outfits/Polls) work
- [ ] Load More pagination works

### 🔴 Fix 3: Add Heart/Reaction Toggle UI on Feed (2 hours)

**Current State:** `CommunityFeedComponent.ts` has `toggleReaction()` method that dispatches `FeedActions.addReaction()` and `FeedActions.removeReaction()`, but the HTML template may not have the visual toggle button wired up.

**Tasks:**

| Step | File | What to Fix |
|------|------|-------------|
| 1 | `community-feed.component.html` | Add heart/favorite icon button next to each post with click handler `toggleReaction(post, $event)` |
| 2 | `community-feed.component.html` | Show filled heart if `post.userReaction` exists, outline if not |
| 3 | `community-feed.component.html` | Display like count (`post.reactionCount` or similar) |
| 4 | Test: Click heart → dispatches addReaction → backend returns success → heart fills |

### 🔴 Fix 4: Add Comment Input UI on Feed (2 hours)

**Current State:** Backend has `addComment` and `getComments` endpoints. Frontend `FeedDataSource` has `addComment()` and `getComments()` methods. But there's no comment input UI on the feed.

**Tasks:**

| Step | File | What to Fix |
|------|------|-------------|
| 1 | `community-feed.component.html` | Add comment input field below each post (expandable) |
| 2 | `community-feed.component.ts` | Add `toggleComments(postId)`, `loadComments(postId)`, `submitComment(postId, text)` methods |
| 3 | `community-feed.component.html` | Display comments list with avatar, username, text, timestamp |
| 4 | Connect to store: dispatch `FeedActions.loadComments()` and `FeedActions.addComment()` |

### 🔴 Fix 5: Add Follow/Unfollow Button on Public Profiles (3 hours)

**Current State:** Backend has `POST /api/user/{id}/follow`, `DELETE /api/user/{id}/follow`, `GET /api/user/{id}/followers`, `GET /api/user/{id}/following`. Frontend has `follow.entity.ts`, `follow.repository.ts`, `follow.datasource.ts`, `FollowEffects`, and `FollowReducer`. But PublicProfileComponent has **no follow button**.

**Tasks:**

| Step | File | What to Fix |
|------|------|-------------|
| 1 | `src/presentation/pages/public-profile/public-profile.component.ts` | Inject `Store`, dispatch `FollowActions.followUser()` / `unfollowUser()` |
| 2 | `src/presentation/pages/public-profile/public-profile.component.html` | Add "Follow" / "Unfollow" button next to user name |
| 3 | `public-profile.component.ts` | Add follower/following count display |
| 4 | `public-profile.component.ts` | Add `isFollowing` state to toggle button text |
| 5 | Test: Click Follow → button changes to "Unfollow" → count increments |

### 🔴 Fix 6: Fix Polls Datasource Response Handling (1 hour)

**Current State:** `PollsDataSource.getPolls()` expects `PollDto[]` but the backend returns `PaginatedResult<PollDto>`.

**Potential Issue:** If the backend wraps poll responses in a paginated envelope, the frontend needs to unwrap it.

**Tasks:**
1. Check actual backend response format for `GET /api/polls`
2. If wrapped in paginated envelope, update `getPolls()` to unwrap `response.items`
3. Test: Polls page loads correctly

---

## Phase 1: Polls Management Foundation (8-10 hours)

**Goal:** Users can manage their polls (view, edit, delete)

### Tasks:
1. **Extend Polls NgRx State** (1 hour)
   - Add `loadUserPolls`, `updatePoll`, `deletePoll`, `closePoll` actions
   - Update reducer with new actions
   - Add selectors: `selectUserPolls`, `selectPollLoadingStates`

2. **Extend Polls Repository** (1 hour)
   - Add methods: `getUserPolls()`, `updatePoll()`, `deletePoll()`, `closePoll()`
   - Implement HTTP calls to backend

3. **Extend Polls Use Cases** (30 min)
   - Add: `getMyPolls()`, `updatePoll()`, `deletePoll()`, `closePoll()`

4. **Create My Polls Page** `/social/my-polls` (3 hours)
   - List view of user's polls
   - Status badges (Active/Expired/Closed)
   - Action buttons: Edit, Delete, Close
   - SweetAlert confirmations
   - Empty state
   - Navigate to detail on click

5. **Create Edit Poll Page** `/social/polls/:id/edit` (2.5 hours)
   - Reuse/refactor create-poll form
   - Load existing poll data
   - Pre-populate all fields
   - Add/remove poll options
   - Save/Cancel actions

### Deliverables:
- ✅ Users can view all their polls
- ✅ Users can edit existing polls
- ✅ Users can delete polls
- ✅ Users can close polls

---

## Phase 2: Polls Feed Integration (6-8 hours)

**Goal:** Polls are interactive in the community feed

### Tasks:
1. **Poll Card Component** (2 hours)
   - Compact poll display for feed
   - Voting options (click to vote)
   - Results view with progress bars
   - Time remaining badge
   - Total votes count

2. **Feed Integration** (2 hours)
   - Update feed to display polls inline
   - Mixed content type support
   - Load polls from backend
   - Handle vote actions

3. **Poll Detail Enhancement** (2 hours)
   - Full voting interface
   - Results visualization
   - Owner controls (Edit, Delete, Close)
   - Comments section

4. **Social Hub Dashboard** (1 hour)
   - Quick "My Polls" card with count
   - "Create Poll" button
   - Recent polls preview

### Deliverables:
- ✅ Polls display in community feed
- ✅ Users can vote on polls in feed
- ✅ Enhanced poll detail page
- ✅ Social hub shows poll summary

---

## Phase 3: Outfit Posts CRUD (10-12 hours)

**Goal:** Users can create and manage outfit posts

### Tasks:
1. **Verify/Extend Outfit Posts State** (1 hour)
   - Review existing NgRx setup
   - Add missing actions if needed
   - Add `loadUserPosts` action

2. **Create Outfit Post Page** `/social/create-post` (4 hours)
   - Outfit selector modal (browse user's outfits)
   - Caption/description input
   - Tag selection (occasion, season, weather)
   - Privacy toggle (Public/Private)
   - Photo upload option
   - Preview before post
   - SweetAlert success

3. **Create My Posts Page** `/social/my-posts` (3 hours)
   - Grid view of user's posts
   - Post cards with image, caption, likes
   - Edit/Delete actions
   - Filter: All, Public, Private
   - Sort: Newest, Most Liked
   - Empty state

4. **Create Edit Post Page** `/social/posts/:id/edit` (2 hours)
   - Load existing post
   - Edit caption, tags, privacy
   - Save/Cancel

### Deliverables:
- ✅ Users can create outfit posts
- ✅ Users can view their posts
- ✅ Users can edit posts
- ✅ Users can delete posts

---

## Phase 4: Feed Enhancement & Polish (8-10 hours)

**Goal:** Unified feed with both content types, complete interactions

### Tasks:
1. **Outfit Post Card Component** (2 hours)
   - Image display with carousel
   - User info header
   - Caption text
   - Like button with count
   - Comment preview
   - Tap to detail

2. **Feed Integration** (3 hours)
   - Mixed content: polls + outfit posts
   - Filter tabs: All, Polls, Outfit Posts
   - Sort options: Latest, Popular
   - Infinite scroll pagination
   - Loading skeletons

3. **Outfit Post Detail Page** `/social/posts/:id` (3 hours)
   - Full outfit display
   - Image carousel
   - Like/Unlike functionality
   - Comments section (wired to real backend)
   - Share button
   - Owner controls

4. **Polish & Testing** (2 hours)
   - Error handling
   - Loading states
   - Empty states
   - Responsive design check
   - Navigation flow testing

### Deliverables:
- ✅ Mixed content feed (polls + posts)
- ✅ Feed filtering and sorting
- ✅ Full outfit post detail page
- ✅ Complete like/comment interactions

---

## Summary Timeline

| Phase | Features | Hours | Cumulative |
|-------|----------|-------|------------|
| **Phase 0** | Critical Bug Fixes (Trending URL, Feed wiring, Hearts UI, Comments UI, Follow UI) | 8-13 | 8-13 hours |
| **Phase 1** | Polls Management | 8-10 | 16-23 hours |
| **Phase 2** | Polls Feed Integration | 6-8 | 22-31 hours |
| **Phase 3** | Outfit Posts CRUD | 10-12 | 32-43 hours |
| **Phase 4** | Feed Enhancement & Polish | 8-10 | 40-53 hours |

---

## Phase Dependencies

```
Phase 0 ──→ Phase 1 ──→ Phase 2 ──┐
                                   ├──→ Phase 4
Phase 3 ───────────────────────────┘
```

- **Phase 0** must be done first (without it, nothing works with the backend)
- Phase 1 → Phase 2: Need poll management before feed integration
- Phase 1 + Phase 3 → Phase 4: Need both content types for unified feed
- Phases 1 and 3 can be done in parallel

---

## Critical Success Criteria

After **Phase 0:**
- ✅ Trending outfits display correctly (URL fix)
- ✅ Feed loads real posts from backend
- ✅ Hearts/reactions work on feed posts
- ✅ Comments can be added/viewed on feed posts
- ✅ Follow/unfollow works on public profiles

After **Phase 1:** Users can fully manage their polls
After **Phase 2:** Polls are interactive in community
After **Phase 3:** Users can share and manage outfit posts  
After **Phase 4:** Complete social feed experience

---

## Execution Order (Recommended)

**Sequential approach (safest):**
1. ✅ **Phase 0** — Fix bugs first (everything depends on this)
2. ✅ **Phase 1** — Poll Management
3. ✅ **Phase 2** — Polls in Feed
4. ✅ **Phase 3** — Outfit Posts
5. ✅ **Phase 4** — Unified Feed

**Parallel approach (faster):**
1. ✅ **Phase 0** — Critical fixes
2. ✅ **Phase 1 + Phase 3** in parallel (independent)
3. ✅ **Phase 2 + Phase 4** (poll feed + unified feed)