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
import { SocialActions } from '../../../core/state/social/social.actions';
import {
  selectAllPolls,
  selectSocialLoading,
  selectTrendingOutfits,
  selectCommentsByVote,
} from '../../../core/state/social/social.selectors';
import {
  ValidationPoll,
  CastVoteRequest,
  PollStatus,
  PollOption,
} from '../../../domain/entities/validation-poll.entity';
import { SOCIAL_REPOSITORY } from '../../../domain/repositories/social.repository';
import { TrendingOutfit, VoteComment, AddVoteCommentRequest } from '../../../domain/entities/social-engagement.entity';

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
  private socialRepository = inject(SOCIAL_REPOSITORY);

  activeTab = signal(0);

  // Get polls, trending outfits, and loading state from NgRx store
  private allPollsSignal = toSignal(this.store.select(selectAllPolls), { initialValue: [] as ValidationPoll[] }) as () => ValidationPoll[];
  trendingOutfits: Signal<TrendingOutfit[]> = toSignal(this.store.select(selectTrendingOutfits), { initialValue: [] as TrendingOutfit[] });
  
  // Limited trending outfits (top 4 for the main social page)
  limitedTrendingOutfits = computed(() => this.trendingOutfits().slice(0, 4));
  
  loading = toSignal(this.store.select(selectSocialLoading), { initialValue: false });
  
  // Selection state for engagement
  selectedOutfit = signal<TrendingOutfit | null>(null);
  selectedOutfitComments = computed(() => {
    const outfit = this.selectedOutfit();
    if (!outfit) return [] as VoteComment[];
    const commentsMap = toSignal(this.store.select(selectCommentsByVote), { initialValue: {} })();
    return (commentsMap as any)[outfit.voteId] || [];
  });

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
    this.store.dispatch(SocialActions.loadPolls());
    this.store.dispatch(SocialActions.loadTrending());
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
    
    this.store.dispatch(SocialActions.vote({ pollId, request: voteRequest }));
  }

  /**
   * Set the active filter chip
   */
  setFilter(filter: string): void {
    this.activeFilter.set(filter);
  }

  /**
   * Select an outfit from the trending feed to see engagement
   */
  viewOutfitDetails(outfit: TrendingOutfit) {
    this.router.navigate(['/outfits', outfit.id]);
  }

  /**
   * Select an outfit from the trending feed to see engagement
   */
  selectOutfit(outfit: TrendingOutfit): void {
    this.selectedOutfit.set(outfit);
    this.store.dispatch(SocialActions.loadVoteComments({ voteId: outfit.voteId }));
  }

  /**
   * Quick reaction to an outfit (e.g. from the card)
   */
  react(outfit: TrendingOutfit, reactionType: string = 'Like'): void {
    this.store.dispatch(SocialActions.reactToVote({ voteId: outfit.voteId, reactionType }));
  }

  /**
   * Add a comment to the selected outfit
   */
  submitComment(content: string): void {
    const outfit = this.selectedOutfit();
    if (!outfit || !content.trim()) return;

    const request: AddVoteCommentRequest = {
      voteId: outfit.voteId,
      content: content.trim()
    };

    this.store.dispatch(SocialActions.addVoteComment({ request }));
  }

  /**
   * Calculate vote percentage for an option
   * Takes the poll and option to find matching option by id
   */
  getVotePercentage(poll: { totalVotes: number; options: { id: string; votes: number }[] }, optionId: string): number {
    if (!poll || poll.totalVotes === 0) {
      return 0;
    }
    const option = poll.options.find(o => o.id === optionId);
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
