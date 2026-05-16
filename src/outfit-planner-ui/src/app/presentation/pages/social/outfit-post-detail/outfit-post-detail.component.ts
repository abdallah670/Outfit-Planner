import { Component, OnInit, inject, signal, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { FeedPostWithComments } from '../../../../domain/entities/feed.entity';
import { FeedUseCases } from '../../../../domain/usecases/feed.usecases';
import { AuthService } from '../../../../core/services/auth.service';
import { CommentsModalComponent } from '../../../components/shared/modals/comments-modal/comments-modal.component';

@Component({
  selector: 'app-outfit-post-detail',
  standalone: true,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [CommonModule, RouterModule, CommentsModalComponent],
  templateUrl: './outfit-post-detail.component.html',
  styleUrls: ['./outfit-post-detail.component.scss'],
})
export class OutfitPostDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private feedUseCases = inject(FeedUseCases);
  private authService = inject(AuthService);
  private router = inject(Router);
  private subscriptions = new Subscription();

  outfitPost = signal<FeedPostWithComments | null>(null);
  loading = signal(true);

  authorName = 'User';
  authorAvatar = 'assets/default-avatar.png';
  currentUser: any = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.currentUser = this.authService.currentUser();
    this.loading.set(true);

    this.subscriptions.add(
      this.feedUseCases.getPostById(id).subscribe({
        next: (post) => {
          this.outfitPost.set(post as FeedPostWithComments);
          
          if (this.currentUser && post.userId === this.currentUser.id) {
            this.authorName = this.currentUser.userName || 'You';
            this.authorAvatar = this.currentUser.avatarUrl || 'assets/default-avatar.png';
          } else {
            this.authorName = post.userName || 'User';
            this.authorAvatar = post.userAvatarUrl || 'assets/default-avatar.png';
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
    if (this.outfitPost() && this.outfitPost()!.id === postId) {
      this.outfitPost.update(post => {
        if (post) post.commentsCount++;
        return post;
      });
    }
  }

  handleCommentDeleted = (postId: string): void => {
    if (this.outfitPost() && this.outfitPost()!.id === postId) {
      this.outfitPost.update(post => {
        if (post && post.commentsCount > 0) post.commentsCount--;
        return post;
      });
    }
  }

  toggleReaction(): void {
    const post = this.outfitPost();
    if (!post) return;

    const originalState = post.isLiked;
    const originalCount = post.likesCount;

    // Optimistic update
    this.outfitPost.update(p => {
      if (!p) return p;
      return {
        ...p,
        isLiked: !originalState,
        likesCount: originalState ? p.likesCount - 1 : p.likesCount + 1
      };
    });

    const action$ = originalState 
      ? this.feedUseCases.removeReaction(post.id) 
      : this.feedUseCases.addReaction(post.id);

    this.subscriptions.add(
      action$.subscribe({
        error: (err) => {
          console.error('Failed to toggle reaction:', err);
          // Rollback on error
          this.outfitPost.update(p => {
            if (!p) return p;
            return {
              ...p,
              isLiked: originalState,
              likesCount: originalCount
            };
          });
        }
      })
    );
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

  sharePost(): void {
    const url = window.location.href;
    if (navigator.share) {
      navigator.share({
        title: 'Outfit Post',
        text: 'Check out this outfit!',
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

  onEdit(): void {
    const post = this.outfitPost();
    if (post) {
      this.router.navigate(['/social/posts', post.id, 'edit']);
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
