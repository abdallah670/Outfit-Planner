import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Follower, Following, FollowStats } from '../../domain/entities/follow.entity';
import { CursorPagedResult } from '../../domain/entities/response.entity';

@Injectable({
  providedIn: 'root',
})
export class FollowDataSource {
  private http = inject(HttpClient);
  private apiUrl = `${environment.baseUrl}/user/users`;

  followUser(userId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${userId}/follow`, {});
  }

  unfollowUser(userId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${userId}/unfollow`);
  }
  
  isFollowing(userId: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/${userId}/isfollowing`);
  }

  getFollowers(userId: string, cursor?: string, pageSize: number = 20,searchQuery?: string): Observable<CursorPagedResult<Follower>> {
    let params = new HttpParams().set('pageSize', pageSize.toString());
    if (cursor) {
      params = params.set('cursor', cursor);
    }
    if (searchQuery) {
      params = params.set('searchQuery', searchQuery);
    }
    return this.http.get<CursorPagedResult<any>>(`${this.apiUrl}/${userId}/followers`, { params }).pipe(
      map(res => ({
        ...res,
        items: (res.items || []).map((f: any) => ({
          id: f.id,
          userId: f.userId,
          userName: f.userName,
          userAvatarUrl: f.avatarUrl ? this.fixUrl(f.avatarUrl) : 'assets/default-avatar.png',
          fullName: f.fullName || f.userName,
          createdAt: new Date(f.createdAt),
          isFollowing: f.isFollowing,
          isOwner: f.isOwner
        }))
      }))
    );
  }

  getFollowing(userId: string, cursor?: string, pageSize: number = 20,searchQuery?: string): Observable<CursorPagedResult<Following>> {
    let params = new HttpParams().set('pageSize', pageSize.toString());
    if (cursor) {
      params = params.set('cursor', cursor);
    }
    if (searchQuery) {
      params = params.set('searchQuery', searchQuery);
    }
    return this.http.get<CursorPagedResult<any>>(`${this.apiUrl}/${userId}/following`, { params }).pipe(
      map(res => ({
        ...res,
        items: (res.items || []).map((f: any) => ({
          id: f.id,
          userId: f.userId,
          userName: f.userName,
          userAvatarUrl: f.avatarUrl ? this.fixUrl(f.avatarUrl) : 'assets/default-avatar.png',
          fullName: f.fullName || f.userName,
          createdAt: new Date(f.createdAt),
          isFollowing: f.isFollowing,
          isOwner: f.isOwner
        }))
      }))
    );
  }

  getFollowStats(userId: string): Observable<FollowStats> {
    return this.http.get<FollowStats>(`${this.apiUrl}/${userId}/stats`);
  }

  private fixUrl(url: string | null | undefined): string {
    if (!url) return '';
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    const path = url.startsWith('/') ? url : `/${url}`;
    return `${environment.resourceBaseUrl}${path}`;
  }
}
