import { Component, Input, OnDestroy, OnInit, inject, ChangeDetectorRef, CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { FeedUseCases } from '../../../../../domain/usecases/feed.usecases';
import { AuthService } from '../../../../../core/services/auth.service';
import { PostComment } from '../../../../../domain/entities/feed.entity';
import { CursorPagedResult } from '../../../../../domain/entities/response.entity';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-comments-modal',
  standalone: true,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [
    CommonModule,
    FormsModule,
  ],
  templateUrl: './comments-modal.component.html',
  styleUrls: ['./comments-modal.component.scss']
})
export class CommentsModalComponent implements OnInit, OnDestroy {
  private _postId!: string;
  @Input() set postId(value: string) {
    if (value && value !== this._postId) {
      this._postId = value;
      this.loadComments(true);
    }
  }
  get postId(): string {
    return this._postId;
  }
  
  @Input() isInline = false;
  @Input() onCommentAdded?: (postId: string) => void;
  @Input() onCommentDeleted?: (postId: string) => void;

  private feedUseCases = inject(FeedUseCases);
  private authService = inject(AuthService);
  private router = inject(Router);
  private cdRef = inject(ChangeDetectorRef);

  isAuthenticated = signal(false);

  comments: PostComment[] = [];
  loading = false;
  loadingMore = false;
  hasMore = false;
  cursor: string | null = null;
  pageSize = 20;

  newCommentContent = '';
  replyingToCommentId: string | null = null;
  replyContent = '';
  editingCommentId: string | null = null;
  editContent = '';
  expandedReplies = new Set<string>();

  private subscriptions = new Subscription();

  ngOnInit(): void {
    // Initialization moved to postId setter to handle dynamic loading
  }

  

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  private loadComments(reset = false): void {
    if (reset) {
      this.cursor = null;
      this.comments = [];
      this.loading = true;
      this.cdRef.detectChanges();
    } else if (this.cursor && !this.loadingMore) {
      this.loadingMore = true;
      this.cdRef.detectChanges();
    }

    const sub = this.feedUseCases.getComments(this.postId, this.cursor || undefined, this.pageSize).subscribe({
      next: (result: CursorPagedResult<PostComment>) => {
        if (reset) {
          this.comments = result.items || [];
          this.loading = false;
        } else {
          this.comments = [...this.comments, ...(result.items || [])];
          this.loadingMore = false;
        }
        this.cursor = result.nextCursor;
        this.hasMore = result.hasMore;
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load comments', err);
        this.loading = false;
        this.loadingMore = false;
        this.cdRef.detectChanges();
      }
    });
    this.subscriptions.add(sub);
  }
  
  
  loadMore(): void {
    if (this.hasMore && this.cursor && !this.loadingMore) {
      this.loadComments(false);
    }
  }

  getCurrentUserId(): string | null {
    return this.authService.currentUser()?.id ?? null;
  }

  isOwnComment(comment: PostComment): boolean {
    const userId = this.getCurrentUserId();
    return userId !== null && comment.userId === userId;
  }

  startEditing(comment: PostComment): void {
    this.editingCommentId = comment.id;
    this.editContent = comment.content;
  }

  cancelEditing(): void {
    this.editingCommentId = null;
    this.editContent = '';
  }

  saveEdit(commentId: string): void {
    const trimmed = this.editContent.trim();
    if (!trimmed) return;

    this.subscriptions.add(
      this.feedUseCases.updateComment(commentId, trimmed).subscribe({
        next: () => {
          this.editingCommentId = null;
          this.editContent = '';
          this.loadComments(true);
        },
        error: (err) => {
          console.error('Failed to update comment', err);
          Swal.fire('Error', 'Failed to update comment', 'error');
        }
      })
    );
  }

  submitComment(): void {
    const trimmed = this.newCommentContent.trim();
    if (!trimmed || !this.postId) return;

    const currentUser = this.authService.currentUser();
    if (!currentUser) return;

    this.subscriptions.add(
      this.feedUseCases.addComment(this.postId, trimmed).subscribe({
        next: (response) => {
          // Create local comment for immediate feedback
          const newComment: PostComment = {
            id: response.id,
            userId: currentUser.id,
            userName: currentUser.userName || 'You',
            userAvatarUrl: currentUser.avatarUrl || 'assets/default-avatar.png',
            content: trimmed,
            createdAt: new Date(),
            isDeleted: false,
            replies: []
          };

          this.comments = [newComment, ...this.comments];
          this.newCommentContent = '';
          this.onCommentAdded?.(this.postId);
          this.cdRef.detectChanges();
        },
        error: (err) => {
          console.error('Failed to add comment', err);
          Swal.fire('Error', 'Failed to add comment', 'error');
          this.cdRef.detectChanges();
        }
      })
    );
  }

  startReplying(comment: PostComment): void {
    this.replyingToCommentId = comment.id;
    this.replyContent = `@${comment.userName} `;
    // Auto expand if it's not already
    this.expandedReplies.add(comment.id);
    this.cdRef.detectChanges();
    
    // Focus input after a short delay
    setTimeout(() => {
      const el = document.querySelector('.reply-textarea') as HTMLTextAreaElement;
      if (el) {
        el.focus();
        el.setSelectionRange(el.value.length, el.value.length);
      }
    }, 100);
  }

  cancelReplying(): void {
    this.replyingToCommentId = null;
    this.replyContent = '';
    this.cdRef.detectChanges();
  }

  submitReply(parentCommentId: string, replyToUser?: string): void {
    let trimmed = this.replyContent.trim();
    if (!trimmed || !this.postId) return;

    const currentUser = this.authService.currentUser();
    if (!currentUser) return;

    if (replyToUser) {
      trimmed = `@${replyToUser} ${trimmed}`;
    }

    this.subscriptions.add(
      this.feedUseCases.addComment(this.postId, trimmed, parentCommentId).subscribe({
        next: (response) => {
          // Find the parent comment in the tree and add the reply
          const newReply: PostComment = {
            id: response.id,
            userId: currentUser.id,
            userName: currentUser.userName || 'You',
            userAvatarUrl: currentUser.avatarUrl || 'assets/default-avatar.png',
            content: trimmed,
            createdAt: new Date(),
            isDeleted: false,
            parentCommentId: parentCommentId,
            replies: []
          };

          this.addReplyToTree(this.comments, parentCommentId, newReply);
          
          this.replyContent = '';
          this.replyingToCommentId = null;
          this.onCommentAdded?.(this.postId);
          this.cdRef.detectChanges();
        },
        error: (err) => {
          console.error('Failed to add reply', err);
          Swal.fire('Error', 'Failed to add reply', 'error');
          this.cdRef.detectChanges();
        }
      })
    );
  }

  private addReplyToTree(comments: PostComment[], parentId: string, newReply: PostComment): boolean {
    for (const comment of comments) {
      if (comment.id === parentId) {
        comment.replies = comment.replies || [];
        comment.replies.push(newReply);
        this.expandedReplies.add(comment.id);
        return true;
      }
      if (comment.replies && comment.replies.length > 0) {
        if (this.addReplyToTree(comment.replies, parentId, newReply)) return true;
      }
    }
    return false;
  }

  deleteComment(commentId: string): void {
    Swal.fire({
      title: 'Delete Comment?',
      text: 'Are you sure you want to delete this comment?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      confirmButtonColor: '#d33',
    }).then((result: any) => {
      if (result.isConfirmed) {
        this.subscriptions.add(
          this.feedUseCases.deleteComment(commentId).subscribe({
            next: () => {
              this.loadComments(true);
              this.onCommentDeleted?.(this.postId);
              Swal.fire('Deleted!', 'Comment deleted successfully', 'success')
                .then(() => setTimeout(() => Swal.close(), 500));
            },
            error: (err) => {
              console.error('Failed to delete comment', err);
              Swal.fire('Error', 'Failed to delete comment', 'error');
            }
          })
        );
      }
    });
  }
  toggleShowReplies(commentId: string): void {
    if (this.expandedReplies.has(commentId)) {
      this.expandedReplies.delete(commentId);
    } else {
      this.expandedReplies.add(commentId);
    }
  }

  areRepliesExpanded(commentId: string): boolean {
    return this.expandedReplies.has(commentId);
  }

  formatTimeAgo(date: Date): string {
    const now = new Date();
    const diff = now.getTime() - new Date(date).getTime();
    const mins = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (mins < 1) return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    if (hours < 24) return `${hours}h ago`;
    if (days < 7) return `${days}d ago`;
    return new Date(date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  /**
   * Parse @mentions in comment content into clickable segments.
   * Searches for known usernames (from comments/replies) in the content to handle multi-word names.
   */
  parseMentions(content: string): Array<{ text: string; isMention: boolean; userId?: string }> {
    if (!content) return [{ text: '', isMention: false }];
    
    // Build a map of username -> userId from all loaded comments and replies
    const userMap = new Map<string, string>();
    const collectUsers = (comments: PostComment[]) => {
      for (const c of comments) {
        if (c.userName && c.userId) {
          userMap.set(c.userName.toLowerCase(), c.userId);
        }
        if (c.replies && c.replies.length > 0) {
          collectUsers(c.replies);
        }
      }
    };
    collectUsers(this.comments);

    // Search for @username in the content for each known user (sorted longest name first to prioritize multi-word)
    interface MentionMatch {
      index: number;
      username: string;
      userId: string;
    }
    const mentions: MentionMatch[] = [];
    
    for (const [nameLower, userId] of userMap.entries()) {
      const searchStr = '@' + nameLower;
      let idx = content.toLowerCase().indexOf(searchStr);
      while (idx !== -1) {
        // Verify it's not part of a longer word (character before @ should be whitespace or start of string)
        const charBefore = idx > 0 ? content[idx - 1] : ' ';
        if (charBefore === ' ' || charBefore === '\t' || charBefore === '\n' || charBefore === '(' || charBefore === '[') {
          mentions.push({ index: idx, username: nameLower, userId });
        }
        idx = content.toLowerCase().indexOf(searchStr, idx + 1);
      }
    }

    // Sort mentions by their position in the content
    mentions.sort((a, b) => a.index - b.index);

    // Build segments
    const segments: Array<{ text: string; isMention: boolean; userId?: string }> = [];
    let lastIndex = 0;

    for (const mention of mentions) {
      // Skip if this mention overlaps with a previous one (e.g., "alex" inside "alex fashion")
      if (mention.index < lastIndex) continue;

      // Text before this mention
      if (mention.index > lastIndex) {
        segments.push({ text: content.slice(lastIndex, mention.index), isMention: false });
      }

      // The actual @username from the original content (preserve original casing)
      const mentionEnd = mention.index + mention.username.length + 1; // +1 for '@'
      segments.push({ text: content.slice(mention.index, mentionEnd), isMention: true, userId: mention.userId });
      lastIndex = mentionEnd;
    }

    // Remaining text after last mention
    if (lastIndex < content.length) {
      segments.push({ text: content.slice(lastIndex), isMention: false });
    }

    return segments.length > 0 ? segments : [{ text: content, isMention: false }];
  }

  goToUserProfile(userId: string): void {
    Swal.close();
    this.router.navigate(['/profile', userId]);
  }

  closeModal(): void {
    Swal.close();
  }
}
