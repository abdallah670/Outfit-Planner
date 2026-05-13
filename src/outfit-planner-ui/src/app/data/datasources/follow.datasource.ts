import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
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

  getFollowers(userId: string, cursor?: string, pageSize: number = 20): Observable<CursorPagedResult<Follower>> {
    let params = new HttpParams().set('pageSize', pageSize.toString());
    if (cursor) {
      params = params.set('cursor', cursor);
    }
    return this.http.get<CursorPagedResult<Follower>>(`${this.apiUrl}/${userId}/followers`, { params });
  }

  getFollowing(userId: string, cursor?: string, pageSize: number = 20): Observable<CursorPagedResult<Following>> {
    let params = new HttpParams().set('pageSize', pageSize.toString());
    if (cursor) {
      params = params.set('cursor', cursor);
    }
    return this.http.get<CursorPagedResult<Following>>(`${this.apiUrl}/${userId}/following`, { params });
  }

  getFollowStats(userId: string): Observable<FollowStats> {
    return this.http.get<FollowStats>(`${this.apiUrl}/${userId}/stats`);
  }
}
