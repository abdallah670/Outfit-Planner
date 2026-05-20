import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { Follower, Following, FollowStats } from '../entities/follow.entity';
import { CursorPagedResult } from '../entities/response.entity';

export const FOLLOW_REPOSITORY = new InjectionToken<FollowRepository>('FollowRepository');

export interface FollowRepository {
  followUser(userId: string): Observable<any>;
  unfollowUser(userId: string): Observable<any>;
  isFollowing(userId: string): Observable<boolean>;
  getFollowers(userId: string, cursor?: string, pageSize?: number,searchQuery?: string): Observable<CursorPagedResult<Follower>>;
  getFollowing(userId: string, cursor?: string, pageSize?: number,searchQuery?: string): Observable<CursorPagedResult<Following>>;
  getFollowStats(userId: string): Observable<FollowStats>;
}
