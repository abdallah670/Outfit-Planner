import { Component, Input, Output, EventEmitter, inject, ViewContainerRef, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import Swal from 'sweetalert2';
import { FeedPost, PostType } from '../../../../domain/entities/feed.entity';
import { Poll, PollOption } from '../../../../domain/entities/poll.entity';
import { FeedUseCases } from '../../../../domain/usecases/feed.usecases';
import { CommentsModalComponent } from '../modals/comments-modal/comments-modal.component';
import { VotersModalComponent } from '../modals/voters-modal/voters-modal.component';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-post-item',
  standalone: true,
  imports: [CommonModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './post-item.component.html',
  styleUrls: ['./post-item.component.scss']
})
export class PostItemComponent {
  @Input({ required: true }) post!: FeedPost;
  @Output() postUpdated = new EventEmitter<FeedPost>();
  @Output() postDeleted = new EventEmitter<string>();

  private feedUseCases = inject(FeedUseCases);
  private router = inject(Router);
  private viewContainerRef = inject(ViewContainerRef);
  private cdRef = inject(ChangeDetectorRef);
  private authService = inject(AuthService);

  readonly PostType = PostType;

  toggleReaction(event: Event): void {
    event.stopPropagation();
    if (this.post.isLiked) {
      this.feedUseCases.removeReaction(this.post.id).subscribe({
        next: () => {
          this.post = { ...this.post, isLiked: false, likesCount: this.post.likesCount - 1 };
          this.postUpdated.emit(this.post);
          this.cdRef.detectChanges();
        }
      });
    } else {
      this.feedUseCases.addReaction(this.post.id).subscribe({
        next: () => {
          this.post = { ...this.post, isLiked: true, likesCount: this.post.likesCount + 1 };
          this.postUpdated.emit(this.post);
          this.cdRef.detectChanges();
        }
      });
    }
  }

  toggleVote(optionId: string, event: Event): void {
    event.stopPropagation();
    const poll = this.post.poll;
    if (!poll || poll.status !== 'active') return;

    if (poll.userVotedOptionId === optionId) {
      this.feedUseCases.removeVote(optionId).subscribe({
        next: () => {
          const updatedPoll = {
            ...poll,
            userVotedOptionId: undefined,
            totalVotes: poll.totalVotes - 1,
            options: poll.options.map(o => o.id === optionId ? { ...o, voteCount: o.voteCount - 1 } : o)
          };
          this.post = { ...this.post, poll: updatedPoll };
          this.postUpdated.emit(this.post);
          this.cdRef.detectChanges();
        }
      });
    } else {
      this.feedUseCases.voteOnPoll(poll.id, optionId).subscribe({
        next: () => {
          const previousOptionId = poll.userVotedOptionId;
          const updatedPoll = {
            ...poll,
            userVotedOptionId: optionId,
            totalVotes: previousOptionId ? poll.totalVotes : poll.totalVotes + 1,
            options: poll.options.map(o => {
              if (o.id === optionId) return { ...o, voteCount: o.voteCount + 1 };
              if (o.id === previousOptionId) return { ...o, voteCount: o.voteCount - 1 };
              return o;
            })
          };
          this.post = { ...this.post, poll: updatedPoll };
          this.postUpdated.emit(this.post);
          this.cdRef.detectChanges();
        }
      });
    }
  }

  openCommentsModal(event: Event): void {
    event.stopPropagation();
    const componentRef = this.viewContainerRef.createComponent(CommentsModalComponent);
    componentRef.instance.postId = this.post.id;
    componentRef.instance.onCommentAdded = () => {
      this.post = { ...this.post, commentsCount: this.post.commentsCount + 1 };
      this.postUpdated.emit(this.post);
      this.cdRef.detectChanges();
    };
    componentRef.instance.onCommentDeleted = () => {
      this.post = { ...this.post, commentsCount: Math.max(0, this.post.commentsCount - 1) };
      this.postUpdated.emit(this.post);
      this.cdRef.detectChanges();
    };

    const element = (componentRef.hostView as any).rootNodes[0] as HTMLElement;
    Swal.fire({
      html: element,
      width: 600,
      showConfirmButton: false,
      background: 'transparent',
      backdrop: true,
      didOpen: () => componentRef.changeDetectorRef.detectChanges(),
      willClose: () => componentRef.destroy()
    });
  }

  openVotersModal(event: Event): void {
    event.stopPropagation();
    if (!this.post.pollId) return;
    const componentRef = this.viewContainerRef.createComponent(VotersModalComponent);
    componentRef.instance.pollId = this.post.pollId;

    const element = (componentRef.hostView as any).rootNodes[0] as HTMLElement;
    Swal.fire({
      html: element,
      width: 500,
      showConfirmButton: false,
      background: 'transparent',
      backdrop: true,
      didOpen: () => componentRef.changeDetectorRef.detectChanges(),
      willClose: () => componentRef.destroy()
    });
  }

  goToUserProfile(userId: string, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/profile', userId]);
  }

  viewPostDetail(): void {
    if (this.post.postType === PostType.Poll) {
      this.router.navigate(['/social/polls', this.post.id]);
    } else {
      this.router.navigate(['/social/posts', this.post.id]);
    }
  }

  getTimeAgo(date: Date | string | undefined): string {
    if (!date) return '';
    const now = new Date();
    const then = new Date(date);
    const diffMs = now.getTime() - then.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHrs = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHrs / 24);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHrs < 24) return `${diffHrs}h ago`;
    if (diffDays < 30) return `${diffDays}d ago`;
    return then.toLocaleDateString();
  }

  getOptionLabel(index: number): string {
    return String.fromCharCode(65 + index);
  }

  getVotePercentage(option: PollOption): string {
    const total = this.post.poll?.totalVotes || 0;
    if (total === 0) return '0%';
    const percentage = (option.voteCount / total) * 100;
    return `${percentage.toFixed(0)}%`;
  }

  isWinner(optionId: string): boolean {
    const poll = this.post.poll;
    if (!poll || poll.totalVotes === 0) return false;
    const maxVotes = Math.max(...poll.options.map(o => o.voteCount));
    if (maxVotes === 0) return false;
    return poll.options.find(o => o.id === optionId)?.voteCount === maxVotes;
  }
  isOwner(post: FeedPost): boolean {
    return post.userId === this.authService.currentUser()?.id;
  }

  showMenu = false;

  toggleMenu(event: Event): void {
    event.stopPropagation();
    this.showMenu = !this.showMenu;
  }

  @HostListener('document:click')
  closeMenu(): void {
    this.showMenu = false;
  }

  editPost(event: Event): void {
    event.stopPropagation();
    this.showMenu = false;
    if (this.post.postType === PostType.Poll) {
      this.router.navigate(['/social/polls', this.post.pollId || this.post.poll?.id || this.post.id, 'edit']);
    } else {
      this.router.navigate(['/social/posts', this.post.id, 'edit']);
    }
  }

  deletePost(): void {
    this.showMenu = false;
    Swal.fire({
      title: 'Delete Post',
      text: 'Are you sure you want to delete this post?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Yes, delete it!',
      cancelButtonText: 'No, cancel',
      confirmButtonColor: '#fab4c6',
      cancelButtonColor: '#2d3748'
    }).then((result) => {
      if (result.isConfirmed) {
        this.feedUseCases.deletePost(this.post.id).subscribe({
          next: () => {
            this.postDeleted.emit(this.post.id);
            
            Swal.fire('Deleted!', 'Your post has been deleted.', 'success');
          },
          error: (error) => {
            Swal.fire('Error!', error.message, 'error');
          }
        });
      }
    });
  }
}
