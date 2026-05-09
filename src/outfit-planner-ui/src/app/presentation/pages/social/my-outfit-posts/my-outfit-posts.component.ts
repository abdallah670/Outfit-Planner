import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import Swal from 'sweetalert2';
import { OutfitPostsActions } from '../../../../core/state/outfit-posts/outfit-posts.actions';
import { selectUserPosts, selectOutfitPostsLoading } from '../../../../core/state/outfit-posts/outfit-posts.selectors';
import { FeedPost, Visibility } from '../../../../domain/entities/feed.entity';

@Component({
  selector: 'app-my-outfit-posts',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatMenuModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './my-outfit-posts.component.html',
  styleUrls: ['./my-outfit-posts.component.scss'],
})
export class MyOutfitPostsComponent implements OnInit {
  private store = inject(Store);

  userPosts$ = this.store.select(selectUserPosts);
  loading$ = this.store.select(selectOutfitPostsLoading);

  activeFilter = signal<'all' | 'public' | 'friends' | 'private'>('all');
  activeSort = signal<'newest' | 'most-liked'>('newest');

  filteredPosts = computed(() => {
    const posts = toSignal(this.userPosts$)();
    if (!posts) return [];

    let filtered = posts;

    // Apply visibility filter
    if (this.activeFilter() !== 'all') {
      filtered = posts.filter((post: FeedPost) => {
        switch (this.activeFilter()) {
          case 'public':
            return post.visibility === Visibility.Public;
          case 'friends':
            return post.visibility === Visibility.FriendsOnly;
          case 'private':
            return post.visibility === Visibility.Private;
          default:
            return true;
        }
      });
    }

    // Apply sort
    return filtered.sort((a: FeedPost, b: FeedPost) => {
      if (this.activeSort() === 'most-liked') {
        return b.likesCount - a.likesCount;
      } else {
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      }
    });
  });

  ngOnInit(): void {
    this.store.dispatch(OutfitPostsActions.loadUserOutfitPosts());
  }

  getVisibilityLabel(visibility: Visibility): string {
    switch (visibility) {
      case Visibility.Public:
        return 'Public';
      case Visibility.FriendsOnly:
        return 'Friends Only';
      case Visibility.Private:
        return 'Private';
      default:
        return 'Public';
    }
  }

  getVisibilityColor(visibility: Visibility): string {
    switch (visibility) {
      case Visibility.Public:
        return 'primary';
      case Visibility.FriendsOnly:
        return 'accent';
      case Visibility.Private:
        return 'warn';
      default:
        return 'primary';
    }
  }

  onDeletePost(postId: string): void {
    Swal.fire({
      title: 'Delete Post?',
      text: 'This action cannot be undone.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      confirmButtonColor: '#d33',
    }).then((result) => {
      if (result.isConfirmed) {
        this.store.dispatch(OutfitPostsActions.deleteOutfitPost({ id: postId }));
      }
    });
  }

  trackByPostId(index: number, post: FeedPost): string {
    return post.id;
  }
}
