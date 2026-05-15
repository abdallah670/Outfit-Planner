import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { OutfitPostsActions } from './outfit-posts.actions';
import { catchError, map, mergeMap, of, Observable, tap } from 'rxjs';
import { OutfitPostsUseCases } from '../../../domain/usecases/outfit-posts.usecases';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FeedPost, PostType } from '../../../domain/entities/feed.entity';
import { CommandResponse } from '../../../domain/entities/response.entity';

@Injectable()
export class OutfitPostsEffects {
  private actions$ = inject(Actions);
  private outfitPostsUseCases = inject(OutfitPostsUseCases);
  private snackBar = inject(MatSnackBar);

  createOutfitPost$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OutfitPostsActions.createOutfitPost),
      mergeMap((action: ReturnType<typeof OutfitPostsActions.createOutfitPost>) =>
        this.outfitPostsUseCases.createOutfitPost(action).pipe(
          map((response: CommandResponse) => {
            const newPost: FeedPost = {
              id: response.id,
              userId: '', // Backend provides this
              userName: '',
              userAvatarUrl: '',
              postType: PostType.Outfit,
              outfitId: action.outfitId,
              caption: action.caption,
              visibility: action.visibility,
              likesCount: 0,
              commentsCount: 0,
              createdAt: new Date(),
              isLiked: false,
              isFollowing: false,
              isOwner: true,
              hasVoted: false
            };
            return OutfitPostsActions.createOutfitPostSuccess({ post: newPost });
          }),
          catchError((error) => of(OutfitPostsActions.createOutfitPostFailure({ error: error.message })))
        )
      )
    )
  );

  createOutfitPostSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OutfitPostsActions.createOutfitPostSuccess),
      tap(() => {
        this.snackBar.open('Shared to community feed!', 'Close', { duration: 3000 });
      }),
    ),
    { dispatch: false },
  );

  getOutfitPost$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OutfitPostsActions.getOutfitPost),
      mergeMap((action: ReturnType<typeof OutfitPostsActions.getOutfitPost>) =>
        this.outfitPostsUseCases.getOutfitPost(action.id).pipe(
          map((post: FeedPost) => OutfitPostsActions.getOutfitPostSuccess({ post })),
          catchError((error) => of(OutfitPostsActions.getOutfitPostFailure({ error: error.message })))
        )
      )
    )
  );

  updateOutfitPost$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OutfitPostsActions.updateOutfitPost),
      mergeMap((action: ReturnType<typeof OutfitPostsActions.updateOutfitPost>) =>
        this.outfitPostsUseCases.updateOutfitPost(action.id, {
          caption: action.caption,
          visibility: action.visibility
        }).pipe(
          map((response: CommandResponse) => OutfitPostsActions.updateOutfitPostSuccess({ response })),
          catchError((error) => of(OutfitPostsActions.updateOutfitPostFailure({ error: error.message })))
        )
      )
    )
  );

  deleteOutfitPost$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OutfitPostsActions.deleteOutfitPost),
      mergeMap((action: ReturnType<typeof OutfitPostsActions.deleteOutfitPost>) =>
        this.outfitPostsUseCases.deleteOutfitPost(action.id).pipe(
          map(() => OutfitPostsActions.deleteOutfitPostSuccess({ id: action.id })),
          catchError((error) => of(OutfitPostsActions.deleteOutfitPostFailure({ error: error.message })))
        )
      )
    )
  );
}
