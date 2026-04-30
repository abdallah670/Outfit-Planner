import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { Store } from '@ngrx/store';
import { FollowActions } from '../../../../core/state/follow/follow.actions';
import { selectIsFollowingUser } from '../../../../core/state/follow/follow.selectors';

@Component({
  selector: 'app-social-profile',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTabsModule],
  templateUrl: './social-profile.component.html',
  styleUrl: './social-profile.component.scss'
})
export class SocialProfileComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private store = inject(Store);

  userId = signal<string | null>(null);
  
  // Profile Data
  displayName = signal<string>('Emily Chen');
  handle = signal<string>('@emily_styles');
  bio = signal<string>('Minimalist fashion lover. Mixing neutral colors & timeless pieces. ☕✨');
  avatarUrl = signal<string>('https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150');
  
  // Stats
  stats = signal({
    outfits: 38,
    items: 142,
    followers: '1.2k',
    following: '450'
  });

  // Tab management
  activeTab = signal<'outfits' | 'activity' | 'followers' | 'following'>('outfits');

  // Follow state
  isFollowing = signal(false);

  // Outfits Grid (Mock data as per image)
  outfits = signal([
    { id: '1', title: 'Weekend Coffee Run', category: 'Casual', season: 'Spring', items: 3, likes: 245, imageUrl: 'https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=500' },
    { id: '2', title: 'Office Meeting Look', category: 'Business Casual', season: 'Fall', items: 4, likes: 182, imageUrl: 'https://images.unsplash.com/photo-1539109132381-31a057ad7c73?w=500' },
    { id: '3', title: 'Dinner Date', category: 'Evening', season: 'Summer', items: 2, likes: 310, imageUrl: 'https://images.unsplash.com/photo-1595777457583-95e059d581b8?w=500' },
    { id: '4', title: 'Sunday Errands', category: 'Sporty', season: 'Spring', items: 3, likes: 124, imageUrl: 'https://images.unsplash.com/photo-1483985988355-763728e1935b?w=500' },
    { id: '5', title: 'Gallery Opening', category: 'Formal', season: 'Winter', items: 4, likes: 456, imageUrl: 'https://images.unsplash.com/photo-1550614000-4895a10e1bfd?w=500' },
    { id: '6', title: 'Airport Look', category: 'Travel', season: 'Fall', items: 5, likes: 289, imageUrl: 'https://images.unsplash.com/photo-1516762689617-e1cffcef479d?w=500' }
  ]);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('userId');
    if (id) {
      this.userId.set(id);
      
      // Check follow status
      this.store.dispatch(FollowActions.checkFollowStatus({ userId: id }));
      this.store.select(selectIsFollowingUser(id)).subscribe(status => {
        this.isFollowing.set(status);
      });
      
      // In a real app, we would load the public profile here
      // this.store.dispatch(UserActions.loadPublicProfile({ userId: id }));
    }
  }

  setTab(tab: 'outfits' | 'activity' | 'followers' | 'following'): void {
    this.activeTab.set(tab);
  }

  onFollowToggle(): void {
    const id = this.userId();
    if (!id) return;

    if (this.isFollowing()) {
      this.store.dispatch(FollowActions.unfollowUser({ userId: id }));
      this.isFollowing.set(false);
    } else {
      this.store.dispatch(FollowActions.followUser({ userId: id }));
      this.isFollowing.set(true);
    }
  }

  onBack(): void {
    window.history.back();
  }
}
