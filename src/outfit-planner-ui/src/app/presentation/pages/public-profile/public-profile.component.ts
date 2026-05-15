import { Component, OnInit, inject, signal, CUSTOM_ELEMENTS_SCHEMA, ViewContainerRef, ComponentRef, ChangeDetectorRef } from '@angular/core';
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
import { CommentsModalComponent } from '../../components/shared/modals/comments-modal/comments-modal.component';
import { VotersModalComponent } from '../../components/shared/modals/voters-modal/voters-modal.component';

type TabType = 'activity' | 'followers' | 'following';

@Component({
  selector: 'app-public-profile',
  standalone: true,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTabsModule, CommentsModalComponent, VotersModalComponent],
  templateUrl: './public-profile.component.html',
  styleUrls: ['./public-profile.component.scss']
})
export class PublicProfileComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private userUseCases = inject(UserUseCases);
  private feedUseCases = inject(FeedUseCases);
  private followUseCases = inject(FollowUseCases);
  private viewContainerRef = inject(ViewContainerRef);
  private cdRef = inject(ChangeDetectorRef);

  userId = signal<string | null>(null);
  
  // Public profile data
  publicProfile = signal<PublicUserProfile | null>(null);
  
  // Stats
  stats = signal({
    outfits: 0,
    items: 0,
    followers: 0,
    following: 0
  });

  // Tab management
  activeTab = signal<TabType>('activity');

  // Follow status
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
  followedUserIds = signal<Set<string>>(new Set());

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
        const current = this.activityPosts();
        const newItems = result.items || [];
        this.activityPosts.set(reset ? newItems : [...current, ...newItems]);
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
        this.followersList.set([...this.followersList(), ...(result.items || [])]);
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
        this.followingList.set([...this.followingList(), ...(result.items || [])]);
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

  goToUserProfile(userId: string): void {
    this.router.navigate(['/profile', userId]);
  }

  onBack(): void {
    window.history.back();
  }

  formatCount(num: number): string {
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'k';
    return num.toString();
  }
  toggleFollowerListUser(userId: string, event: Event): void {
    event.stopPropagation();

    // Find the user in followers or following list   
    const follower = this.followersList().find(u => u.userId === userId);

    if (follower.isFollowing) {
      this.followUseCases.unfollowUser(userId).subscribe({
        next: () => {
          // Update the specific user in the list
          this.followersList.update(list =>
            list.map(u => u.userId === userId ? { ...u, isFollowing: false } : u)
          );
        }
      });
    } else {
      this.followUseCases.followUser(userId).subscribe({
        next: () => {
          // Update the specific user in the list
          this.followersList.update(list =>
            list.map(u => u.userId === userId ? { ...u, isFollowing: true } : u)
          );
        }
      });
    }
  }
  toggleFollowingListUser(userId: string, event: Event): void {
    event.stopPropagation();
    const following = this.followingList().find(u => u.userId === userId);
    if (following.isFollowing) {
      this.followUseCases.unfollowUser(userId).subscribe({
        next: () => {
          this.followingList.update(list =>
            list.map(u => u.userId === userId ? { ...u, isFollowing: false } : u)
          );
        }
      });
    } else {
      this.followUseCases.followUser(userId).subscribe({
        next: () => {
          this.followingList.update(list =>
            list.map(u => u.userId === userId ? { ...u, isFollowing: true } : u)
          );
        }
      });
    }
  }

  toggleVote(post:FeedPost,optionId: string, event: Event): void {
    event.stopPropagation();
    const poll = post?.poll;
    if (!poll) return;
    //if user voted before and click the same option, remove vote. Otherwise, vote on the new option and remove previous vote if exists
    if (poll.userVotedOptionId === optionId) {
      this.feedUseCases.removeVote(optionId).subscribe({
        next: () => this.activityPosts.update(posts => posts.map(p => {
          if (p.id === post.id && p.poll) {
            return {
              ...p,
              poll: {
                ...p.poll,
                userVotedOptionId: undefined,
                totalVotes: p.poll.totalVotes - 1,
                options: p.poll.options.map(o => o.id === optionId ? { ...o, voteCount: o.voteCount - 1 } : o)
              }
            };
          }          return p;
        }))
      });
    } else if (poll.userVotedOptionId!== optionId && poll.userVotedOptionId) {
      this.feedUseCases.voteOnPoll(poll.id, optionId).subscribe({
        next: () => this.activityPosts.update(posts => posts.map(p => {
          if (p.id === post.id && p.poll) {
            return {
              ...p,
              poll: {
                ...p.poll,
                userVotedOptionId: optionId,
                totalVotes: p.poll.totalVotes, // stays the same (removed old vote, added new vote)
                options: p.poll.options.map(o => o.id === optionId ? { ...o, voteCount: o.voteCount + 1 } : o.id === poll.userVotedOptionId ? { ...o, voteCount: o.voteCount - 1 } : o)
              }
            };
          }
          return p;
        }))
      });
    }
      else {
      this.feedUseCases.voteOnPoll(poll.id, optionId).subscribe({
        next: () => this.activityPosts.update(posts => posts.map(p => {
          if (p.id === post.id && p.poll) {
            return {
              ...p,
              poll: {
                ...p.poll,
                userVotedOptionId: optionId,
                totalVotes: p.poll.totalVotes + 1,
                options: p.poll.options.map(o => o.id === optionId ? { ...o, voteCount: o.voteCount + 1 } : o)
              }
            };
          }
          return p;
        }))
      });
    }
  }
  toggleReaction(post: FeedPost,event:Event): void {
    event.stopPropagation();
    const hasLiked = post.isLiked;
    const isOwner = post.isOwner;
    if (isOwner) return;
    
    if (hasLiked) {
      this.feedUseCases.removeReaction(post.id).subscribe({
        next: () => this.activityPosts.update(posts => posts.map(p => p.id === post.id ? { ...p, isLiked: false, likesCount: p.likesCount - 1 } : p))
      });
    } else {
      this.feedUseCases.addReaction(post.id).subscribe({
        next: () => this.activityPosts.update(posts => posts.map(p => p.id === post.id ? { ...p, isLiked: true, likesCount: p.likesCount + 1 } : p))
      });
    }
  }
  onTagClick(tag: string): void {
    //Tag for following   
    // this.router.navigate(['/feed'], { queryParams: { tag } });
  }
  getTimeAgo(dateString: Date): string {
    const now = new Date();
    const date = new Date(dateString);
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);
    const diffMonths = Math.floor(diffMs / 2592000000);
    const diffYears = Math.floor(diffMs / 31536000000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 30) return `${diffDays}d ago`;
    if (diffMonths < 12) return `${diffMonths}mo ago`;
    return `${diffYears}y ago`;
  }
  getOptionLabel(index: number): string {
    // For up to 26 options, label them A, B, C, etc. Beyond that, use "Option 1", "Option 2", etc.
    const labels = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    return labels[index] || `Option ${index + 1}`;
  }

  getvotePercentage(totalVotes: number, voteCount: number): string {
    if (totalVotes === 0) return '0%';
    const percentage = (voteCount / totalVotes) * 100;
    return `${percentage.toFixed(0)}%`;
  }

  isWinner(poll: any, optionId: string): boolean {
    if (!poll || poll.totalVotes === 0) return false;
    const maxVotes = Math.max(...poll.options.map((o: any) => o.voteCount));
    const option = poll.options.find((o: any) => o.id === optionId);
    return option ? option.voteCount === maxVotes : false;
  }

  openCommentsModal(postId: string): void {
    // Create the comments modal component dynamically
    const componentRef = this.viewContainerRef.createComponent(CommentsModalComponent);
    componentRef.instance.postId = postId;

    // Set callbacks to update commentsCount in the activity feed
    componentRef.instance.onCommentAdded = (id: string) => {
      this.activityPosts.update(posts => posts.map(p =>
        p.id === id ? { ...p, commentsCount: p.commentsCount + 1 } : p
      ));
    };
    componentRef.instance.onCommentDeleted = (id: string) => {
      this.activityPosts.update(posts => posts.map(p =>
        p.id === id ? { ...p, commentsCount: Math.max(0, p.commentsCount - 1) } : p
      ));
    };

    // Get the native element of the component
    const element = (componentRef.hostView as any).rootNodes[0] as HTMLElement;

    // Show SweetAlert with the component embedded
    Swal.fire({
      html: element,
      width: 600,
      showCloseButton: false,
      showConfirmButton: false,
      background: 'transparent',
      backdrop: true,
      didOpen: () => {
        // Trigger change detection multiple times to ensure signals are evaluated
        componentRef.changeDetectorRef.detectChanges();
        setTimeout(() => componentRef.changeDetectorRef.detectChanges(), 100);
      },
      willClose: () => {
        // Clean up component when modal closes
        componentRef.destroy();
      }
    });
  }

  openVotersModal(pollId: string, optionId?: string): void {
    // Create the voters modal component dynamically
    const componentRef = this.viewContainerRef.createComponent(VotersModalComponent);
    componentRef.instance.pollId = pollId;
    componentRef.instance.optionId = optionId;

    // Get the native element
    const element = (componentRef.hostView as any).rootNodes[0] as HTMLElement;

    // Show SweetAlert
    Swal.fire({
      html: element,
      width: 500,
      showCloseButton: false,
      showConfirmButton: false,
      background: 'transparent',
      backdrop: true,
      didOpen: () => {
        componentRef.changeDetectorRef.detectChanges();
      },
      willClose: () => {
        componentRef.destroy();
      }
    });
  }
}