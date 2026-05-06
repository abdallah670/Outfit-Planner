import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';

import { UserActions } from '../../../core/state/user/user.actions';
import { selectSelectedPublicProfile, selectPublicProfileLoading } from '../../../core/state/user/user.selectors';
import { FeedPost } from '../../../domain/entities/feed.entity';
import { PublicUserProfile } from '../../../domain/entities/public-user-profile.entity';
import { FollowActions } from '../../../core/state/follow/follow.actions';
import { selectIsFollowingUser } from '../../../core/state/follow/follow.selectors';

type TabType = 'activity' | 'followers' | 'following';

@Component({
  selector: 'app-public-profile',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTabsModule],
  templateUrl: './public-profile.component.html',
  styleUrls: ['./public-profile.component.scss']
})
export class PublicProfileComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private store = inject(Store);

  userId = signal<string | null>(null);
  
  // State from store
  publicProfile = toSignal(this.store.select(selectSelectedPublicProfile));
  profileLoading = toSignal(this.store.select(selectPublicProfileLoading));
  isFollowing = toSignal(this.store.select(selectIsFollowingUser));

  // Stats computed from profile
  stats = computed(() => {
    const profile = this.publicProfile();
    return {
      outfits: profile?.outfitCount || 0,
      items: profile?.wardrobeItemCount || 0,
      followers: profile?.followersCount || 0,
      following: profile?.followingCount || 0
    };
  });

  // Tab management
  activeTab = signal<TabType>('activity');

  // Activity Feed (Local state for now or move to store if needed)
  activityPosts = signal<FeedPost[]>([]);
  activityLoading = signal(false);
  activityHasMore = signal(false);
  activityCursor = signal<string | null>(null);

  // Followers List
  followersList = signal<any[]>([]);
  followersLoading = signal(false);

  // Following List
  followingList = signal<any[]>([]);
  followingLoading = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('userId');
    if (id) {
      this.userId.set(id);
      this.store.dispatch(UserActions.loadUserProfile({ userId: id }));
      // Initialize follow status if needed, though selector should handle it
    }
  }

  setTab(tab: TabType): void {
    this.activeTab.set(tab);
    // Add logic to load data for tabs if not loaded
  }

  onFollowToggle(): void {
    const id = this.userId();
    if (!id) return;

    if (this.isFollowing()) {
      this.store.dispatch(FollowActions.unfollowUser({ targetUserId: id }));
    } else {
      this.store.dispatch(FollowActions.followUser({ targetUserId: id }));
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
