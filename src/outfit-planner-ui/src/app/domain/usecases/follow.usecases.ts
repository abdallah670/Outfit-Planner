import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FOLLOW_REPOSITORY, FollowRepository } from '../repositories/follow.repository';
import { Follower, Following, FollowStats } from '../entities/follow.entity';
import { CursorPagedResult } from '../entities/response.entity';

@Injectable({
  providedIn: 'root',
})
export class FollowUseCases {
  constructor(
    @Inject(FOLLOW_REPOSITORY) private readonly followRepository: FollowRepository,
  ) {}

  followUser(userId: string): Observable<any> {
    return this.followRepository.followUser(userId);
  }

  unfollowUser(userId: string): Observable<any> {
    return this.followRepository.unfollowUser(userId);
  }

  isFollowing(userId: string): Observable<boolean> {
    return this.followRepository.isFollowing(userId);
  }

  getFollowers(userId: string, cursor?: string, pageSize?: number): Observable<CursorPagedResult<Follower>> {
    return this.followRepository.getFollowers(userId, cursor, pageSize);
  }

  getFollowing(userId: string, cursor?: string, pageSize?: number): Observable<CursorPagedResult<Following>> {
    return this.followRepository.getFollowing(userId, cursor, pageSize);
  }

  getFollowStats(userId: string): Observable<FollowStats> {
    return this.followRepository.getFollowStats(userId);
  }
}
