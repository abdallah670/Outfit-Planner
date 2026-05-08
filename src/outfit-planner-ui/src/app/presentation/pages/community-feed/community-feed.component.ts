import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { Store } from '@ngrx/store';
import { FeedActions } from '../../../core/state/feed/feed.actions';
import { PollsActions } from '../../../core/state/polls/polls.actions';
import {
  selectPosts,
  selectNextCursor,
  selectHasMore,
  selectFeedLoading,
} from '../../../core/state/feed/feed.selectors';
import { FeedPost, PostType, PostComment } from '../../../domain/entities/feed.entity';
import { CastVoteRequest } from '../../../domain/entities/poll.entity';
import { PollCardComponent } from '../../components/poll-card/poll-card.component';
import { toSignal } from '@angular/core/rxjs-interop';

type FilterType = 'all' | 'outfits' | 'polls';

@Component({
  selector: 'app-community-feed',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatCardModule,
    PollCardComponent,
  ],
  templateUrl: './community-feed.component.html',
  styleUrl: './community-feed.component.scss',
})
export class CommunityFeedComponent implements OnInit {
  private store = inject(Store);
  private router = inject(Router);

  activeFilter = signal<FilterType>('all');

  private allPostsSignal = toSignal(this.store.select(selectPosts), { initialValue: [] as FeedPost[] }) as () => FeedPost[];
  loading = toSignal(this.store.select(selectFeedLoading), { initialValue: false });
  nextCursor = toSignal(this.store.select(selectNextCursor), { initialValue: null as string | null });
  hasMore = toSignal(this.store.select(selectHasMore), { initialValue: false });

  filteredPosts = computed((): FeedPost[] => {
    const posts = this.allPostsSignal();
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

  filters: { value: FilterType; label: string }[] = [
    { value: 'all', label: 'All' },
    { value: 'outfits', label: 'Outfits' },
    { value: 'polls', label: 'Polls' },
  ];

  ngOnInit(): void {
    this.store.dispatch(FeedActions.loadPosts({ pageSize: 20 }));
  }

  setFilter(filter: FilterType): void {
    this.activeFilter.set(filter);
    const postType = filter === 'outfits' ? 'OutfitPost' : filter === 'polls' ? 'PollPost' : undefined;
    this.store.dispatch(FeedActions.loadPosts({ postType, pageSize: 20, cursor: undefined }));
  }

  createPoll(): void {
    this.router.navigate(['/social/create']);
  }

  viewPostDetail(post: FeedPost): void {
    if (post.postType === PostType.PollPost && post.pollId) {
      this.router.navigate(['/social/polls', post.pollId]);
    } else if (post.outfitId) {
      this.router.navigate(['/outfits', post.outfitId]);
    } else {
      this.router.navigate(['/social/feed']); // fallback
    }
  }

  getTimeLeft(expiresAt: Date | undefined): string {
    if (!expiresAt) return '';
    const now = new Date();
    const expires = new Date(expiresAt);
    const diff = expires.getTime() - now.getTime();

    if (diff <= 0) return 'Expired';

    const hours = Math.floor(diff / (1000 * 60 * 60));
    const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

    if (hours > 24) {
      const days = Math.floor(hours / 24);
      return `${days}d ${hours % 24}h left`;
    }
    return `${hours}h ${minutes}m left`;
  }

  toggleReaction(post: FeedPost, event: Event): void {
    event.stopPropagation();
    if (post.userReaction) {
      this.store.dispatch(FeedActions.removeReaction({ postId: post.id }));
    } else {
      this.store.dispatch(FeedActions.addReaction({ postId: post.id }));
    }
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
}
