import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { FeedPost } from '../entities/feed.entity';
import { PostComment } from '../entities/feed.entity';
import { CommandResponse, CursorPagedResult } from '../entities/response.entity';

export const FEED_REPOSITORY = new InjectionToken<FeedRepository>('FeedRepository');

export interface FeedRepository {
  getFeedPosts(
    cursor?: string,
    pageSize?: number,
    visibility?: string,
    sortBy?: string,
    postType?: string
  ): Observable<CursorPagedResult<FeedPost>>;

  getPostById(id: string): Observable<FeedPost>;

  deletePost(id: string): Observable<void>;

  addReaction(postId: string): Observable<CommandResponse>;

  removeReaction(postId: string): Observable<CommandResponse>;

  getComments(postId: string, cursor?: string, pageSize?: number): Observable<CursorPagedResult<PostComment>>;

  addComment(postId: string, content: string, parentCommentId?: string): Observable<CommandResponse>;

  deleteComment(commentId: string): Observable<void>;

  createOutfitPost(dto: { outfitId: string; caption?: string; visibility: number }): Observable<CommandResponse>;
}
