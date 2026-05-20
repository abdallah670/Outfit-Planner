import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { CommandResponse } from '../../domain/entities/response.entity';
import { FeedPost, PostComment } from '../../domain/entities/feed.entity';
import { CreateOutfitPostRequest, UpdateOutfitPostRequest } from '../../domain/entities/outfitpost.entity';


@Injectable({
  providedIn: 'root',
})
export class OutfitPostsDataSource {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.baseUrl}/outfitposts`;

  createOutfitPost(dto: CreateOutfitPostRequest): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(this.apiUrl, dto);
  }

  getOutfitPost(id: string): Observable<FeedPost> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(post => this.mapFeedPost(post))
    );
  }

  updateOutfitPost(id: string, dto: UpdateOutfitPostRequest): Observable<CommandResponse> {
    return this.http.put<CommandResponse>(`${this.apiUrl}/${id}`, dto);
  }

  deleteOutfitPost(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getUserOutfitPosts(): Observable<FeedPost[]> {
    return this.http.get<any[]>(`${this.apiUrl}/my`).pipe(
      map(posts => posts.map(post => this.mapFeedPost(post)))
    );
  }

  private fixUrl(url: string | null | undefined): string {
    if (!url) return '';
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    const path = url.startsWith('/') ? url : `/${url}`;
    return `${environment.resourceBaseUrl}${path}`;
  }

  private mapFeedPost(post: any): FeedPost {
    if (post.userAvatarUrl) {
      post.userAvatarUrl = this.fixUrl(post.userAvatarUrl);
    }

    if (post.outfit && post.outfit.imageUrl) {
      post.outfit.imageUrl = this.fixUrl(post.outfit.imageUrl);
    }

    if (post.poll && post.poll.options) {
      post.poll.options.forEach((option: any) => {
        if (option.outfitThumbnail) {
          option.outfitThumbnail = this.fixUrl(option.outfitThumbnail);
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
    if (comment.userAvatarUrl) {
      comment.userAvatarUrl = this.fixUrl(comment.userAvatarUrl);
    }
    
    return {
      ...comment,
      createdAt: new Date(comment.createdAt),
      replies: comment.replies ? comment.replies.map((r: any) => this.mapPostComment(r)) : []
    };
  }
}
