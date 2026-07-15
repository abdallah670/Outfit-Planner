import { Component, Input, OnDestroy, OnInit, inject, ChangeDetectorRef, CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { FeedUseCases } from '../../../../../domain/usecases/feed.usecases';
import { FollowUseCases } from '../../../../../domain/usecases/follow.usecases';
import { AuthService } from '../../../../../core/services/auth.service';
import { PostComment, MentionedUser } from '../../../../../domain/entities/feed.entity';
import { Follower } from '../../../../../domain/entities/follow.entity';
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
  private followUseCases = inject(FollowUseCases);
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

  // @mention state
  mentionedUsers = new Map<string, MentionedUser>();
  mentionDropdownOpen = false;
  mentionMatches: Follower[] = [];
  activeMentionField: 'new' | 'reply' | null = null;

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

  // ----- @mention picker -----

  mentionSearch = '';

  onMentionInput(event: Event, field: 'new' | 'reply'): void {
    const textarea = event.target as HTMLTextAreaElement;
    const caret = textarea.selectionStart ?? textarea.value.length;
    const before = textarea.value.substring(0, caret);
    const atIndex = before.lastIndexOf('@');
    if (atIndex === -1) {
      this.closeMentionPicker();
      return;
    }
    const token = before.substring(atIndex + 1);
    if (/\s/.test(token)) {
      this.closeMentionPicker();
      return;
    }
    this.activeMentionField = field;
    this.mentionSearch = token;
    this.loadMentionSuggestions(token);
  }

  loadMentionSuggestions(query: string): void {
    const currentUserId = this.getCurrentUserId();
    if (!currentUserId) {
      this.closeMentionPicker();
      return;
    }
    this.subscriptions.add(
      this.followUseCases.getFollowers(currentUserId, undefined, 20, query).subscribe({
        next: (res) => {
          this.mentionMatches = res.items || [];
          this.mentionDropdownOpen = this.mentionMatches.length > 0;
          this.cdRef.detectChanges();
        },
        error: () => this.closeMentionPicker()
      })
    );
  }

  selectMention(follower: Follower, textarea: HTMLTextAreaElement): void {
    const caret = textarea.selectionStart ?? textarea.value.length;
    const value = textarea.value;
    const before = value.substring(0, caret);
    const after = value.substring(caret);
    const atIndex = before.lastIndexOf('@');
    const newBefore = before.substring(0, atIndex) + '@' + (follower.fullName || follower.userName) + ' ';
    const newValue = newBefore + after;

    if (this.activeMentionField === 'new') {
      this.newCommentContent = newValue;
    } else {
      this.replyContent = newValue;
    }

    this.mentionedUsers.set(follower.userId, {
      userId: follower.userId,
      userName: follower.fullName || follower.userName,
      profilePictureUrl: follower.userAvatarUrl
    });

    this.closeMentionPicker();
    this.cdRef.detectChanges();

    setTimeout(() => {
      textarea.focus();
      const pos = newBefore.length;
      textarea.setSelectionRange(pos, pos);
    }, 0);
  }

  closeMentionPicker(): void {
    this.mentionDropdownOpen = false;
    this.mentionMatches = [];
    this.activeMentionField = null;
  }

  submitComment(): void {
    const trimmed = this.newCommentContent.trim();
    if (!trimmed || !this.postId) return;

    const currentUser = this.authService.currentUser();
    if (!currentUser) return;

    this.subscriptions.add(
      this.feedUseCases.addComment(this.postId, trimmed, undefined, Array.from(this.mentionedUsers.values())).subscribe({
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
          this.mentionedUsers.clear();
          this.closeMentionPicker();
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
    // First mention is always the parent comment's author.
    this.mentionedUsers.set(comment.userId, {
      userId: comment.userId,
      userName: comment.userName,
      profilePictureUrl: comment.userAvatarUrl
    });
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
      this.feedUseCases.addComment(this.postId, trimmed, parentCommentId, Array.from(this.mentionedUsers.values())).subscribe({
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
          this.mentionedUsers.clear();
          this.closeMentionPicker();
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
        comment.totalReplies = (comment.totalReplies || 0) + 1;
        this.expandedReplies.add(comment.id);
        return true;
      }
      if (comment.replies && comment.replies.length > 0) {
        if (this.addReplyToTree(comment.replies, parentId, newReply)) return true;
      }
    }
    return false;
  }
  private deleteReplyFromTree(comments: PostComment[], commentId: string): boolean {
    for (const comment of comments) {
      if (comment.replies && comment.replies.length > 0) {
        const index = comment.replies.findIndex(r => r.id === commentId);
        if (index !== -1) {
          comment.replies.splice(index, 1);
          comment.totalReplies = Math.max(0, (comment.totalReplies || 0) - 1);
          return true;
        }
        if (this.deleteReplyFromTree(comment.replies, commentId)) return true;
      }
    }
    return false;
  }
  //switch parent comment of replies of deleted comment if it has replies, otherwise remove it from the list
  swapParentCommentofRepliesofDeletedComment(comments: PostComment[], commentId: string): boolean {
    for (const comment of comments) {
      if (comment.id === commentId) {
        if (comment.replies && comment.replies.length > 0) {
          for (const reply of comment.replies) {
            reply.parentCommentId = comment.parentCommentId || undefined;
            if (reply.parentCommentId) {
              this.addReplyToTree(this.comments, reply.parentCommentId, reply);
            } else {
              this.comments.push(reply);
            }
          }
        }
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
              //swap parent id of replies
              this.swapParentCommentofRepliesofDeletedComment(this.comments, commentId);
              // Try to remove from top-level comments first
              const index = this.comments.findIndex(c => c.id === commentId);
              
              if (index !== -1) {
                this.comments.splice(index, 1);
              } else {
              
                // If not found at top level, try to remove from replies
                this.deleteReplyFromTree(this.comments, commentId);
              }
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
    const newSet = new Set(this.expandedReplies);
    if (newSet.has(commentId)) {
      newSet.delete(commentId);
    } else {
      newSet.add(commentId);
      // Lazily fetch replies from the backend when expanding
      this.loadRepliesForComment(commentId);
    }
    this.expandedReplies = newSet;
    this.cdRef.detectChanges();
  }

  private loadRepliesForComment(commentId: string): void {
    // Find the comment in the tree and check if replies are already populated
    const findComment = (comments: PostComment[]): PostComment | null => {
      for (const c of comments) {
        if (c.id === commentId) return c;
        if (c.replies && c.replies.length > 0) {
          const found = findComment(c.replies);
          if (found) return found;
        }
      }
      return null;
    };

    const comment = findComment(this.comments);
    if (!comment) return;

    // Only fetch if replies are not already loaded
    if (comment.replies && comment.replies.length > 0) return;

    // Fetch replies via the same paginated comments endpoint using parentCommentId
    this.subscriptions.add(
      this.feedUseCases.getComments(this.postId, undefined, 100).subscribe({
        next: (result) => {
          // Filter all comments that are direct children of this comment
          const directReplies = (result.items || []).filter(
            c => c.parentCommentId === commentId
          );
          if (directReplies.length > 0) {
            comment.replies = directReplies;
            // Trigger change detection
            this.cdRef.detectChanges();
          }
        },
        error: (err) => {
          console.error('Failed to load replies for comment', commentId, err);
        }
      })
    );
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
