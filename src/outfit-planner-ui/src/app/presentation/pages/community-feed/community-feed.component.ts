import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { Store } from '@ngrx/store';
import { SocialActions } from '../../../core/state/social/social.actions';
import {
  selectAllPolls,
  selectSocialLoading,
} from '../../../core/state/social/social.selectors';
import { ValidationPoll, PollStatus } from '../../../domain/entities/validation-poll.entity';
import { toSignal } from '@angular/core/rxjs-interop';

type FilterType = 'all' | 'active' | 'closed' | 'my-polls';

@Component({
  selector: 'app-community-feed',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatCardModule,
  ],
  templateUrl: './community-feed.component.html',
  styleUrl: './community-feed.component.scss',
})
export class CommunityFeedComponent implements OnInit {
  private store = inject(Store);
  private router = inject(Router);

  activeFilter = signal<FilterType>('all');

  // Get polls from store as signals
  private allPollsSignal = toSignal(this.store.select(selectAllPolls), { initialValue: [] as ValidationPoll[] }) as () => ValidationPoll[];
  loading = toSignal(this.store.select(selectSocialLoading), { initialValue: false });

  // Transform polls based on filter using computed
  filteredPolls = computed((): ValidationPoll[] => {
    const polls = this.allPollsSignal();
    const filter = this.activeFilter();
    switch (filter) {
      case 'active':
        return polls.filter((p: ValidationPoll) => p.status === PollStatus.Active);
      case 'closed':
        return polls.filter(
          (p: ValidationPoll) => p.status === PollStatus.Closed || p.status === PollStatus.Expired,
        );
      case 'my-polls':
        // In a real app, filter by current user ID
        return polls;
      default:
        return polls;
    }
  });

  filters: { value: FilterType; label: string }[] = [
    { value: 'all', label: 'All' },
    { value: 'active', label: 'Active' },
    { value: 'closed', label: 'Closed' },
    { value: 'my-polls', label: 'My Polls' },
  ];

  ngOnInit(): void {
    this.store.dispatch(SocialActions.loadPolls());
  }

  setFilter(filter: FilterType): void {
    this.activeFilter.set(filter);
  }

  createPoll(): void {
    this.router.navigate(['/social/create']);
  }

  viewPollDetail(pollId: string): void {
    this.router.navigate(['/social/polls', pollId]);
  }

  getTimeLeft(expiresAt: Date): string {
    const now = new Date();
    const diff = expiresAt.getTime() - now.getTime();

    if (diff <= 0) return 'Expired';

    const hours = Math.floor(diff / (1000 * 60 * 60));
    const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

    if (hours > 24) {
      const days = Math.floor(hours / 24);
      return `${days}d ${hours % 24}h left`;
    }
    return `${hours}h ${minutes}m left`;
  }

  getTotalVotes(poll: ValidationPoll): number {
    return poll.totalVotes;
  }

  getOptionThumbnail(option: PollOption): string {
    return (
      option.outfitThumbnail ||
      'https://images.unsplash.com/photo-1515372039744-b8f02a3ae446?w=200&h=200&fit=crop'
    );
  }
}

// Interface for poll option
interface PollOption {
  outfitThumbnail?: string;
}
