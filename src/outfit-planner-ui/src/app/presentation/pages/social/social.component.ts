import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';

import { PollsActions } from '../../../core/state/polls/polls.actions';
import { TrendingActions } from '../../../core/state/trending/trending.actions';
import { FeedActions } from '../../../core/state/feed/feed.actions';
import { selectPollsLoading, selectRecentPoll, selectRecentPollComments, selectCommentsCursor, selectHasMoreComments, selectCommentsLoading, selectUserPolls } from '../../../core/state/polls/polls.selectors';
import { selectTrendingOutfits, selectTrendingLoading, selectTrendingTotalCount } from '../../../core/state/trending/trending.selectors';
import { selectPosts, selectHasMore, selectNextCursor, selectFeedLoading } from '../../../core/state/feed/feed.selectors';
import { Poll, CastVoteRequest, getTimeLeft, mapPollOptionToDisplayOption } from '../../../domain/entities/poll.entity';
import { TrendingOutfit } from '../../../domain/entities/outfit.entity';
import { FeedPost, PostType } from '../../../domain/entities/feed.entity';
import { PollCardComponent } from '../../components/poll-card/poll-card.component';

type FilterType = 'all' | 'outfits' | 'polls';

@Component({
  selector: 'app-social',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTabsModule, MatChipsModule, MatSnackBarModule, MatCardModule, PollCardComponent],
  templateUrl: './social.component.html',
  styleUrl: './social.component.scss'
})
export class SocialComponent implements OnInit {
  private store = inject(Store);
  router = inject(Router);
  private snackBar = inject(MatSnackBar);

  activeTab = signal(0);
  activeFilter = signal<FilterType>('all');

  filters: { value: FilterType; label: string }[] = [
    { value: 'all', label: 'All' },
    { value: 'outfits', label: 'Outfits' },
    { value: 'polls', label: 'Polls' },
  ];

  // Signals for trending pagination
  private trendingPage = signal(1);

  // Selectors as signals - public for template access
  private recentPollSignal = toSignal(this.store.select(selectRecentPoll), { initialValue: null as Poll | null });
  private recentCommentsSignal = toSignal(this.store.select(selectRecentPollComments), { initialValue: [] as any[] });
  private commentsCursorSignal = toSignal(this.store.select(selectCommentsCursor), { initialValue: null as string | null });
  readonly hasMoreCommentsSignal = toSignal(this.store.select(selectHasMoreComments), { initialValue: false });
  readonly commentsLoadingSignal = toSignal(this.store.select(selectCommentsLoading), { initialValue: false });
  readonly trendingOutfitsSignal = toSignal(this.store.select(selectTrendingOutfits), { initialValue: [] as TrendingOutfit[] });
  private trendingTotalCount = toSignal(this.store.select(selectTrendingTotalCount), { initialValue: 0 });
  private trendingLoadingSignal = toSignal(this.store.select(selectTrendingLoading), { initialValue: false });
  private pollsLoadingSignal = toSignal(this.store.select(selectPollsLoading), { initialValue: false });
  private postsSignal = toSignal(this.store.select(selectPosts), { initialValue: [] as FeedPost[] });
  readonly hasMoreSignal = toSignal(this.store.select(selectHasMore), { initialValue: false });
  readonly nextCursorSignal = toSignal(this.store.select(selectNextCursor), { initialValue: null as string | null });
  readonly feedLoadingSignal = toSignal(this.store.select(selectFeedLoading), { initialValue: false });

  loading = computed(() => this.pollsLoadingSignal() || this.trendingLoadingSignal() || this.feedLoadingSignal());

  // Trending pagination computed values
  trendingHasMore = computed(() => this.trendingOutfitsSignal().length < this.trendingTotalCount());
  trendingLoadingMore = computed(() => this.trendingLoadingSignal() && this.trendingPage() > 1);

  // Filtered posts computed
  filteredPosts = computed((): FeedPost[] => {
    const posts = this.postsSignal();
    const filter = this.activeFilter();
    switch (filter) {
      case 'outfits':
        return posts.filter((p: FeedPost) => p.postType === PostType.OutfitPost);
      case 'polls':
        return posts.filter((p: FeedPost) => p.postType === PostType.PollPost);
      default:
        return posts;
    }
  });

  featuredPoll = computed(() => {
    const activePoll = this.recentPollSignal();

    if (!activePoll) return null;

    return {
      ...activePoll,
      displayOptions: activePoll.options.map(mapPollOptionToDisplayOption),
      timeLeft: getTimeLeft(activePoll.expiresAt),
      formattedTime: new Date(activePoll.createdAt).toLocaleTimeString()
    };
  });

  // Get comments for the featured poll
  featuredPollComments = computed(() => this.recentCommentsSignal());

  // User polls count for social hub
  private userPollsSignal = toSignal(this.store.select(selectUserPolls), { initialValue: [] as Poll[] });
  userPollsCount = computed(() => this.userPollsSignal().length);

  ngOnInit(): void {
    this.store.dispatch(PollsActions.loadRecentPoll({ commentsPageSize: 20 }));
    this.store.dispatch(TrendingActions.loadTrending({ page: 1, pageSize: 20 }));
    this.store.dispatch(FeedActions.loadPosts({ pageSize: 20 }));
  }

  setFilter(filter: FilterType): void {
    this.activeFilter.set(filter);
  }

  vote(pollId: string, optionId: string): void {
    const voteRequest: CastVoteRequest = {
      optionId: optionId,
      rating: 5,
      isAnonymous: false
    };
    this.store.dispatch(PollsActions.vote({ pollId, request: voteRequest }));
  }

  onVoteOnPoll(pollId: string, optionId: string): void {
    const request: CastVoteRequest = {
      optionId: optionId,
      rating: 1,
      isAnonymous: false,
    };
    this.store.dispatch(PollsActions.vote({ pollId, request }));
  }

  onViewPollDetail(pollId: string): void {
    this.router.navigate(['/social/polls', pollId]);
  }

  viewPostDetail(post: FeedPost): void {
    if (post.postType === PostType.PollPost && post.pollId) {
      this.router.navigate(['/social/polls', post.pollId]);
    } else if (post.outfitId) {
      this.router.navigate(['/outfits', post.outfitId]);
    } else {
      this.router.navigate(['/social/feed']);
    }
  }

  loadMoreTrending(): void {
    if (this.trendingHasMore() && !this.trendingLoadingSignal()) {
      const nextPage = this.trendingPage() + 1;
      this.trendingPage.set(nextPage);
      this.store.dispatch(TrendingActions.loadTrending({ page: nextPage, pageSize: 20 }));
    }
  }

  loadMorePollComments(): void {
    if (this.hasMoreCommentsSignal() && !this.commentsLoadingSignal()) {
      const cursor = this.commentsCursorSignal();
      this.store.dispatch(PollsActions.loadMorePollComments({ cursor: cursor ?? undefined, pageSize: 20 }));
    }
  }

  loadMoreFeed(): void {
    if (this.hasMoreSignal() && !this.feedLoadingSignal()) {
      this.store.dispatch(FeedActions.loadPosts({ cursor: this.nextCursorSignal() ?? undefined, pageSize: 20 }));
    }
  }

  viewUserProfile(userId: string, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/social/profile', userId]);
  }

  getVotePercentage(poll: any, optionId: string): number {
    if (!poll || poll.totalVotes === 0) return 0;
    const option = poll.options.find((o: any) => o.id === optionId);
    return option ? Math.round((option.voteCount / poll.totalVotes) * 100) : 0;
  }

  copyLink(): void {
    const url = window.location.href;
    navigator.clipboard.writeText(url).then(() => {
      this.snackBar.open('Link copied to clipboard!', 'Close', {
        duration: 2000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom'
      });
    });
  }

  qrCodeUrl = computed(() => {
    const poll = this.featuredPoll();
    const id = poll?.id || 'default';
    return `https://api.qrserver.com/v1/create-qr-code/?size=180x180&data=outfitplanner.com/poll/${id}`;
  });

  currentPollLink = computed(() => {
    const poll = this.featuredPoll();
    return poll ? `outfitplanner.com/poll/${poll.id}` : 'outfitplanner.com/social';
  });
}