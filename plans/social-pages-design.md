# Outfit Planner Social Pages Implementation Plan

## Overview
This plan focuses specifically on the social-related pages and features of the Outfit Planner application, including the community feed, polls, trending outfits, and social interactions.

## 1. Community Feed Page (/social/feed)

### Page Structure
```
- Header: "Community Feed" with back to social button
- Filter section: Category filters (All, Outfits, Polls)
- Posts grid with:
  * User avatar and name
  * Post timestamp
  * Post type indicator (Outfit/Poll)
  * Outfit images or poll question
  * Like/comment/share buttons
  * Interaction counts
- Floating action button for creating new posts
- Empty state when no posts
```
### Features
- **Post Types**:
  - Outfit posts (single or multiple images)
  - Poll posts (2-4 outfit options)
  - Question posts (text-based)
- **Interactions**:
  - Like/unlike with animation
  - Comment system with replies
  - Share functionality
- **Filtering**:
  - By category
  - By post type
  - By time (newest, trending)
- **Pagination**:
  - Infinite scroll
  - Cursor-based pagination



## 2. Social Validation Page (/social)

### Page Structure
```
- Header: "Outfit Planner - Social Validation"
- Three-column layout:
  * Left: Community Feed (trending outfits)
    - Header with "Trending Outfits"
    - Outfit cards with likes and comments
    - "See All" link
  * Center: Active Polls
    - Featured poll card
    - Poll options with images
    - Vote buttons
    - Live results display
    - Countdown timer
  * Right: Results & Share
    - Live results bar chart
    - Share poll section
    - QR code for mobile voting
- Community feedback section
```

### Backend Integration
- Connect to `PollsController` endpoints:
  - GET `/api/polls` - get all polls
  - GET `/api/polls/{id}` - get specific poll
  - POST `/api/polls` - create new poll
  - POST `/api/polls/{id}/vote` - cast vote
  - GET `/api/polls/{id}/results` - get results

- Connect to `TrendingController` endpoints:
  - GET `/api/trending` - get trending outfits
  - GET `/api/trending/{id}` - get specific trending item

### State Management
```typescript
// NgRx Actions
- loadPolls()
- loadPollsSuccess(polls)
- loadPollById(id)
- loadPollByIdSuccess(poll)
- createPoll(request)
- createPollSuccess(response)
- vote(pollId, request)
- voteSuccess(pollId)
- loadTrending()
- loadTrendingSuccess(outfits)
- reactToVote(voteId, reactionType)
- addVoteComment(request)
- loadVoteComments(voteId)

// Selectors
- selectActivePolls
- selectFeaturedPoll
- selectTrendingOutfits
- selectCommentsByVote
```

## 3. Poll Detail Page (/social/polls/:id)

### Page Structure
```
- Header with back button and poll question
- Poll options display:
  * Images for each option
  * Vote count and percentage
  * Vote buttons
- Results section:
  * Visual bar chart
  * Total votes count
  * Time remaining (if active)
- Comments section:
  * User comments
  * Add comment form
  * Comment replies
```

### Features
- **Voting**:
  - Single vote per user
  - Real-time result updates
  - Visual feedback on vote
- **Results**:
  - Percentage-based display
  - Animated transitions
  - Share results functionality
- **Comments**:
  - Threaded comments
  - Like comments
  - Timestamps
  - User avatars

### Component Architecture
```typescript
// Core Components
- PollDetailPage (main container)
- PollHeader (question and metadata)
- PollOptions (voting section)
- PollResults (visualization)
- CommentsSection (discussion)
- AddCommentForm (input)
```

## 4. Create Poll Page (/social/create)

### Page Structure
```
- Header: "Create Poll"
- Form steps:
  * Step 1: Select Outfits
    - Outfit selection from wardrobe
    - Drag and drop to reorder
  * Step 2: Configure Poll
    - Question input
    - Duration setting
    - Privacy options
  * Step 3: Preview & Publish
    - Poll preview
    - Share options
- Navigation between steps
- Cancel/Back buttons
```

### Features
- **Outfit Selection**:
  - Browse wardrobe items
  - Select multiple outfits (2-4)
  - Preview selected outfits
- **Poll Configuration**:
  - Custom question
  - Duration (1-7 days)
  - Privacy settings
- **Preview & Share**:
  - Poll preview
  - Generate share link
  - QR code generation

### Component Architecture
```typescript
// Core Components
- CreatePollPage (main container)
- PollWizard (step navigation)
- OutfitSelectionStep
- PollConfigStep
- PreviewStep
- OutfitSelector
```

## 5. Follow System / User Profiles

### Page Structure
```
- Profile Header:
  * User avatar and name
  * Follow/Unfollow button
  * Stats (items, outfits, followers)
- Tabbed Content:
  * Outfits (gallery)
  * Activity (recent posts, comments)
  * Following/Followers lists
```

### Features
- **Follow/Unfollow**:
  - One-click follow
  - Follow count display
  - Follow notifications
- **Profile Gallery**:
  - User's outfits
  - Liked outfits
  - Activity timeline
- **Social Connections**:
  - Following list
  - Followers list
  - Mutual connections

### Backend Integration
- Connect to `FollowController` endpoints:
  - GET `/api/following` - get users following
  - GET `/api/followers` - get user followers
  - POST `/api/follow/{userId}` - follow user
  - DELETE `/api/follow/{userId}` - unfollow user

## 6. Outfit Post Creation UI

### Page Structure
```
- Modal/Overlay with:
  * Header: "Share to Feed"
  * Outfit preview
  * Caption input
  * Tags/mentions
  * Privacy settings
  * Share button
```

### Features
- **Quick Share**:
  - Share from outfit detail page
  - Pre-populated outfit data
  - Caption customization
  - Tag friends
- **Post Options**:
  - Privacy settings
  - Location tagging
  - Outfit categories

### Integration Points
- Add "Share to Feed" button on outfit detail page
- Connect to `FeedController` POST endpoint
- Handle image upload for outfit posts

## Design System Reference

### Colors

| Name | Hex Code | Usage |
|------|----------|-------|
| Background | #FAFAFA | Page background |
| Surface | #FFFFFF | Cards, panels |
| Primary | #F8B4C4 | CTAs, highlights, active states |
| Secondary | #9CAF88 | Success, secondary actions |
| Text Primary | #2D3436 | Headlines, body text |
| Text Secondary | #636E72 | Captions, metadata |
| Border | #DFE6E9 | Dividers, borders |
| Error | #E17055 | Error states |
| Warning | #FDCB6E | Warning states |

### Social-Specific Components

### Post Card

```
A card component displaying social posts with:
- User avatar and name
- Timestamp
- Post type indicator (Outfit/Poll)
- Content images
- Like/comment/share buttons
- Interaction counts
- Hover effects
- Dimensions: Variable width, 300px height
```

### Poll Card

```
A card for social validation polls with:
- Question at top
- 2-4 outfit options side by side
- Vote count for each option
- Time remaining badge
- Total votes counter
- "Vote" button for each option
- Dimensions: Full width, 400px height
```

### Trending Outfit Card

```
A card displaying trending outfits with:
- Outfit image
- User avatar
- Like count
- Comment count
- Trending rank badge
- "View Details" button
- Dimensions: 200x250px
```

## Implementation Priorities

### Phase 1: Feed Foundation
- [ ] Fix FeedDataSource URLs
- [ ] Implement basic feed page with post display
- [ ] Add like/comment functionality
- [ ] Create post creation modal

### Phase 2: Poll System
- [ ] Fix SocialDataSource URLs
- [ ] Implement poll creation page
- [ ] Add voting functionality
- [ ] Display poll results

### Phase 3: Social Integration
- [ ] Wire Feed into Community Feed page
- [ ] Wire Polls & Trending into Social page
- [ ] Add follow system UI
- [ ] Implement user profiles

### Phase 4: Enhanced Features
- [ ] Outfit Post creation UI
- [ ] Real-time updates
- [ ] Push notifications
- [ ] Advanced search/filtering

## API Endpoints Reference

### Feed Endpoints
- `GET /api/feed` - Get feed posts
- `POST /api/feed` - Create post
- `GET /api/feed/{id}` - Get post by ID
- `POST /api/feed/{id}/like` - Like/unlike post
- `GET /api/feed/{id}/comments` - Get comments
- `POST /api/feed/{id}/comments` - Add comment
- `DELETE /api/feed/{id}` - Delete post

### Polls Endpoints
- `GET /api/polls` - Get all polls
- `GET /api/polls/{id}` - Get poll by ID
- `POST /api/polls` - Create poll
- `POST /api/polls/{id}/vote` - Cast vote
- `GET /api/polls/{id}/results` - Get results

### Trending Endpoints
- `GET /api/trending` - Get trending outfits
- `GET /api/trending/{page}` - Paginated trending

### Follow Endpoints
- `GET /api/following` - Get following list
- `GET /api/followers` - Get followers
- `POST /api/follow/{userId}` - Follow user
- `DELETE /api/follow/{userId}` - Unfollow user

*Document Version: 1.0*
*Last Updated: April 2026*