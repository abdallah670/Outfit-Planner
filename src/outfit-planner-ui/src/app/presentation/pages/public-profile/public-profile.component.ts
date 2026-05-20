import { Component, OnInit, inject, signal, CUSTOM_ELEMENTS_SCHEMA, ViewContainerRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import Swal from 'sweetalert2';
import { UserUseCases } from '../../../domain/usecases/user.usecases';
import { FeedUseCases } from '../../../domain/usecases/feed.usecases';
import { FollowUseCases } from '../../../domain/usecases/follow.usecases';
import { PublicUserProfile } from '../../../domain/entities/public-user-profile.entity';
import { CursorPagedResult } from '../../../domain/entities/response.entity';
import { FeedPost } from '../../../domain/entities/feed.entity';
import { PostItemComponent } from '../../components/shared/post-item/post-item.component';

type TabType = 'activity' | 'followers' | 'following';

@Component({
  selector: 'app-public-profile',
  standalone: true,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTabsModule, PostItemComponent],
  templateUrl: './public-profile.component.html',
  styleUrls: ['./public-profile.component.scss']
})
export class PublicProfileComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private userUseCases = inject(UserUseCases);
  private feedUseCases = inject(FeedUseCases);
  private followUseCases = inject(FollowUseCases);

  userId = signal<string | null>(null);
  publicProfile = signal<PublicUserProfile | null>(null);
  
  stats = signal({
    outfits: 0,
    items: 0,
    followers: 0,
    following: 0
  });

  activeTab = signal<TabType>('activity');
  followLoading = signal(false);

  // Activity Feed
  activityPosts = signal<FeedPost[]>([]);
  activityLoading = signal(false);
  activityHasMore = signal(false);
  activityCursor = signal<string | null>(null);

  // Followers List
  followersList = signal<any[]>([]);
  followersLoading = signal(false);
  followersHasMore = signal(false);
  followersCursor = signal<string | null>(null);

  // Following List
  followingList = signal<any[]>([]);
  followingLoading = signal(false);
  followingHasMore = signal(false);
  followingCursor = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('userId');
    if (id) {
      this.userId.set(id);
      this.loadPublicProfile(id);
      this.setTab('activity');
    }
  }

  private loadPublicProfile(userId: string): void {
    this.userUseCases.getPublicProfile(userId).subscribe({
      next: (profile) => {
        this.publicProfile.set(profile);
        this.stats.set({
          outfits: profile.outfitCount,
          items: profile.wardrobeItemCount,
          followers: profile.followersCount,
          following: profile.followingCount
        });
      },
      error: (err) => console.error('Failed to load public profile', err)
    });
  }

  setTab(tab: TabType): void {
    this.activeTab.set(tab);
    if (tab === 'activity' && this.activityPosts().length === 0) {
      this.loadActivityFeed(true);
    } else if (tab === 'followers' && this.followersList().length === 0) {
      this.loadFollowers();
    } else if (tab === 'following' && this.followingList().length === 0) {
      this.loadFollowing();
    }
  }

  loadActivityFeed(reset = false): void {
    const userId = this.userId();
    if (!userId) return;
    
    if (reset) {
      this.activityCursor.set(null);
      this.activityPosts.set([]);
    }
    
    this.activityLoading.set(true);
    this.feedUseCases.getUserFeed(userId, this.activityCursor() || undefined, 20).subscribe({
      next: (result: CursorPagedResult<FeedPost>) => {
        const newItems = result.items || [];
        this.activityPosts.update(current => reset ? newItems : [...current, ...newItems]);
        this.activityCursor.set(result.nextCursor);
        this.activityHasMore.set(result.hasMore);
        this.activityLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load activity', err);
        this.activityLoading.set(false);
      }
    });
  }

  loadMoreActivity(): void {
    if (this.activityHasMore() && this.activityCursor() && !this.activityLoading()) {
      this.loadActivityFeed(false);
    }
  }

  loadFollowers(): void {
    const userId = this.userId();
    if (!userId) return;
    
    this.followersLoading.set(true);
    this.followUseCases.getFollowers(userId, undefined, 20).subscribe({
      next: (result: CursorPagedResult<any>) => {
        this.followersList.set(result.items || []);
        this.followersCursor.set(result.nextCursor);
        this.followersHasMore.set(result.hasMore);
        this.followersLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load followers', err);
        this.followersLoading.set(false);
      }
    });
  }

  loadMoreFollowers(): void {
    const userId = this.userId();
    if (!userId || !this.followersCursor() || this.followersLoading()) return;
    
    this.followersLoading.set(true);
    this.followUseCases.getFollowers(userId, this.followersCursor() || undefined, 20).subscribe({
      next: (result: CursorPagedResult<any>) => {
        this.followersList.update(current => [...current, ...(result.items || [])]);
        this.followersCursor.set(result.nextCursor);
        this.followersHasMore.set(result.hasMore);
        this.followersLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load more followers', err);
        this.followersLoading.set(false);
      }
    });
  }

  loadFollowing(): void {
    const userId = this.userId();
    if (!userId) return;
    
    this.followingLoading.set(true);
    this.followUseCases.getFollowing(userId, undefined, 20).subscribe({
      next: (result: CursorPagedResult<any>) => {
        this.followingList.set(result.items || []);
        this.followingCursor.set(result.nextCursor);
        this.followingHasMore.set(result.hasMore);
        this.followingLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load following', err);
        this.followingLoading.set(false);
      }
    });
  }

  loadMoreFollowing(): void {
    const userId = this.userId();
    if (!userId || !this.followingCursor() || this.followingLoading()) return;
    
    this.followingLoading.set(true);
    this.followUseCases.getFollowing(userId, this.followingCursor() || undefined, 20).subscribe({
      next: (result: CursorPagedResult<any>) => {
        this.followingList.update(current => [...current, ...(result.items || [])]);
        this.followingCursor.set(result.nextCursor);
        this.followingHasMore.set(result.hasMore);
        this.followingLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load more following', err);
        this.followingLoading.set(false);
      }
    });
  }

  onFollowToggle(): void {
    const id = this.userId();
    if (!id) return;

    if (this.publicProfile()?.isFollowing) {
      this.followUseCases.unfollowUser(id).subscribe({
        next: () => {
          this.publicProfile.update((profile) => ({ ...profile!, isFollowing: false }));
          this.stats.update(s => ({ ...s, followers: s.followers - 1 }));
        }
      });
    } else {
      this.followUseCases.followUser(id).subscribe({
        next: () => {
          this.publicProfile.update((profile) => ({ ...profile!, isFollowing: true }));
          this.stats.update(s => ({ ...s, followers: s.followers + 1 }));
        }
      });
    }
  }

  onPostUpdated(updatedPost: FeedPost): void {
    this.activityPosts.update(posts => posts.map(p => p.id === updatedPost.id ? updatedPost : p));
  }

  onPostDeleted(postId: string): void {
    this.activityPosts.update(posts => posts.filter(p => p.id !== postId));
  }

  toggleUserFollow(userId: string, event: Event, listType: 'followers' | 'following'): void {
    event.stopPropagation();
    const list = listType === 'followers' ? this.followersList : this.followingList;
    const user = list().find(u => u.userId === userId);
    if (!user) return;

    if (user.isFollowing) {
      this.followUseCases.unfollowUser(userId).subscribe({
        next: () => {
          list.update(items => items.map(u => u.userId === userId ? { ...u, isFollowing: false } : u));
        }
      });
    } else {
      this.followUseCases.followUser(userId).subscribe({
        next: () => {
          list.update(items => items.map(u => u.userId === userId ? { ...u, isFollowing: true } : u));
        }
      });
    }
  }

  onBack(): void {
    window.history.back();
  }

  formatCount(num: number): string {
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'k';
    return num.toString();
  }
}