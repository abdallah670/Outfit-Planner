import { Component, OnInit, signal, computed, inject, Signal } from '@angular/core';
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
import {
  selectPolls,
  selectPollsLoading,
} from '../../../core/state/polls/polls.selectors';
import {
  selectTrendingOutfits,
  selectTrendingLoading,
} from '../../../core/state/trending/trending.selectors';
import {
  Poll,
  CastVoteRequest,
  PollStatus,
  PollOption,
} from '../../../domain/entities/poll.entity';
import { TrendingOutfit } from '../../../domain/entities/outfit.entity';

/**
 * Helper to map PollOption from ValidationPoll to local interface
 */
function mapPollOptionToDisplayOption(option: PollOption): { id: string; imageUrl: string; label: string; votes: number } {
  return {
    id: option.id,
    imageUrl: option.outfitThumbnail || '',
    label: option.description,
    votes: option.voteCount,
  };
}

/**
 * Calculate time remaining string from expiresAt Date
 */
function getTimeLeft(expiresAt: Date | string): string {
  const expiry = new Date(expiresAt);
  const now = new Date();
  const diff = expiry.getTime() - now.getTime();
  
  if (diff <= 0) {
    return 'Expired';
  }
  
  const hours = Math.floor(diff / (1000 * 60 * 60));
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
  
  if (hours > 24) {
    const days = Math.floor(hours / 24);
    return `${days}d left`;
  }
  
  if (hours > 0) {
    return `${hours}h ${minutes}m left`;
  }
  
  return `${minutes}m left`;
}

@Component({
  selector: 'app-social',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTabsModule, MatChipsModule, MatSnackBarModule],
  templateUrl: './social.component.html',
  styleUrl: './social.component.scss'
})
export class SocialComponent implements OnInit {
  private store = inject(Store);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  activeTab = signal(0);

  // Get polls, trending outfits, and loading state from NgRx store
  private allPollsSignal = toSignal(this.store.select(selectPolls), { initialValue: [] as Poll[] }) as () => Poll[];
  trendingOutfits: Signal<TrendingOutfit[]> = toSignal(this.store.select(selectTrendingOutfits), { initialValue: [] as TrendingOutfit[] });
  
  // Limited trending outfits (top 4 for the main social page)
  limitedTrendingOutfits = computed(() => this.trendingOutfits().slice(0, 4));
  
  loading = computed(() => toSignal(this.store.select(selectPollsLoading))() || toSignal(this.store.select(selectTrendingLoading))());
  
  // Selection state
  selectedOutfit = signal<TrendingOutfit | null>(null);

  // Computed: Featured Poll - first active poll from the store
  featuredPoll = computed(() => {
    const polls = this.allPollsSignal();
    const activePolls = polls.filter(p => p.status === PollStatus.Active);
    
    if (activePolls.length > 0) {
      const poll = activePolls[0];
      return {
        id: poll.id,
        question: poll.question,
        options: poll.options.map(mapPollOptionToDisplayOption),
        timeLeft: getTimeLeft(poll.expiresAt),
        totalVotes: poll.totalVotes,
        createdAt: new Date(poll.createdAt).toLocaleTimeString()
      };
    }
    
    return null;
  });

  // Computed: All polls from store
  polls = computed(() => {
    const polls = this.allPollsSignal();
    return polls.map(poll => ({
      id: poll.id,
      question: poll.question,
      options: poll.options.map(mapPollOptionToDisplayOption),
      timeLeft: getTimeLeft(poll.expiresAt),
      totalVotes: poll.totalVotes
    }));
  });

  // Computed: Dynamic link for the featured poll
  currentPollLink = computed(() => {
    const poll = this.featuredPoll();
    if (!poll) return 'outfitplanner.com/poll/123call';
    return `outfitplanner.com/poll/${poll.id}`;
  });

  // Computed: QR Code URL
  qrCodeUrl = computed(() => {
    const poll = this.featuredPoll();
    const id = poll?.id || '123call';
    return `https://api.qrserver.com/v1/create-qr-code/?size=180x180&data=outfitplanner.com/poll/${id}`;
  });

  // Filter options
  filterChips = ['All', 'Date Night', 'Work', 'Casual', 'Sports', 'Formal'];
  activeFilter = signal('All');

  ngOnInit(): void {
    this.store.dispatch(PollsActions.loadPolls());
    this.store.dispatch(TrendingActions.loadTrending({ page: 1, pageSize: 20 }));
  }

  /**
   * Vote on a poll option
   * Dispatches the vote action to NgRx store
   */
  vote(pollId: string, optionId: string): void {
    const voteRequest: CastVoteRequest = {
      optionId: optionId,
      rating: 5, // Default rating
      isAnonymous: false
    };
    
    this.store.dispatch(PollsActions.vote({ pollId, request: voteRequest }));
  }

  /**
   * Set the active filter chip
   */
  setFilter(filter: string): void {
    this.activeFilter.set(filter);
  }

  /**
   * Select an outfit from the trending feed
   */
  viewOutfitDetails(outfit: TrendingOutfit) {
    this.router.navigate(['/outfits', outfit.id]);
  }

  /**
   * Select an outfit from the trending feed
   */
  selectOutfit(outfit: TrendingOutfit): void {
    this.selectedOutfit.set(outfit);
  }

  /**
   * Calculate vote percentage for an option
   * Takes the poll and option to find matching option by id
   */
  getVotePercentage(poll: { totalVotes: number; options: { id: string; votes: number }[] }, optionId: string): number {
    if (!poll || poll.totalVotes === 0) {
      return 0;
    }
    const option = poll.options.find((o: { id: string; votes: number }) => o.id === optionId);
    if (!option) {
      return 0;
    }
    return Math.round((option.votes / poll.totalVotes) * 100);
  }

  /**
   * Copy poll link to clipboard
   */
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
}
