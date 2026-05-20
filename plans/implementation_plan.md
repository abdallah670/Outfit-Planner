# Implementation Plan - Community Feed Consolidation & Standardization

This plan outlines the steps to unify post design and consolidate all feed types (All, Following, Trending, Followers, Following) into a single Community Feed component using shared domain use cases.

## Goal

- Centralize post rendering into a reusable `PostItemComponent` based on the premium styling of the `PublicProfileComponent`.
- Consolidate "Trending Outfits" into the Community Feed as a tab.
- Support infinite scrolling across all tabs. For the "Trending" tab, the frontend will adapt the backend's page-based pagination into a seamless cursor-like scrolling experience.
- Completely remove the use of NgRx Actions in favor of direct UseCase injection for `FeedUseCases`, `TrendingUseCases`, and `FollowUseCases`.

---

## Proposed Changes

### 1. Shared Components

#### [NEW] `src/app/presentation/components/shared/post-item/post-item.component.ts`
- **Purpose**: A reusable component for rendering a single `FeedPost` (both Polls and Outfit posts).
- **Features**:
  - Handles "Like" toggling via `FeedUseCases.addReaction`/`removeReaction`.
  - Handles "Vote" toggling via `FeedUseCases.voteOnPoll`/`removeVote`.
  - Opens `CommentsModalComponent` and `VotersModalComponent` using `SweetAlert2` and `ViewContainerRef`.
  - Emits a `postUpdated` event to allow parent components to sync state if necessary.

#### [NEW] `src/app/presentation/components/shared/post-item/post-item.component.html`
- **Design**: Uses the exact HTML structure and classes from `PublicProfileComponent`'s `.post-card` block. Supports showing the outfit image, poll grid, tags, header (avatar, username, time ago), and action buttons.

#### [NEW] `src/app/presentation/components/shared/post-item/post-item.component.scss`
- **Styling**: Contains the isolated styles required for the post card, extracted from `public-profile.component.scss`.

---

### 2. Community Feed Consolidation

#### [MODIFY] `src/app/presentation/pages/social/community-feed/community-feed.component.ts`
- **Dependencies**: Inject `FeedUseCases`, `TrendingUseCases`, `FollowUseCases`, and `AuthService`.
- **Tabs Configuration**:
  - `all`: Calls `feedUseCases.getFeed(cursor, pageSize)`.
  - `following`: Calls `feedUseCases.getFeed(cursor, pageSize, visibility, sortBy, postType)`. *(Note: Requires determining backend support for "following only" feed, or using `getUserFeed` if applicable).*
  - `trending`: Calls `trendingUseCases.getTrendingOutfits(page, pageSize)`.
  - `followers`: Calls `followUseCases.getFollowers(userId, cursor, pageSize)`.
  - `following-list`: Calls `followUseCases.getFollowing(userId, cursor, pageSize)`.
- **Trending Pagination Strategy**:
  - Maintain a signal `trendingPage = signal(1)`.
  - When `loadMore()` is triggered on the trending tab, increment `trendingPage()` and fetch the next page.
  - Map the returned `TrendingOutfit[]` into the `FeedPost[]` interface locally so it can be passed uniformly to `PostItemComponent`.
- **State Management**: Use localized Angular signals (`posts`, `userList`, `nextCursor`, `loading`, `hasMore`) instead of the NgRx store to manage the lists.

#### [MODIFY] `src/app/presentation/pages/social/community-feed/community-feed.component.html`
- Replace the existing `ngFor` loop for posts with a loop rendering `<app-post-item [post]="post" (postUpdated)="onPostUpdated($event)"></app-post-item>`.
- Add an `ng-container` to render the user lists (avatars, handles, follow/unfollow buttons) for the `followers` and `following-list` tabs.
- Ensure the `loadMore()` button triggers regardless of the active tab.

#### [MODIFY] `src/app/presentation/pages/social/community-feed/community-feed.component.scss`
- Remove existing post-related styles (as they are now in `PostItemComponent`).
- Add styles for the `user-list` to match the design from the Public Profile.

---

### 3. Public Profile Updates

#### [MODIFY] `src/app/presentation/pages/public-profile/public-profile.component.ts`
- Remove local duplicated methods: `toggleReaction`, `toggleVote`, `openCommentsModal`, `openVotersModal`, `getTimeAgo`, `getvotePercentage`, `isWinner`.

#### [MODIFY] `src/app/presentation/pages/public-profile/public-profile.component.html`
- Replace the `div.post-card` block with `<app-post-item [post]="post"></app-post-item>`.

---

### 4. Route Cleanup

#### [DELETE] `src/app/presentation/pages/social/trending-outfits/trending-outfits.component.ts` (and related `.html`, `.scss`)
- This component is completely replaced by the `trending` tab in the community feed.

#### [MODIFY] `src/app/app.routes.ts`
- Remove `{ path: 'social/trending', component: TrendingOutfitsComponent }`.

---

## Open Questions

1. **"Following" Posts Feed API**: Does `feedRepository.getFeedPosts()` currently support a filter parameter to only return posts from users the current user follows? If not, we may need a minor backend tweak to filter the feed, or rely on `Visibility` filters.
2. **TrendingOutfit to FeedPost Mapping**: We will map `TrendingOutfit` to `FeedPost` locally on the client. Are there any specific properties on a `TrendingOutfit` (like `trendingScore`) that you want visible in the UI, or should it look exactly like a standard Outfit post?

## User Review Required

Please review the pagination strategy for the Trending tab (handling page numbers locally to simulate a cursor) and the mapping plan. If this aligns with your expectations, approve the plan and I will complete the execution.
