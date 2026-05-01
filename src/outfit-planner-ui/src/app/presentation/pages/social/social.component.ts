import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';

import { PollsActions } from '../../../core/state/polls/polls.actions';
import { TrendingActions } from '../../../core/state/trending/trending.actions';
import { selectPollsLoading, selectRecentPoll, selectRecentPollComments, selectCommentsCursor, selectHasMoreComments, selectCommentsLoading } from '../../../core/state/polls/polls.selectors';
import { selectTrendingOutfits, selectTrendingLoading, selectTrendingTotalCount } from '../../../core/state/trending/trending.selectors';
import { Poll, CastVoteRequest, getTimeLeft, mapPollOptionToDisplayOption } from '../../../domain/entities/poll.entity';
import { TrendingOutfit } from '../../../domain/entities/outfit.entity';

@Component({
  selector: 'app-social',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTabsModule, MatChipsModule, MatSnackBarModule],
  templateUrl: './social.component.html',
  styleUrl: './social.component.scss'
})
export class SocialComponent implements OnInit {
  private store = inject(Store);
  router = inject(Router);
  private snackBar = inject(MatSnackBar);

  activeTab = signal(0);

  // Signals for trending pagination
  private trendingPage = signal(1);

  // Selectors as signals
  private recentPollSignal = toSignal(this.store.select(selectRecentPoll), { initialValue: null as Poll | null });
  private recentCommentsSignal = toSignal(this.store.select(selectRecentPollComments), { initialValue: [] as any[] });
  private commentsCursorSignal = toSignal(this.store.select(selectCommentsCursor), { initialValue: null as string | null });
  private hasMoreCommentsSignal = toSignal(this.store.select(selectHasMoreComments), { initialValue: false });
  private commentsLoadingSignal = toSignal(this.store.select(selectCommentsLoading), { initialValue: false });
  private trendingOutfitsSignal = toSignal(this.store.select(selectTrendingOutfits), { initialValue: [] as TrendingOutfit[] });
  private trendingTotalCount = toSignal(this.store.select(selectTrendingTotalCount), { initialValue: 0 });
  private trendingLoadingSignal = toSignal(this.store.select(selectTrendingLoading), { initialValue: false });
  private pollsLoadingSignal = toSignal(this.store.select(selectPollsLoading), { initialValue: false });

  loading = computed(() => this.pollsLoadingSignal() || this.trendingLoadingSignal());

  // Trending pagination computed values
  trendingHasMore = computed(() => this.trendingOutfitsSignal().length < this.trendingTotalCount());
  trendingLoadingMore = computed(() => this.trendingLoadingSignal() && this.trendingPage() > 1);

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

  ngOnInit(): void {
    this.store.dispatch(PollsActions.loadRecentPoll({ commentsPageSize: 20 }));
    this.store.dispatch(TrendingActions.loadTrending({ page: 1, pageSize: 20 }));
  }

  vote(pollId: string, optionId: string): void {
    const voteRequest: CastVoteRequest = {
      optionId: optionId,
      rating: 5,
      isAnonymous: false
    };
    this.store.dispatch(PollsActions.vote({ pollId, request: voteRequest }));
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
