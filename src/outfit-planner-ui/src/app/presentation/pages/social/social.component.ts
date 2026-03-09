import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
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
  selectActivePolls,
} from '../../../core/state/social/social.selectors';
import {
  ValidationPoll,
  CastVoteRequest,
  PollStatus,
  PollOption,
} from '../../../domain/entities/validation-poll.entity';

interface TrendingOutfit {
  id: string;
  userName: string;
  userAvatar: string;
  imageUrl: string;
  likes: number;
  occasion: string;
}

interface CommunityFeedItem {
  id: string;
  image1: string;
  image2: string;
  userAvatar: string;
  likes: string;
}

interface Comment {
  id: string;
  author: string;
  userAvatar: string;
  time: string;
  text: string;
}

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
  private snackBar = inject(MatSnackBar);

  activeTab = signal(0);

  // Get polls from NgRx store as signals
  private allPollsSignal = toSignal(this.store.select(selectAllPolls), { initialValue: [] as ValidationPoll[] }) as () => ValidationPoll[];
  loading = toSignal(this.store.select(selectSocialLoading), { initialValue: false });

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
    
    // Return null when no polls available
    return null;
  });

  // Computed: All polls from store (for the polls tab)
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

  // Community Feed items - static for now (would come from separate API)
  communityFeed = signal<CommunityFeedItem[]>([
    {
      id: '1',
      image1: 'https://images.unsplash.com/photo-1515372039744-b8f02a3ae446?w=200&h=150&fit=crop',
      image2: 'https://images.unsplash.com/photo-1543076447-215ad9ba6923?w=200&h=150&fit=crop',
      userAvatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=50&h=50&fit=crop',
      likes: '1.2k'
    },
    {
      id: '2',
      image1: 'https://images.unsplash.com/photo-1595777457583-95e059d581b8?w=200&h=150&fit=crop',
      image2: 'https://images.unsplash.com/photo-1521369909029-2afed882baee?w=200&h=150&fit=crop',
      userAvatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=50&h=50&fit=crop',
      likes: '1.2k'
    },
    {
      id: '3',
      image1: 'https://images.unsplash.com/photo-1594633312681-425c7b97ccd1?w=200&h=150&fit=crop',
      image2: 'https://images.unsplash.com/photo-1585487000160-6ebcfceb0d03?w=200&h=150&fit=crop',
      userAvatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=50&h=50&fit=crop',
      likes: '1.2k'
    },
    {
      id: '4',
      image1: 'https://images.unsplash.com/photo-1594631252845-29fc4cc8cde9?w=200&h=150&fit=crop',
      image2: 'https://images.unsplash.com/photo-1539008835657-9e8e9680c956?w=200&h=150&fit=crop',
      userAvatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=50&h=50&fit=crop',
      likes: '1.2k'
    }
  ]);

  // Community Comments - static for now
  comments = signal<Comment[]>([
    {
      id: '1',
      author: 'StyleMaven92',
      userAvatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=50&h=50&fit=crop',
      time: '2 mins ago',
      text: 'Option B is super cute and perfect for the weather!'
    },
    {
      id: '2',
      author: 'FashionFan45',
      userAvatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=50&h=50&fit=crop',
      time: '2 mins ago',
      text: 'I love the colors in A!'
    },
    {
      id: '3',
      author: 'TrendSetter88',
      userAvatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=50&h=50&fit=crop',
      time: '2 mins ago',
      text: 'Option C looks so elegant!'
    }
  ]);

  // Mock trending outfits - static for now
  trendingOutfits = signal<TrendingOutfit[]>([
    { id: '1', userName: 'Sarah', userAvatar: '', imageUrl: '', likes: 234, occasion: 'Date Night' },
    { id: '2', userName: 'Mike', userAvatar: '', imageUrl: '', likes: 189, occasion: 'Casual' },
    { id: '3', userName: 'Alex', userAvatar: '', imageUrl: '', likes: 156, occasion: 'Business' },
    { id: '4', userName: 'Emma', userAvatar: '', imageUrl: '', likes: 142, occasion: 'Weekend' },
    { id: '5', userName: 'James', userAvatar: '', imageUrl: '', likes: 128, occasion: 'Date Night' },
    { id: '6', userName: 'Olivia', userAvatar: '', imageUrl: '', likes: 115, occasion: 'Casual' }
  ]);

  // Filter options
  filterChips = ['All', 'Date Night', 'Work', 'Casual', 'Sports', 'Formal'];
  activeFilter = signal('All');

  ngOnInit(): void {
    // Load polls from API via NgRx
    this.store.dispatch(SocialActions.loadPolls());
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
    
    this.snackBar.open('Vote submitted successfully!', 'Close', {
      duration: 3000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom'
    });
  }

  /**
   * Set the active filter chip
   */
  setFilter(filter: string): void {
    this.activeFilter.set(filter);
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
