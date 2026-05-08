# Unified Social Module Implementation Plan

## Overview
This plan consolidates all social features into a single, cohesive social module with unified routing and state management. The goal is to eliminate scattered files and create a maintainable social experience.

## Current Structure Analysis

### Existing Components
- `src/outfit-planner-ui/src/app/presentation/pages/social/social.component.ts` - Main social page (3-column layout)
- `src/outfit-planner-ui/src/presentation/pages/community-feed/community-feed.component.ts` - Feed page
- `src/outfit-planner-ui/src/presentation/pages/create-poll/create-poll.component.ts` - Poll creation
- `src/outfit-planner-ui/src/presentation/pages/social/poll-detail/poll-detail.component.ts` - Poll voting
- `src/outfit-planner-ui/src/presentation/pages/social/trending-outfits/trending-outfits.component.ts` - Trending page

### Current State Management
- NgRx store with `SocialActions`, `SocialEffects`, `SocialReducer`
- Complete CRUD operations for polls, trending, and engagement
- Feed actions and effects already implemented

### Current Issues
1. **API URL Mismatch**: SocialDataSource uses `/Polls`, `/Feed`, `/Trending` instead of `/api/polls`, `/api/feed`, `/api/trending`
2. **Scattered Pages**: Social features spread across multiple component files
3. **Disconnected Features**: Feed, polls, and trending are not fully integrated

## Implementation Plan

### Phase 1: Fix API URLs and Backend Integration

#### 1.1 Fix SocialDataSource URLs
**File**: `src/outfit-planner-ui/src/app/data/datasources/social.datasource.ts`

**Changes needed**:
```typescript
// Line 107: Change from /Polls to /api/polls
private readonly socialApiUrl = `${environment.baseUrl}/api/polls`;

// Line 108: Change from /Feed to /api/feed  
private readonly voteEngagementApiUrl = `${environment.baseUrl}/api/feed`;

// Line 109: Change from /Trending to /api/trending
private readonly trendingApiUrl = `${environment.baseUrl}/api/trending`;
```

**Impact**: All social features will connect to correct backend endpoints

#### 1.2 Verify FeedDataSource URLs
**File**: `src/outfit-planner-ui/src/app/data/datasources/feed.datasource.ts`

**Check and fix**:
```typescript
// Should be using /api/feed endpoints
private readonly feedApiUrl = `${environment.baseUrl}/api/feed`;
```

### Phase 2: Unified Social Module Structure

#### 2.1 Create Unified Social Page Component
**New File**: `src/outfit-planner-ui/src/app/presentation/pages/social/social-page.component.ts`

**Purpose**: Replace current 3-column layout with a tabbed interface that consolidates all social features

**Structure**:
```typescript
@Component({
  selector: 'app-social-page',
  standalone: true,
  imports: [...],
  templateUrl: './social-page.component.html',
  styleUrls: ['./social-page.component.scss']
})
export class SocialPageComponent implements OnInit {
  // Unified state management
  activeTab = signal<'feed' | 'polls' | 'trending' | 'create'>('feed');
  
  // Feed data
  feedPosts = signal<FeedPost[]>([]);
  
  // Polls data
  polls = signal<ValidationPoll[]>([]);
  
  // Trending data
  trendingOutfits = signal<TrendingOutfit[]>([]);
  
  // Create poll state
  isCreatingPoll = signal(false);
  
  // Navigation methods
  navigateToFeed() { /* ... */ }
  navigateToPolls() { /* ... */ }
  navigateToTrending() { /* ... */ }
  navigateToCreatePoll() { /* ... */ }
}
```

#### 2.2 Create Tabbed Navigation
**New File**: `src/outfit-planner-ui/src/app/presentation/pages/social/social-page.component.html`

**Structure**:
```html
<div class="social-page">
  <!-- Header with tabs -->
  <div class="social-header">
    <mat-tab-group [(selectedIndex)]="activeTabIndex">
      <mat-tab label="Feed" [routerLink]="['/social']"></mat-tab>
      <mat-tab label="Polls" [routerLink]="['/social/polls']"></mat-tab>
      <mat-tab label="Trending" [routerLink]="['/social/trending']"></mat-tab>
      <mat-tab label="Create" [routerLink]="['/social/create']"></mat-tab>
    </mat-tab-group>
  </div>
  
  <!-- Tab content -->
  <div class="social-content">
    <router-outlet></router-outlet>
  </div>
</div>
```

#### 2.3 Update Routing Configuration
**File**: `src/outfit-planner-ui/src/app/app.routes.ts`

**Changes**:
```typescript
// Replace current social routes with unified structure
{
  path: 'social',
  component: SocialPageComponent,
  canActivate: [authGuard],
  children: [
    { path: '', redirectTo: 'feed', pathMatch: 'full' },
    { path: 'feed', component: CommunityFeedComponent },
    { path: 'polls', component: PollsListComponent },
    { path: 'polls/:id', component: PollDetailComponent },
    { path: 'trending', component: TrendingOutfitsComponent },
    { path: 'create', component: CreatePollComponent },
  ]
}
```

### Phase 3: Implement Unified Components

#### 3.1 Community Feed Integration
**File**: `src/outfit-planner-ui/src/app/presentation/pages/social/components/feed/social-feed.component.ts`

**Features**:
- Consolidated feed with all post types (outfits, polls, questions)
- Infinite scroll pagination
- Real-time updates
- Like/comment/share functionality
- Filter by post type

#### 3.2 Poll System Integration
**File**: `src/outfit-planner-ui/src/app/presentation/pages/social/components/polls/social-polls.component.ts`

**Features**:
- List of all polls
- Active polls with voting
- Poll results visualization
- Create poll functionality
- Countdown timers

#### 3.3 Trending Outfits Integration
**File**: `src/outfit-planner-ui/src/app/presentation/pages/social/components/trending/social-trending.component.ts`

**Features**:
- Real-time trending algorithm
- Outfit cards with engagement stats
- Quick like/comment
- Navigation to outfit details

### Phase 4: Enhanced Social Features

#### 4.1 Follow System Implementation
**New File**: `src/outfit-planner-ui/src/app/presentation/pages/social/components/follow/social-follow.component.ts`

**Features**:
- Follow/unfollow users
- Following list
- Followers list
- Mutual connections
- User profiles with activity

#### 4.2 Outfit Post Creation UI
**Enhancement**: Add "Share to Feed" button to outfit detail page

**File**: `src/outfit-planner-ui/src/app/presentation/pages/outfit-detail/outfit-detail.component.ts`

**Add method**:
```typescript
shareToFeed(): void {
  const postRequest: CreatePostRequest = {
    outfitId: this.outfitId,
    caption: this.caption,
    tags: this.tags,
    isPublic: true
  };
  this.store.dispatch(SocialActions.createPost({ request: postRequest }));
}
```

#### 4.3 Real-time Updates
**New Service**: `src/outfit-planner-ui/src/app/core/services/social-realtime.service.ts`

**Features**:
- WebSocket connections for live updates
- Real-time notifications
- Live comment updates
- Live vote counts

### Phase 5: Performance Optimization

#### 5.1 Lazy Loading
**Update routing**:
```typescript
{
  path: 'social',
  component: SocialPageComponent,
  canActivate: [authGuard],
  children: [
    { path: 'feed', component: CommunityFeedComponent, loadChildren: () => import('./components/feed/feed.module').then(m => m.FeedModule) },
    // ... other lazy loaded routes
  ]
}
```

#### 5.2 Caching Strategy
**New Service**: `src/outfit-planner-ui/src/app/core/services/social-cache.service.ts`

**Features**:
- Feed post caching
- Poll results caching
- Trending outfits caching
- Cache invalidation on updates

## Implementation Timeline

### Week 1: Foundation
- [ ] Fix API URLs in SocialDataSource
- [ ] Fix API URLs in FeedDataSource
- [ ] Create unified social-page component
- [ ] Update routing configuration

### Week 2: Core Features
- [ ] Implement tabbed navigation
- [ ] Integrate community feed
- [ ] Integrate poll system
- [ ] Integrate trending outfits

### Week 3: Enhanced Features
- [ ] Implement follow system
- [ ] Add outfit post creation
- [ ] Add real-time updates
- [ ] Performance optimization

### Week 4: Testing & Polish
- [ ] End-to-end testing
- [ ] UI/UX improvements
- [ ] Performance testing
- [ ] Documentation

## API Integration Points

### Updated Endpoints after URL fixes:
- `GET /api/polls` - Get all polls
- `GET /api/polls/{id}` - Get specific poll
- `POST /api/polls` - Create poll
- `POST /api/polls/{id}/vote` - Cast vote
- `GET /api/feed` - Get feed posts
- `POST /api/feed` - Create post
- `POST /api/feed/{id}/like` - Add reaction
- `GET /api/feed/{id}/comments` - Get comments
- `POST /api/feed/{id}/comments` - Add comment
- `GET /api/trending` - Get trending outfits
- `GET /api/trending/outfits` - Get paginated trending

## Benefits of Unified Approach

1. **Maintainability**: All social features in one module
2. **Consistency**: Unified UX and design patterns
3. **Performance**: Shared state management and caching
4. **Scalability**: Easy to add new social features
5. **Testing**: Comprehensive testing of social features

## Risk Mitigation

1. **Backward Compatibility**: Maintain existing routes during transition
2. **Performance**: Lazy loading and caching for large datasets
3. **Error Handling**: Comprehensive error handling for all social features
4. **State Management**: Avoid state pollution with proper NgRx patterns

*Document Version: 1.0*
*Last Updated: April 2026*