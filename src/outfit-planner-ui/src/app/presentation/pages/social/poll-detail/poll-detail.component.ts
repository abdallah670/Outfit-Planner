import { Component, OnInit, inject, signal, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Poll, PollOption, PollStatus, getTimeLeft } from '../../../../domain/entities/poll.entity';
import { FeedPost, FeedPostWithComments, PostComment } from '../../../../domain/entities/feed.entity';
import { FeedUseCases } from '../../../../domain/usecases/feed.usecases';
import { AuthService } from '../../../../core/services/auth.service';
import { CursorPagedResult } from '../../../../domain/entities/response.entity';
import { CommentsModalComponent } from '../../../components/shared/modals/comments-modal/comments-modal.component';

@Component({
  selector: 'app-poll-detail',
  standalone: true,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    CommentsModalComponent,
  ],
  templateUrl: './poll-detail.component.html',
  styleUrls: ['./poll-detail.component.scss'],
})
export class PollDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private feedUseCases = inject(FeedUseCases);
  private authService = inject(AuthService);
  private router = inject(Router);
  private subscriptions = new Subscription();

  // Poll data
  pollPost = signal<FeedPostWithComments | null>(null);
  postloading = false;
  loading = signal(true);

  // Populated from FeedPost
  get poll(): Poll | null {
    return this.pollPost()?.poll ?? null;
  }

  // Author info
  pollAuthorName = 'User';
  pollAuthorAvatar = 'assets/default-avatar.png';

  // Voting state
  userVotedOptionId: string | null = null;
  submittingVote = signal(false);

  // Current user
  currentUser: any = null;
  currentUserAvatar = 'assets/default-avatar.png';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    // Load current user info
    this.currentUser = this.authService.currentUser();
    if (this.currentUser) {
      this.currentUserAvatar = this.currentUser.avatarUrl || 'assets/default-avatar.png';
    }

    this.loading.set(true);
    console.log('Fetching post with ID:', id);

    this.subscriptions.add(
      this.feedUseCases.getPostById(id).subscribe({
        next: (post) => {
          console.log('Received post:', post);
          if (post.poll) {

            this.pollPost.set(post as FeedPostWithComments);

            // Extract author info directly from the FeedPost
            if (this.currentUser && post.userId === this.currentUser.id) {
              this.pollAuthorName = this.currentUser.userName || 'You';
              this.pollAuthorAvatar = this.currentUser.avatarUrl || 'assets/default-avatar.png';
            } else {
              this.pollAuthorName = post.userName || 'User';
              this.pollAuthorAvatar = post.userAvatarUrl || 'assets/default-avatar.png';
            }

            // Track user's voted option from nested poll entity
            if (post.poll.userVotedOptionId) {
              this.userVotedOptionId = post.poll.userVotedOptionId;
            }
          }
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        }
      })
    );
  }

  handleCommentAdded = (postId: string): void => {
    if (this.pollPost() && this.pollPost()!.id === postId) {
      this.pollPost.update(post => {
        if (post) post.commentsCount++;
        return post;
      });
    }
  }

  handleCommentDeleted = (postId: string): void => {
    if (this.pollPost() && this.pollPost()!.id === postId) {
      this.pollPost.update(post => {
        if (post && post.commentsCount > 0) post.commentsCount--;
        return post;
      });
    }
  }

  // Voting
  toggleVote(pollPost: FeedPost, optionId: string, event: Event): void {
    event.stopPropagation();
    if (!pollPost.poll) return;

    const poll = pollPost.poll;
    if (poll.status !== 'active' || this.submittingVote()) return;

    this.submittingVote.set(true);

    if (poll.userVotedOptionId === optionId) {
      // Remove vote
      this.subscriptions.add(
        this.feedUseCases.removeVote(optionId).subscribe({
          next: () => {
            poll.userVotedOptionId = undefined;
            poll.totalVotes--;
            const option = poll.options.find(o => o.id === optionId);
            if (option) option.voteCount--;
            this.userVotedOptionId = null;
            this.submittingVote.set(false);
          },
          error: (err) => {
            console.error('Failed to remove vote', err);
            this.submittingVote.set(false);
          }
        })
      );
    } else {
      // Cast new vote
      this.subscriptions.add(
        this.feedUseCases.voteOnPoll(poll.id, optionId).subscribe({
          next: () => {
            // If they voted previously, decrement the old option
            if (poll.userVotedOptionId) {
              const oldOption = poll.options.find(o => o.id === poll.userVotedOptionId);
              if (oldOption) oldOption.voteCount--;
            } else {
              poll.totalVotes++; // Only increment total if they haven't voted before
            }
            
            poll.userVotedOptionId = optionId;
            const option = poll.options.find(o => o.id === optionId);
            if (option) option.voteCount++;
            this.userVotedOptionId = optionId;
            this.submittingVote.set(false);
          },
          error: (err) => {
            console.error('Failed to vote', err);
            this.submittingVote.set(false);
          }
        })
      );
    }
  }

  getLetter(index: number): string {
    return String.fromCharCode(65 + index);
  }

  getPercentage(poll: Poll, optionId: string): number {
    if (poll.totalVotes === 0) return 0;
    const option = poll.options.find((o: PollOption) => o.id === optionId);
    return option ? Math.round((option.voteCount / poll.totalVotes) * 100) : 0;
  }

  isWinner(poll: Poll, optionId: string): boolean {
    if (poll.totalVotes === 0) return false;
    const maxVotes = Math.max(...poll.options.map((o: PollOption) => o.voteCount));
    const optionSource = poll.options.find((o: PollOption) => o.id === optionId);
    return optionSource ? optionSource.voteCount === maxVotes : false;
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

  formatTimeAgo(date: Date): string {
    return this.getTimeAgo(date);
  }

  // Re-exported from poll.entity for template
  getTimeLeft = getTimeLeft;

  sharePoll(): void {
    const url = window.location.href;
    if (navigator.share) {
      navigator.share({
        title: 'Outfit Poll',
        text: 'Check out this outfit poll!',
        url,
      }).catch(() => {});
    } else {
      navigator.clipboard.writeText(url).then(() => {
        console.log('Link copied to clipboard');
      }).catch(() => {});
    }
  }
  onBack(): void {
    window.history.back();
  }

  goToUserProfile(userId: string): void {
    this.router.navigate(['/profile', userId]);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}