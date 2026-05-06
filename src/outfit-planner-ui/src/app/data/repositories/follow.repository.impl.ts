import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { FollowRepository, FOLLOW_REPOSITORY } from '../../domain/repositories/follow.repository';
import { FollowDataSource } from '../../data/datasources/follow.datasource';
import { Follower, Following, FollowStats } from '../../domain/entities/follow.entity';
import { CursorPagedResult } from '../../domain/entities/response.entity';

@Injectable({
  providedIn: 'root',
})
export class FollowRepositoryImpl implements FollowRepository {
  private followDataSource = inject(FollowDataSource);

  followUser(userId: string): Observable<any> {
    return this.followDataSource.followUser(userId);
  }

  unfollowUser(userId: string): Observable<any> {
    return this.followDataSource.unfollowUser(userId);
  }

  isFollowing(userId: string): Observable<boolean> {
    return this.followDataSource.isFollowing(userId);
  }

  getFollowers(userId: string, cursor?: string, pageSize?: number): Observable<CursorPagedResult<Follower>> {
    return this.followDataSource.getFollowers(userId, cursor, pageSize);
  }

  getFollowing(userId: string, cursor?: string, pageSize?: number): Observable<CursorPagedResult<Following>> {
    return this.followDataSource.getFollowing(userId, cursor, pageSize);
  }

  getFollowStats(userId: string): Observable<FollowStats> {
    return this.followDataSource.getFollowStats(userId);
  }
}

export const followRepositoryProvider = {
  provide: FOLLOW_REPOSITORY,
  useClass: FollowRepositoryImpl,
};
