import { Component, OnInit, OnDestroy, signal, inject, ViewEncapsulation, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { FeedPost, PostType } from '../../../../domain/entities/feed.entity';
import { Subscription } from 'rxjs';

import { FeedUseCases } from '../../../../domain/usecases/feed.usecases';
import { FollowUseCases } from '../../../../domain/usecases/follow.usecases';
import { AuthService } from '../../../../core/services/auth.service';
import { SocialHubService, SocialPostDto } from '../../../../core/services/social-hub.service';
import { PostItemComponent } from '../../../components/shared/post-item/post-item.component';
import { TrendingOutfitsComponent } from '../trending-outfits/trending-outfits.component';
import { CursorPagedResult } from '../../../../domain/entities/response.entity';

type FeedTab = 'all' | 'following' | 'trending' | 'followers' | 'following-list' | 'my-posts';

interface FeedTabConfig {
  value: FeedTab;
  label: string;
  icon?: string;
}

@Component({
  selector: 'app-community-feed',
  standalone: true,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatIconModule,
    PostItemComponent,
    TrendingOutfitsComponent,
  ],
  templateUrl: './community-feed.component.html',
  styleUrl: './community-feed.component.scss',
  encapsulation: ViewEncapsulation.Emulated,
})
export class CommunityFeedComponent implements OnInit, OnDestroy {
  private feedUseCases = inject(FeedUseCases);
  private followUseCases = inject(FollowUseCases);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private socialHubService = inject(SocialHubService);

  activeTab = signal<FeedTab>('all');
  loading = signal(false);
  
  // Posts data
  posts = signal<FeedPost[]>([]);
  nextCursor = signal<string | null>(null);
  hasMore = signal(false);
  
  // User lists data (for followers/following tabs)
  userList = signal<any[]>([]);
  userListCursor = signal<string | null>(null);
  userListHasMore = signal(false);

  // My Posts Filter State
  myPostsFilter = signal<'all' | 'outfit' | 'poll'>('all');

  // SignalR subscriptions
  private signalRSubscriptions: Subscription[] = [];

  feedTabs: FeedTabConfig[] = [
    { value: 'all', label: 'All Posts', icon: 'layout-grid' },
    { value: 'following', label: 'Following', icon: 'users' },
    { value: 'trending', label: 'Trending', icon: 'trending-up' },
    { value: 'my-posts', label: 'My Posts', icon: 'user' },
    { value: 'followers', label: 'My Followers', icon: 'user-check' },
    { value: 'following-list', label: 'My Following', icon: 'user-plus' },
  ];

  ngOnInit(): void {
    // Setup SignalR real-time subscriptions
    this.setupSignalRSubscriptions();
    
    this.route.queryParams.subscribe(params => {
      const tabParam = params['tab'] as FeedTab;
      if (tabParam && this.feedTabs.some(t => t.value === tabParam)) {
        this.activeTab.set(tabParam);
      }
      
      const filterParam = params['filter'] as 'all' | 'outfit' | 'poll';
      if (filterParam && ['all', 'outfit', 'poll'].includes(filterParam)) {
        this.myPostsFilter.set(filterParam);
      }

      this.loadData(true);
    });
  }

  ngOnDestroy(): void {
    // Clean up SignalR subscriptions
    this.signalRSubscriptions.forEach(sub => sub.unsubscribe());
  }

  private setupSignalRSubscriptions(): void {
    // Subscribe to new posts from SignalR
    const newPostSub = this.socialHubService.newPost$.subscribe({
      next: (socialPost) => this.handleNewPost(socialPost)
    });
    this.signalRSubscriptions.push(newPostSub);

    // Subscribe to comment updates from SignalR
    const commentUpdateSub = this.socialHubService.commentUpdate$.subscribe({
      next: ({ postId, count }) => this.handleCommentUpdate(postId, count)
    });
    this.signalRSubscriptions.push(commentUpdateSub);

    // Subscribe to reaction updates from SignalR
    const reactionUpdateSub = this.socialHubService.reactionUpdate$.subscribe({
      next: ({ postId, count }) => this.handleReactionUpdate(postId, count)
    });
    this.signalRSubscriptions.push(reactionUpdateSub);
  }

  private handleNewPost(socialPost: SocialPostDto): void {
    // Convert SocialPostDto to FeedPost format
    const feedPost: FeedPost = {
      id: socialPost.id,
      userId: socialPost.userId,
      userName: socialPost.userName,
      userAvatarUrl: socialPost.userAvatarUrl || '',
      postType: socialPost.postType === 'Outfit' ? PostType.Outfit : PostType.Poll,
      caption: socialPost.caption,
      visibility: 2, // Public
      likesCount: socialPost.likesCount,
      commentsCount: socialPost.commentsCount,
      createdAt: new Date(socialPost.createdAt),
      isLiked: false,
      isOwner: false,
      outfitId: socialPost.outfitId,
      pollId: socialPost.pollId,
    };

    // Prepend post if it doesn't already exist
    this.posts.update(current => {
      if (current.some(p => p.id === feedPost.id)) {
        return current; // Already exists, don't add duplicate
      }
      return [feedPost, ...current];
    });
  }

  private handleCommentUpdate(postId: string, count: number): void {
    this.posts.update(posts => 
      posts.map(p => p.id === postId ? { ...p, commentsCount: count } : p)
    );
  }

  private handleReactionUpdate(postId: string, count: number): void {
    this.posts.update(posts => 
      posts.map(p => p.id === postId ? { ...p, likesCount: count } : p)
    );
  }

  setTab(tab: FeedTab): void {
    if (this.activeTab() === tab) return;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab },
      queryParamsHandling: 'merge'
    });
  }

  loadData(reset = false): void {
    const tab = this.activeTab();
    this.loading.set(true);

    if (reset) {
      this.posts.set([]);
      this.userList.set([]);
      this.nextCursor.set(null);
      this.userListCursor.set(null);
    }

    if (tab === 'all') {
      this.loadAllPosts(reset);
    } else if (tab === 'following') {
      this.loadFollowingPosts(reset);
    } else if (tab === 'my-posts') {
      this.loadMyPosts(reset);
    } else if (tab === 'followers') {
      this.loadFollowers(reset);
    } else if (tab === 'following-list') {
      this.loadFollowingList(reset);
    } else if (tab === 'trending') {
      // Trending is handled entirely by TrendingOutfitsComponent
      this.loading.set(false);
    }
  }

  private loadAllPosts(reset: boolean): void {
    this.feedUseCases.getFeedPosts(this.nextCursor() || undefined, 10).subscribe({
      next: (result) => this.handlePostsResult(result, reset),
      error: () => this.loading.set(false)
    });
  }

  private loadFollowingPosts(reset: boolean): void {
    this.feedUseCases.getFeedPosts(this.nextCursor() || undefined, 10, 'Public', 'recent', 'All', true).subscribe({
      next: (result) => this.handlePostsResult(result, reset),
      error: () => this.loading.set(false)
    });
  }

  private loadMyPosts(reset: boolean): void {
    const filter = this.myPostsFilter();
    let postType: string | undefined = undefined;
    if (filter === 'outfit') {
      postType = 'Outfit';
    } else if (filter === 'poll') {
      postType = 'Poll';
    }

    this.feedUseCases.getMyPosts(this.nextCursor() || undefined, 10, postType).subscribe({
      next: (result) => this.handlePostsResult(result, reset),
      error: () => this.loading.set(false)
    });
  }

  setMyPostsFilter(filter: 'all' | 'outfit' | 'poll'): void {
    if (this.myPostsFilter() === filter) return;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { filter },
      queryParamsHandling: 'merge'
    });
  }

  private loadFollowers(reset: boolean): void {
    const currentUserId = this.authService.currentUser()?.id;
    if (!currentUserId) return;

    this.followUseCases.getFollowers(currentUserId, this.userListCursor() || undefined, 10).subscribe({
      next: (result) => this.handleUserListResult(result, reset),
      error: () => this.loading.set(false)
    });
  }

  private loadFollowingList(reset: boolean): void {
    const currentUserId = this.authService.currentUser()?.id;
    if (!currentUserId) return;

    this.followUseCases.getFollowing(currentUserId, this.userListCursor() || undefined, 10).subscribe({
      next: (result) => this.handleUserListResult(result, reset),
      error: () => this.loading.set(false)
    });
  }

  private handlePostsResult(result: CursorPagedResult<FeedPost>, reset: boolean): void {
    this.posts.update(current => reset ? result.items : [...current, ...result.items]);
    this.nextCursor.set(result.nextCursor);
    this.hasMore.set(result.hasMore);
    this.loading.set(false);
  }

  private handleUserListResult(result: CursorPagedResult<any>, reset: boolean): void {
    this.userList.update(current => reset ? result.items : [...current, ...result.items]);
    this.userListCursor.set(result.nextCursor);
    this.userListHasMore.set(result.hasMore);
    this.loading.set(false);
  }

  loadMore(): void {
    if (this.loading()) return;
    this.loadData(false);
  }

  onPostUpdated(updatedPost: FeedPost): void {
    this.posts.update(posts => posts.map(p => {
      if (p.id !== updatedPost.id) return p;
      return {
        ...updatedPost,
        // Preserve server-authoritative counts — SignalR is the source of truth for these
        likesCount: p.likesCount,
        commentsCount: p.commentsCount,
      };
    }));
  }

  onPostDeleted(postId: string): void {
    this.posts.update(posts => posts.filter(p => p.id !== postId));
  }

  toggleUserFollow(userId: string, event: Event): void {
    event.stopPropagation();
    const user = this.userList().find(u => u.userId === userId);
    if (!user) return;

    if (user.isFollowing) {
      this.followUseCases.unfollowUser(userId).subscribe({
        next: () => this.updateUserInList(userId, { isFollowing: false })
      });
    } else {
      this.followUseCases.followUser(userId).subscribe({
        next: () => this.updateUserInList(userId, { isFollowing: true })
      });
    }
  }

  private updateUserInList(userId: string, changes: any): void {
    this.userList.update(list => list.map(u => u.userId === userId ? { ...u, ...changes } : u));
  }

  openCreatePost(): void {
    this.router.navigate(['/social/create-post']);
  }

  openCreatePoll(): void {
    this.router.navigate(['/social/create-poll']);
  }
}