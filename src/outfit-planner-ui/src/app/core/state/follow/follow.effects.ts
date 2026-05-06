import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { FollowActions } from './follow.actions';
import { catchError, map, mergeMap, of, Observable } from 'rxjs';
import { FollowUseCases } from '../../../domain/usecases/follow.usecases';

@Injectable()
export class FollowEffects {
  private actions$ = inject(Actions);
  private followUseCases = inject(FollowUseCases);

  followUser$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FollowActions.followUser),
        mergeMap((action: ReturnType<typeof FollowActions.followUser>) =>
          this.followUseCases.followUser(action.userId).pipe(
            map(() => FollowActions.followUserSuccess({ userId: action.userId })),
            catchError((error) =>
              of(
                FollowActions.followUserFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  unfollowUser$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FollowActions.unfollowUser),
        mergeMap((action: ReturnType<typeof FollowActions.unfollowUser>) =>
          this.followUseCases.unfollowUser(action.userId).pipe(
            map(() => FollowActions.unfollowUserSuccess({ userId: action.userId })),
            catchError((error) =>
              of(
                FollowActions.unfollowUserFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  checkFollowStatus$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FollowActions.checkFollowStatus),
        mergeMap((action: ReturnType<typeof FollowActions.checkFollowStatus>) =>
          this.followUseCases.isFollowing(action.userId).pipe(
            map((isFollowing: boolean) => FollowActions.checkFollowStatusSuccess({ userId: action.userId, isFollowing })),
            catchError((error) =>
              of(
                FollowActions.checkFollowStatusFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  loadFollowers$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FollowActions.loadFollowers),
        mergeMap((action: ReturnType<typeof FollowActions.loadFollowers>) =>
          this.followUseCases.getFollowers(action.userId, action.cursor, action.pageSize).pipe(
            map((result: any) => FollowActions.loadFollowersSuccess({ userId: action.userId, result, append: !!action.cursor })),
            catchError((error) =>
              of(
                FollowActions.loadFollowersFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  loadFollowing$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FollowActions.loadFollowing),
        mergeMap((action: ReturnType<typeof FollowActions.loadFollowing>) =>
          this.followUseCases.getFollowing(action.userId, action.cursor, action.pageSize).pipe(
            map((result: any) => FollowActions.loadFollowingSuccess({ userId: action.userId, result, append: !!action.cursor })),
            catchError((error) =>
              of(
                FollowActions.loadFollowingFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  loadFollowStats$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(FollowActions.loadFollowStats),
        mergeMap((action: ReturnType<typeof FollowActions.loadFollowStats>) =>
          this.followUseCases.getFollowStats(action.userId).pipe(
            map((stats: any) => FollowActions.loadFollowStatsSuccess({ userId: action.userId, stats })),
            catchError((error) =>
              of(
                FollowActions.loadFollowStatsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );
}
