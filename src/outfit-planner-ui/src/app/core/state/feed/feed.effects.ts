import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { FeedActions } from './feed.actions';
import { catchError, map, mergeMap, of, Observable, tap } from 'rxjs';
import { FeedUseCases } from '../../../domain/usecases/feed.usecases';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FeedPost, PostComment } from '../../../domain/entities/feed.entity';
import { CursorPagedResult } from '../../../domain/entities/response.entity';

@Injectable()
export class FeedEffects {
  private actions$ = inject(Actions);
  private feedUseCases = inject(FeedUseCases);
  private snackBar = inject(MatSnackBar);

  loadPosts$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FeedActions.loadPosts),
      mergeMap((action: ReturnType<typeof FeedActions.loadPosts>) =>
        this.feedUseCases.getFeedPosts(action.cursor, action.pageSize, action.visibility, action.sortBy, action.postType).pipe(
          map((result: CursorPagedResult<FeedPost>) =>
            FeedActions.loadPostsSuccess({ result, append: !!action.cursor })
          ),
          catchError((error) => of(FeedActions.loadPostsFailure({ error: error.message })))
        )
      )
    )
  );

  loadPostById$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FeedActions.loadPostById),
      mergeMap((action: ReturnType<typeof FeedActions.loadPostById>) =>
        this.feedUseCases.getPostById(action.id).pipe(
          map((post: FeedPost) => FeedActions.loadPostByIdSuccess({ post })),
          catchError((error) => of(FeedActions.loadPostByIdFailure({ error: error.message })))
        )
      )
    )
  );

  addReaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FeedActions.addReaction),
      mergeMap((action: ReturnType<typeof FeedActions.addReaction>) =>
        this.feedUseCases.addReaction(action.postId).pipe(
          map(() => FeedActions.addReactionSuccess({ postId: action.postId })),
          catchError((error) => of(FeedActions.addReactionFailure({ error: error.message })))
        )
      )
    )
  );

  removeReaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FeedActions.removeReaction),
      mergeMap((action: ReturnType<typeof FeedActions.removeReaction>) =>
        this.feedUseCases.removeReaction(action.postId).pipe(
          map(() => FeedActions.removeReactionSuccess({ postId: action.postId })),
          catchError((error) => of(FeedActions.removeReactionFailure({ error: error.message })))
        )
      )
    )
  );

  loadComments$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FeedActions.loadComments),
      mergeMap((action: ReturnType<typeof FeedActions.loadComments>) =>
        this.feedUseCases.getComments(action.postId, action.cursor, action.pageSize).pipe(
          map((result: CursorPagedResult<PostComment>) =>
            FeedActions.loadCommentsSuccess({ postId: action.postId, result, append: !!action.cursor })
          ),
          catchError((error) => of(FeedActions.loadCommentsFailure({ error: error.message })))
        )
      )
    )
  );

  addComment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FeedActions.addComment),
      mergeMap((action: ReturnType<typeof FeedActions.addComment>) =>
        this.feedUseCases.addComment(action.postId, action.content, action.parentCommentId).pipe(
          map((response) => FeedActions.addCommentSuccess({ postId: action.postId, comment: response })),
          catchError((error) => of(FeedActions.addCommentFailure({ error: error.message })))
        )
      )
    )
  );

  deletePost$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FeedActions.deletePost),
      mergeMap((action: ReturnType<typeof FeedActions.deletePost>) =>
        this.feedUseCases.deletePost(action.id).pipe(
          map(() => FeedActions.deletePostSuccess({ id: action.id })),
          catchError((error) => of(FeedActions.deletePostFailure({ error: error.message })))
        )
      )
    )
  );

  deleteComment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(FeedActions.deleteComment),
      mergeMap((action: ReturnType<typeof FeedActions.deleteComment>) =>
        this.feedUseCases.deleteComment(action.commentId).pipe(
          map(() => FeedActions.deleteCommentSuccess({ commentId: action.commentId, postId: action.postId })),
          catchError((error) => of(FeedActions.deleteCommentFailure({ error: error.message })))
        )
      )
    )
  );

}
