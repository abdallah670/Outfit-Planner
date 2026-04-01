# Social Platform Architecture Review: Plan vs Reality

## 1. Brutally Honest Summary

The current architectural plan (`dynamic-social-revised-plan.md`) and implementation are fundamentally flawed. You are attempting to build a fully-fledged social platform by "hacking" an existing `ValidationPoll` (A/B testing) system. 

Forcing an open-ended social feed ("Instagram-style posts") into a closed-loop polling system causes catastrophic impedance mismatch. Polls have lifecycles, expiration dates, and distinct options. Social posts are open, continuous streams of content. 

By treating every shared Outfit as a "1-option Poll" and treating generic "Likes" as "Votes on Poll Options", and even worse, "Comments" as "Comments on Votes", you are introducing massive conceptual leaks, horrifying database query complexity, and an unmaintainable codebase. 

**Verdict**: Do not proceed with the "Revised Plan". It is a technical debt factory that will not scale. You must decouple the general Social Feed from the Validation Polls feature.

---

## 2. Major Problems

1. **The Conceptual Hack**: The plan states: `User creates outfit → Automatically creates "Outfit Poll"`. This means every single shared outfit generates redundant poll data.
2. **Comments on Votes**: The plan proposes `VoteComments`. In a real social app, users comment on the *Post* (Outfit), not on the fact that *John voted 5 stars*. Threaded discussions attached to a "Vote" entity make absolutely no sense and break basic UX expectations.
3. **Poll Lifecycles**: `ValidationPoll` entities have `ExpiresAt` and `Status` (Active/Closed). Does a social post "expire"? No. The system will artificially close engagement on outfits just because the underlying "poll" expired.
4. **Rating vs Liking**: The `Vote` entity has a 1-5 `Rating`. The plan attempts to bypass this by hardcoding `rating: 5` in the Angular frontend (`social.component.ts:132`) just to simulate a "Like". This is a massive smell.
5. **Reactions on Votes**: Instead of liking an outfit, users are liking a *vote* via `VoteReactions`. The query required to find out how many people "Loved" an outfit involves traversing `Outfit -> PollOption -> Vote -> VoteReaction`. This is absurd.

---

## 3. Detailed Audit

### A. Frontend (Angular)
- **Component Misalignment**: `social.component.ts` uses `ValidationPoll` entities but tries to present them as social feeds. It maps `PollOptions` to display cards. It hardcodes `rating: 5` to cast a vote.
- **State Management**: The NgRx store requests `loadPolls` and `loadTrending` at the same time, but the underlying data models (`TrendingOutfit` vs `ValidationPoll`) are disconnected. The UI has to manually format "time left" strings because it's rendering polls, not continuous social posts.
- **Scalability**: Fetching all active polls (`selectAllPolls`) into memory and filtering them on the client-side (`featuredPoll = computed(...)`) will crash the user's browser when you have 10,000 active posts.

### B. Backend (.NET)
- **API Design**: The endpoints (`/api/social/polls`) strictly talk about polls. To fetch a social feed, a client has to fetch polls, extract the first option, and pretend it's a post.
- **Business Logic**: Controllers like `VoteEngagementController` handling `comments/{commentId}/reply` where the root is a `Vote` creates convoluted validation rules. How do you prevent users from replying to a vote on an expired poll? 
- **Domain Violation**: You are mixing "Asking for advice" (Polls) with "Showcasing style" (Social Feed).

### C. Database (EF Core)
- **Relationships**: 
  - `ValidationPoll` -> `PollOption` -> `Vote` -> `VoteReaction`
- **Performance Nightmare**: To generate the "Daily Trending Outfits", the background job has to scan `Outfits`, join `PollOptions`, join `Votes` (decaying by date), and join `VoteComments`. This will instantly lock up a database with high concurrent writes.
- **Missing Constraints**: The EF schema in `AppDbContext` lacks dedicated `OutfitLikes` mapping that connects directly to `Outfits` (though a DbSet exists, it's bypassed by the new plan). 

---

## 4. Production Risks

1. **The Feed Performance Collapse**: Pagination and sorting a feed based on nested aggregations (counting `Votes` grouped by `OptionId` where `PollStatus == Active`) requires heavy CPU usage on the SQL Server. As your table grows to millions of votes, the feed simply won't load.
2. **Race Conditions on Engagement Counters**: When an outfit goes viral, thousands of users might upvote simultaneously. Without Redis or atomic database increments (`UPDATE Posts SET LikeCount = LikeCount + 1`), you will experience deadlocks or lost votes.
3. **The "Duplicate Poll" Abuse**: Users spamming outfits will create millions of 1-option polls, bloating the `ValidationPolls` table with garbage rows that serve no purpose other than acting as a proxy for an Outfit ID.

---

## 5. Recommended Architecture (The Correct Way)

Throw out the idea of using `ValidationPolls` for the social feed. Keep polls strictly for "Which of these 2 outfits should I wear to the wedding?". 

Create a proper, decoupled Social domain.

### Core Entities
1. **`OutfitPost`**: The actual social feed entity.
   - `Id`, `UserId`, `OutfitId`, `Caption`, `Visibility` (Public/Friends), `LikeCount`, `CommentCount`, `CreatedAt`
2. **`PostLike` (or `OutfitLike`)**:
   - `Id`, `PostId`, `UserId`, `ReactionType` (Like, Fire, etc.), `CreatedAt`
   - *Index on (PostId, UserId)* with a UNIQUE constraint to prevent duplicate likes.
3. **`PostComment`**:
   - `Id`, `PostId`, `ParentCommentId` (for threads), `UserId`, `Content`, `CreatedAt`

### Concurrency and Scale
- **Materialized Counters**: Do not `COUNT()` likes on the fly. Store `LikeCount` on `OutfitPost`. Use asynchronous events (MediatR `PostLikedEvent`) to increment this counter safely.
- **Idempotent Votes**: API endpoints must check `EXISTS` or catch `DbUpdateException` from the UNIQUE constraint to handle double-clicks without crashing.

---

## 6. Step-by-Step Implementation Plan

### Phase 1: Database Redesign & Migration
1. Revert `dynamic-social-revised-plan.md` entirely.
2. Establish `OutfitPost`, `PostLike`, and `PostComment` entities in `.NET` Domain.
3. Keep `ValidationPoll` exactly as it was original intended (Multi-option A/B testing).
4. Run EF Migrations to generate the clean social tables.

### Phase 2: Backend Social Services
1. Create `SocialFeedController` using cursor-based pagination (e.g., `?before=TIMESTAMP`) instead of `page/pageSize` to prevent feed shifting as new items arrive.
2. Implement backend validation: Prevent a user from liking the same `PostId` twice.
3. Create `TrendingJob` that calculates scores based ONLY on direct `PostLikes` and `PostComments`, saving results into memory (Redis) or a `TrendingPosts` cache table.

### Phase 3: Frontend Complete Overhaul
1. Replace `SocialComponent` poll-hack. Fetch from `/api/feed/trending` and `/api/feed/recent`.
2. Update NgRx state to handle `OutfitPost` entities, not `ValidationPoll` entities.
3. **Optimistic UI**: When a user clicks "Like", immediately increment the counter and change the icon to red on the frontend *before* the API call completes. If the API fails, revert the state.

### Final Thoughts
This pivot will save you months of agonizing bug fixing. Build a real social feed, not a disguised survey app.
