import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { FeedPost } from '../../domain/entities/feed.entity';
import { PostComment } from '../../domain/entities/feed.entity';
import { CommandResponse,CursorPagedResult } from '../../domain/entities/response.entity';
import { VoterInfo } from '../../domain/entities/poll.entity';


@Injectable({
  providedIn: 'root',
})
export class FeedDataSource {
  private readonly apiUrl = `${environment.baseUrl}/feed`;

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

  updateComment(commentId: string, content: string): Observable<CommandResponse> {
    return this.http.put<CommandResponse>(`${this.apiUrl}/comments/${commentId}`, { content });
  }

  getVotersForPoll(pollId: string, optionId?: string): Observable<VoterInfo[]> {
    let url = `${environment.baseUrl}/polls/${pollId}/voters`;
    let params = new HttpParams();
    if (optionId) {
      params = params.set('optionId', optionId);
    }

    return this.http.get<any[]>(url, { params }).pipe(
      map(votes => votes.map((v: any) => ({
        voterId: v.voterId,
        voterName: v.voterName,
        voterAvatarUrl: v.voterAvatarUrl ? (v.voterAvatarUrl.startsWith('http') ? v.voterAvatarUrl : `${environment.resourceBaseUrl}${v.voterAvatarUrl}`) : 'assets/default-avatar.png',
        votedAt: new Date(v.votedAt),
        optionId: v.optionId,
        optionDescription: v.optionDescription || '',
        optionDisplayOrder: v.optionDisplayOrder ?? 0
      })))
    );
  }

  private mapFeedPost(post: any): FeedPost {
    const resourceUrl = environment.resourceBaseUrl;
    
    // Prefix user avatar
    if (post.userAvatarUrl && !post.userAvatarUrl.startsWith('http')) {
      post.userAvatarUrl = `${resourceUrl}${post.userAvatarUrl}`;
    }

    // Prefix outfit image
    if (post.outfit && post.outfit.imageUrl && !post.outfit.imageUrl.startsWith('http')) {
      post.outfit.imageUrl = `${resourceUrl}${post.outfit.imageUrl}`;
    }

    // Prefix poll option thumbnails
    if (post.poll && post.poll.options) {
      post.poll.options.forEach((option: any) => {
        if (option.outfitThumbnail && !option.outfitThumbnail.startsWith('http')) {
          option.outfitThumbnail = `${resourceUrl}${option.outfitThumbnail}`;
        }
      });
    }

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
        status: post.poll.status?.toLowerCase(),
        createdAt: new Date(post.poll.createdAt),
        expiresAt: new Date(post.poll.expiresAt),
        userVotedOptionId: post.poll.userVotedOptionId
      } : undefined,
      isFollowing: post.isFollowing,
      isLiked: post.isLiked,
      isOwner: post.isOwner,
      hasVoted: post.hasVoted,
      comments: post.comments ? post.comments.map((c: any) => this.mapPostComment(c)) : undefined
    };
  }

  private mapPostComment(comment: any): PostComment {
    const resourceUrl = environment.resourceBaseUrl;
    
    // Prefix user avatar if it's a relative URL
    if (comment.userAvatarUrl && !comment.userAvatarUrl.startsWith('http')) {
      comment.userAvatarUrl = `${resourceUrl}${comment.userAvatarUrl}`;
    }
    
    return {
      ...comment,
      createdAt: new Date(comment.createdAt),
      replies: comment.replies ? comment.replies.map((r: any) => this.mapPostComment(r)) : []
    };
  }
}
