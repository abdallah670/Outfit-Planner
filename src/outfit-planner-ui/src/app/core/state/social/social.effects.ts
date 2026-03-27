import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { SocialActions } from './social.actions';
import { catchError, map, mergeMap, of, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { SocialUseCases } from '../../../domain/usecases/social.usecases';
import { ValidationPoll, CommandResponse } from '../../../domain/entities/validation-poll.entity';
import { TrendingOutfit, OutfitComment } from '../../../domain/entities/social-engagement.entity';

@Injectable()
export class SocialEffects {
  private actions$ = inject(Actions);
  private socialUseCases = inject(SocialUseCases);
  private router = inject(Router);

  loadPolls$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.loadPolls),
        mergeMap(() =>
          this.socialUseCases.getPolls().pipe(
            map((polls: ValidationPoll[]) => SocialActions.loadPollsSuccess({ polls })),
            catchError((error) =>
              of(
                SocialActions.loadPollsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  loadPollById$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.loadPollById),
        mergeMap((action: ReturnType<typeof SocialActions.loadPollById>) =>
          this.socialUseCases.getPollById(action.id).pipe(
            map((poll: ValidationPoll) => SocialActions.loadPollByIdSuccess({ poll })),
            catchError((error) =>
              of(
                SocialActions.loadPollByIdFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  createPoll$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.createPoll),
        mergeMap((action: ReturnType<typeof SocialActions.createPoll>) =>
          this.socialUseCases.createPoll(action.request).pipe(
            map((response: CommandResponse) => SocialActions.createPollSuccess({ pollId: response.id })),
            catchError((err) => {
              let errorMessage = err.message || 'Failed to create poll';
              if (err.error?.errors) {
                errorMessage = Object.values(err.error.errors).flat().join('\n');
              } else if (err.error?.title) {
                errorMessage = err.error.title;
              }
              return of(
                SocialActions.createPollFailure({
                  error: errorMessage,
                }),
              );
            }),
          ),
        ),
      ) as Observable<Action>,
  );

  createPollSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.createPollSuccess),
        tap(() => {
          Swal.fire({
            title: 'Success!',
            text: 'Your poll has been created.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          }).then(() => {
            this.router.navigate(['/social']);
          });
        }),
      ),
    { dispatch: false },
  );

  createPollFailure$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.createPollFailure),
        tap(({ error }: { error: string }) => {
          Swal.fire({
            title: 'Error!',
            text: error,
            icon: 'error',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#ef4444',
          });
        }),
      ),
    { dispatch: false },
  );

  vote$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.vote),
        mergeMap((action: ReturnType<typeof SocialActions.vote>) =>
          this.socialUseCases.voteOnPoll(action.pollId, action.request).pipe(
            map(() => SocialActions.voteSuccess({ pollId: action.pollId })),
            catchError((error) =>
              of(
                SocialActions.voteFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  voteSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.voteSuccess),
        tap(() => {
          Swal.fire({
            title: 'Vote Recorded!',
            text: 'Your vote has been submitted.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
            timer: 1500,
            showConfirmButton: false,
          });
        }),
      ),
    { dispatch: false },
  );

  voteFailure$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.voteFailure),
        tap(({ error }: { error: string }) => {
          Swal.fire({
            title: 'Error!',
            text: error,
            icon: 'error',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#ef4444',
          });
        }),
      ),
    { dispatch: false },
  );

  loadTrending$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.loadTrending),
        mergeMap(() =>
          this.socialUseCases.getTrendingOutfits().pipe(
            map((outfits: TrendingOutfit[]) => SocialActions.loadTrendingSuccess({ outfits })),
            catchError((error) =>
              of(
                SocialActions.loadTrendingFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  likeOutfit$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.likeOutfit),
        mergeMap((action: ReturnType<typeof SocialActions.likeOutfit>) =>
          this.socialUseCases.likeOutfit(action.outfitId).pipe(
            map(() => SocialActions.likeOutfitSuccess({ outfitId: action.outfitId })),
            catchError((error) =>
              of(
                SocialActions.likeOutfitFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  unlikeOutfit$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.unlikeOutfit),
        mergeMap((action: ReturnType<typeof SocialActions.unlikeOutfit>) =>
          this.socialUseCases.unlikeOutfit(action.outfitId).pipe(
            map(() => SocialActions.unlikeOutfitSuccess({ outfitId: action.outfitId })),
            catchError((error) =>
              of(
                SocialActions.unlikeOutfitFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  addComment$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.addComment),
        mergeMap((action: ReturnType<typeof SocialActions.addComment>) =>
          this.socialUseCases.addComment(action.outfitId, action.content).pipe(
            map((comment: OutfitComment) => SocialActions.addCommentSuccess({ outfitId: action.outfitId, comment })),
            catchError((error) =>
              of(
                SocialActions.addCommentFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  loadComments$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.loadComments),
        mergeMap((action: ReturnType<typeof SocialActions.loadComments>) =>
          this.socialUseCases.getComments(action.outfitId).pipe(
            map((response: { items: OutfitComment[]; totalCount: number }) => SocialActions.loadCommentsSuccess({ outfitId: action.outfitId, comments: response.items })),
            catchError((error) =>
              of(
                SocialActions.loadCommentsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );
}
