import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { FeedPost } from '../../domain/entities/feed.entity';
import { PostComment } from '../../domain/entities/feed.entity';
import { CommandResponse,CursorPagedResult } from '../../domain/entities/response.entity';


@Injectable({
  providedIn: 'root',
})
export class FeedDataSource {
  private readonly apiUrl = `${environment.baseUrl}/api/feed`;

  constructor(private http: HttpClient) {}

  getFeedPosts(
    cursor?: string,
    pageSize: number = 20,
    visibility: string = 'Public',
    sortBy: string = 'recent',
    postType?: string
  ): Observable<CursorPagedResult<FeedPost>> {
    let params = new HttpParams()
      .set('pageSize', pageSize.toString())
      .set('visibility', visibility)
      .set('sortBy', sortBy);

    if (cursor) {
      params = params.set('cursor', cursor);
    }
    if (postType) {
      params = params.set('postType', postType);
    }

    return this.http.get<CursorPagedResult<any>>(this.apiUrl, { params }).pipe(
      map(response => ({
        ...response,
        nextCursor: response.nextCursor || null,
        items: response.items.map((post: any) => this.mapFeedPost(post))
      }))
    );
  }
getUserFeed(
    userId: string,
    cursor?: string,
    pageSize: number = 20,
    postType?: string
  ): Observable<CursorPagedResult<FeedPost>> {
    let params = new HttpParams().set('pageSize', pageSize.toString());
    if (cursor) params = params.set('cursor', cursor);
    if (postType) params = params.set('postType', postType);

    return this.http.get<CursorPagedResult<any>>(`${this.apiUrl}/user/${userId}`, { params }).pipe(
      map(response => ({
        ...response,
        nextCursor: response.nextCursor || null,
        items: response.items.map((post: any) => this.mapFeedPost(post))
      }))
    );
  }
  getPostById(id: string): Observable<FeedPost> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(post => this.mapFeedPost(post))
    );
  }

  deletePost(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  addReaction(postId: string): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(`${this.apiUrl}/${postId}/heart`, {});
  }

  removeReaction(postId: string): Observable<CommandResponse> {
    return this.http.delete<CommandResponse>(`${this.apiUrl}/${postId}/heart`);
  }

  getComments(postId: string, cursor?: string, pageSize: number = 20): Observable<CursorPagedResult<PostComment>> {
    let params = new HttpParams().set('pageSize', pageSize.toString());
    if (cursor) {
      params = params.set('cursor', cursor);
    }

    return this.http.get<CursorPagedResult<any>>(`${this.apiUrl}/${postId}/comments`, { params }).pipe(
      map(response => ({
        ...response,
        nextCursor: response.nextCursor || null,
        items: response.items.map((comment: any) => this.mapPostComment(comment))
      }))
    );
  }

  addComment(postId: string, content: string, parentCommentId?: string): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(`${this.apiUrl}/${postId}/comments`, {
      content,
      parentCommentId
    });
  }

  deleteComment(commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/comments/${commentId}`);
  }

  private mapFeedPost(post: any): FeedPost {
    return {
      ...post,
      createdAt: new Date(post.createdAt),
      outfit: post.outfit ? {
        ...post.outfit,
        createdAt: new Date(post.outfit.createdAt),
        lastWorn: new Date(post.outfit.lastWorn)
      } : undefined,
      poll: post.poll ? {
        ...post.poll,
        createdAt: new Date(post.poll.createdAt),
        expiresAt: new Date(post.poll.expiresAt)
      } : undefined
    };
  }

  private mapPostComment(comment: any): PostComment {
    return {
      ...comment,
      createdAt: new Date(comment.createdAt),
      replies: comment.replies ? comment.replies.map((r: any) => this.mapPostComment(r)) : []
    };
  }
}
