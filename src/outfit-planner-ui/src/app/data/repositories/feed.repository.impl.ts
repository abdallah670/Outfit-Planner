import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { FeedRepository, FEED_REPOSITORY } from '../../domain/repositories/feed.repository';
import { FeedDataSource } from '../../data/datasources/feed.datasource';
import { OutfitPostsDataSource } from '../../data/datasources/outfit-posts.datasource';
import { FeedPost } from '../../domain/entities/feed.entity';
import { PostComment } from '../../domain/entities/feed.entity';
import { CommandResponse, CursorPagedResult } from '../../domain/entities/response.entity';

@Injectable({
  providedIn: 'root',
})
export class FeedRepositoryImpl implements FeedRepository {
  private feedDataSource = inject(FeedDataSource);
  private outfitPostsDataSource = inject(OutfitPostsDataSource);

  getFeedPosts(
    cursor?: string,
    pageSize?: number,
    visibility?: string,
    sortBy?: string,
    postType?: string
  ): Observable<CursorPagedResult<FeedPost>> {
    return this.feedDataSource.getFeedPosts(cursor, pageSize, visibility, sortBy, postType);
  }

  getPostById(id: string): Observable<FeedPost> {
    return this.feedDataSource.getPostById(id);
  }

  deletePost(id: string): Observable<void> {
    return this.feedDataSource.deletePost(id);
  }

  addReaction(postId: string): Observable<CommandResponse> {
    return this.feedDataSource.addReaction(postId);
  }

  removeReaction(postId: string): Observable<CommandResponse> {
    return this.feedDataSource.removeReaction(postId);
  }

  getComments(postId: string, cursor?: string, pageSize?: number): Observable<CursorPagedResult<PostComment>> {
    return this.feedDataSource.getComments(postId, cursor, pageSize);
  }

  addComment(postId: string, content: string, parentCommentId?: string): Observable<CommandResponse> {
    return this.feedDataSource.addComment(postId, content, parentCommentId);
  }

  deleteComment(commentId: string): Observable<void> {
    return this.feedDataSource.deleteComment(commentId);
  }

  createOutfitPost(dto: { outfitId: string; caption?: string; visibility: number }): Observable<CommandResponse> {
    return this.outfitPostsDataSource.createOutfitPost(dto);
  }
}

export const feedRepositoryProvider = {
  provide: FEED_REPOSITORY,
  useClass: FeedRepositoryImpl,
};
