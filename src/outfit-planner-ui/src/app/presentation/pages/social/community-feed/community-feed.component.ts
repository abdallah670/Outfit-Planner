import { Component, OnInit, signal, computed, inject, ViewEncapsulation, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { FeedPost, PostType, Visibility } from '../../../../domain/entities/feed.entity';

import { FeedUseCases } from '../../../../domain/usecases/feed.usecases';
import { TrendingUseCases } from '../../../../domain/usecases/trending.usecases';
import { FollowUseCases } from '../../../../domain/usecases/follow.usecases';
import { AuthService } from '../../../../core/services/auth.service';
import { PostItemComponent } from '../../../components/shared/post-item/post-item.component';
import { CursorPagedResult } from '../../../../domain/entities/response.entity';
import { TrendingOutfit } from '../../../../domain/entities/outfit.entity';

type FeedTab = 'all' | 'following' | 'trending' | 'followers' | 'following-list';

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
  ],
  templateUrl: './community-feed.component.html',
  styleUrl: './community-feed.component.scss',
  encapsulation: ViewEncapsulation.Emulated,
})
export class CommunityFeedComponent implements OnInit {
  private feedUseCases = inject(FeedUseCases);
  private trendingUseCases = inject(TrendingUseCases);
  private followUseCases = inject(FollowUseCases);
  private authService = inject(AuthService);
  private router = inject(Router);

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

  feedTabs: FeedTabConfig[] = [
    { value: 'all', label: 'All Posts', icon: 'layout-grid' },
    { value: 'following', label: 'Following', icon: 'users' },
    { value: 'trending', label: 'Trending', icon: 'trending-up' },
    { value: 'followers', label: 'My Followers', icon: 'user-check' },
    { value: 'following-list', label: 'My Following', icon: 'user-plus' },
  ];

  ngOnInit(): void {
    this.loadData(true);
  }

  setTab(tab: FeedTab): void {
    if (this.activeTab() === tab) return;
    this.activeTab.set(tab);
    this.loadData(true);
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
    } else if (tab === 'trending') {
      this.loadTrendingPosts(reset);
    } else if (tab === 'followers') {
      this.loadFollowers(reset);
    } else if (tab === 'following-list') {
      this.loadFollowingList(reset);
    }
  }

  private loadAllPosts(reset: boolean): void {
    this.feedUseCases.getFeedPosts(this.nextCursor() || undefined, 10).subscribe({
      next: (result) => this.handlePostsResult(result, reset),
      error: () => this.loading.set(false)
    });
  }

  private loadFollowingPosts(reset: boolean): void {
    // If backend supports filtering by "following", we can use it.
    this.feedUseCases.getFeedPosts(this.nextCursor() || undefined, 10, 'Public', 'recent', 'All', true).subscribe({
      next: (result) => this.handlePostsResult(result, reset),
      error: () => this.loading.set(false)
    });

  }

  private loadTrendingPosts(reset: boolean): void {
    const cursor = reset ? undefined : (this.nextCursor() ?? undefined);
    this.trendingUseCases.getTrendingOutfits(cursor, 10).subscribe({

      next: (result) => {
        const mappedPosts: FeedPost[] = result.items.map(item => this.mapTrendingToPost(item));
        this.posts.update(current => reset ? mappedPosts : [...current, ...mappedPosts]);
        this.nextCursor.set(result.nextCursor);
        this.hasMore.set(result.hasMore);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
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

  private mapTrendingToPost(item: TrendingOutfit): FeedPost {
    return {
      id: item.id,
      userId: item.userId,
      userName: item.userName,
      userAvatarUrl: item.userAvatar,
      createdAt: item.createdAt,
      postType: PostType.Outfit,
      caption: item.userName + "'s trending outfit",
      likesCount: item.likes,
      commentsCount: item.comments,
      isLiked: item.isliked,
      isOwner: item.isowner,
      outfitId: item.id,
      outfit: {
        id: item.id,
        userId: item.userId,
        name: item.userName + "'s outfit",
        imageUrl: item.imageUrl,
        items: [],
        occasion: 'Social' as any,
        suitableWeather: {} as any,
        season: 'AllSeason' as any,
        comfortLevel: 0,
        styleRating: 0,
        createdAt: item.createdAt,
        lastWorn: item.createdAt,
        timesWorn: 0,
        status: 'active' as any,
        feedback: []
      },
      tags: [],
      visibility: Visibility.Public,

    
    };
  }

  loadMore(): void {
    if (this.loading()) return;
    this.loadData(false);
  }

  onPostUpdated(updatedPost: FeedPost): void {
    this.posts.update(posts => posts.map(p => p.id === updatedPost.id ? updatedPost : p));
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