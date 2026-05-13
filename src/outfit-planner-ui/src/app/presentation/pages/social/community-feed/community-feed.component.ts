import { Component, OnInit, signal, computed, inject, ViewEncapsulation, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { FeedActions } from '../../../../core/state/feed/feed.actions';
import { PollsActions } from '../../../../core/state/polls/polls.actions';
import {
  selectPosts,
  selectNextCursor,
  selectHasMore,
  selectFeedLoading,
} from '../../../../core/state/feed/feed.selectors';
import { FeedPost, PostType, PostComment } from '../../../../domain/entities/feed.entity';
import { CastVoteRequest, Poll, PollOption } from '../../../../domain/entities/poll.entity';
import { MatIconModule } from '@angular/material/icon';
import { toSignal } from '@angular/core/rxjs-interop';
import { TrendingOutfitsComponent } from '../trending-outfits/trending-outfits.component';

type FeedTab = 'all' | 'following' | 'trending' | 'my-outfits' | 'my-polls' | 'my-followers' | 'my-following';

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
    TrendingOutfitsComponent,
  ],
  templateUrl: './community-feed.component.html',
  styleUrl: './community-feed.component.scss',
  encapsulation: ViewEncapsulation.Emulated,
})
export class CommunityFeedComponent implements OnInit {
  private store = inject(Store);
  private router = inject(Router);

  readonly PostType = PostType;

  activeTab = signal<FeedTab>('all');

  // Feed tabs — populated from API / config
  feedTabs: FeedTabConfig[] = [
    { value: 'all', label: 'All' },
    { value: 'following', label: 'Following' },
    { value: 'trending', label: 'Trending', icon: 'trending_up' },
    { value: 'my-outfits', label: 'My Outfit Posts' },
    { value: 'my-polls', label: 'My Polls' },
    { value: 'my-followers', label: 'My Followers' },
    { value: 'my-following', label: 'My Following' },
  ];

  // Feed posts
  private allPostsSignal = toSignal(this.store.select(selectPosts), { initialValue: [] as FeedPost[] }) as () => FeedPost[];
  loading = toSignal(this.store.select(selectFeedLoading), { initialValue: false });
  nextCursor = toSignal(this.store.select(selectNextCursor), { initialValue: null as string | null });
  hasMore = toSignal(this.store.select(selectHasMore), { initialValue: false });

  userAvatar = signal<string>('');

  displayPosts = computed((): FeedPost[] => {
    return this.allPostsSignal();
  });

  isTrending = computed(() => this.activeTab() === 'trending');

  ngOnInit(): void {
    this.store.dispatch(FeedActions.loadPosts({ pageSize: 20 }));
  }

  setTab(tab: FeedTab): void {
    this.activeTab.set(tab);

    // For trending tab, no need to load data here — TrendingOutfitsComponent handles it
    if (tab === 'trending') {
      return;
    }

    // For my-outfits and my-polls, send the postType filter to the backend
    // NOTE: Must match C# PostType enum values: Poll = 0, Outfit = 1
    const postType = tab === 'my-outfits' ? 'Outfit' : tab === 'my-polls' ? 'Poll' : undefined;
    this.store.dispatch(FeedActions.loadPosts({ postType, pageSize: 20, cursor: undefined }));
  }

  viewPostDetail(post: FeedPost): void {
    if (post.postType === PostType.Poll && post.pollId) {
      this.router.navigate(['/social/polls', post.pollId]);
    } else if (post.outfitId) {
      this.router.navigate(['/outfits', post.outfitId]);
    } else {
      this.router.navigate(['/social/feed']);
    }
  }

  toggleReaction(post: FeedPost, event: Event): void {
    event.stopPropagation();
    if (post.userReaction) {
      this.store.dispatch(FeedActions.removeReaction({ postId: post.id }));
    } else {
      this.store.dispatch(FeedActions.addReaction({ postId: post.id }));
    }
  }

  onVoteOnPoll(pollId: string | undefined, optionId: string): void {
    if (!pollId) return;
    const request: CastVoteRequest = {
      optionId: optionId
    };
    this.store.dispatch(PollsActions.vote({ pollId, request }));
  }

  onViewPollDetail(pollId: string | undefined): void {
    if (!pollId) return;
    this.router.navigate(['/social/polls', pollId]);
  }

  formatCount(count: number): string {
    if (count >= 1000000) {
      return (count / 1000000).toFixed(1).replace(/\.0$/, '') + 'M';
    }
    if (count >= 1000) {
      return (count / 1000).toFixed(1).replace(/\.0$/, '') + 'k';
    }
    return count.toString();
  }

  getOptionLabel(index: number): string {
    return String.fromCharCode(65 + index);
  }

  getVotePercent(option: PollOption, poll?: Poll): number {
    if (!poll?.totalVotes || poll.totalVotes === 0) return 0;
    return Math.round((option.voteCount / poll.totalVotes) * 100);
  }

  isWinningOption(option: PollOption, poll: Poll): boolean {
    if (!poll.options || poll.options.length === 0) return false;
    const maxVotes = Math.max(...poll.options.map(opt => opt.voteCount));
    return option.voteCount === maxVotes && option.voteCount > 0;
  }

  getTimeAgo(date: Date | string | undefined): string {
    if (!date) return '';
    const now = new Date();
    const then = new Date(date);
    const diffMs = now.getTime() - then.getTime();
    const diffSecs = Math.floor(diffMs / 1000);
    const diffMins = Math.floor(diffSecs / 60);
    const diffHrs = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHrs / 24);

    if (diffSecs < 60) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHrs < 24) return `${diffHrs}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    if (diffDays < 30) {
      const weeks = Math.floor(diffDays / 7);
      return `${weeks}w ago`;
    }
    return then.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  // Comment UI state
  expandedCommentPostId = signal<string | null>(null);
  commentText = signal('');
  commentsCache = signal<{ [postId: string]: { items: PostComment[]; nextCursor: string | null; hasMore: boolean } }>({});

  toggleComments(postId: string): void {
    if (this.expandedCommentPostId() === postId) {
      this.expandedCommentPostId.set(null);
    } else {
      this.expandedCommentPostId.set(postId);
      if (!this.commentsCache()[postId]) {
        this.store.dispatch(FeedActions.loadComments({ postId, pageSize: 10 }));
      }
    }
  }

  submitComment(postId: string): void {
    const text = this.commentText().trim();
    if (!text) return;
    this.store.dispatch(FeedActions.addComment({ postId, content: text }));
    this.commentText.set('');
  }

  loadMore(): void {
    if (this.hasMore() && this.nextCursor()) {
      this.store.dispatch(FeedActions.loadPosts({ cursor: this.nextCursor() ?? undefined, pageSize: 20 }));
    }
  }

  openCreatePost(): void {
    this.router.navigate(['/social/create-outfit-post']);
  }

  openCreatePoll(): void {
    this.router.navigate(['/social/create-poll']);
  }
}