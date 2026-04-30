import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FEED_REPOSITORY, FeedRepository } from '../repositories/feed.repository';
import { FeedPost } from '../entities/feed.entity';
import { PostComment } from '../entities/feed.entity';
import { CommandResponse, CursorPagedResult } from '../entities/response.entity';

@Injectable({
  providedIn: 'root',
})
export class FeedUseCases {
  constructor(
    @Inject(FEED_REPOSITORY) private readonly feedRepository: FeedRepository,
  ) {}

  getFeedPosts(
    cursor?: string,
    pageSize?: number,
    visibility?: string,
    sortBy?: string,
    postType?: string
  ): Observable<CursorPagedResult<FeedPost>> {
    return this.feedRepository.getFeedPosts(cursor, pageSize, visibility, sortBy, postType);
  }

  getPostById(id: string): Observable<FeedPost> {
    return this.feedRepository.getPostById(id);
  }

  deletePost(id: string): Observable<void> {
    return this.feedRepository.deletePost(id);
  }

  addReaction(postId: string): Observable<CommandResponse> {
    return this.feedRepository.addReaction(postId);
  }

  removeReaction(postId: string): Observable<CommandResponse> {
    return this.feedRepository.removeReaction(postId);
  }

  getComments(postId: string, cursor?: string, pageSize?: number): Observable<CursorPagedResult<PostComment>> {
    return this.feedRepository.getComments(postId, cursor, pageSize);
  }

  addComment(postId: string, content: string, parentCommentId?: string): Observable<CommandResponse> {
    return this.feedRepository.addComment(postId, content, parentCommentId);
  }

  deleteComment(commentId: string): Observable<void> {
    return this.feedRepository.deleteComment(commentId);
  }

  createOutfitPost(dto: { outfitId: string; caption?: string; visibility: number }): Observable<CommandResponse> {
    return this.feedRepository.createOutfitPost(dto);
  }
}
