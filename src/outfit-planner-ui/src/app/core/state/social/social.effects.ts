import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { SocialActions } from './social.actions';
import { catchError, map, mergeMap, of, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { SocialUseCases } from '../../../domain/usecases/social.usecases';
import { ValidationPoll, CommandResponse } from '../../../domain/entities/validation-poll.entity';
import { TrendingOutfit, VoteComment, AddVoteCommentRequest } from '../../../domain/entities/social-engagement.entity';

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
        mergeMap((action: ReturnType<typeof SocialActions.loadTrending>) =>
          this.socialUseCases.getTrendingOutfits(action.page ?? 1, action.pageSize ?? 20).pipe(
            map((response: { items: TrendingOutfit[]; totalCount: number }) =>
              SocialActions.loadTrendingSuccess({ outfits: response.items, totalCount: response.totalCount })
            ),
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

  reactToVote$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.reactToVote),
        mergeMap((action: { voteId: string; reactionType: string }) =>
          this.socialUseCases.reactToVote(action.voteId, action.reactionType).pipe(
            map(() => SocialActions.reactToVoteSuccess({ voteId: action.voteId, reactionType: action.reactionType })),
            catchError((error) =>
              of(
                SocialActions.reactToVoteFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  addVoteComment$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.addVoteComment),
        mergeMap((action: { request: AddVoteCommentRequest }) =>
          this.socialUseCases.addVoteComment(action.request).pipe(
            map((comment: VoteComment) => SocialActions.addVoteCommentSuccess({ comment })),
            catchError((error) =>
              of(
                SocialActions.addVoteCommentFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  loadVoteComments$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.loadVoteComments),
        mergeMap((action: { voteId: string; maxDepth?: number }) =>
          this.socialUseCases.getVoteComments(action.voteId, action.maxDepth).pipe(
            map((comments: VoteComment[]) => SocialActions.loadVoteCommentsSuccess({ voteId: action.voteId, comments })),
            catchError((error) =>
              of(
                SocialActions.loadVoteCommentsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  likeVoteComment$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(SocialActions.likeVoteComment),
        mergeMap((action: { commentId: string }) =>
          this.socialUseCases.likeVoteComment(action.commentId).pipe(
            map(() => SocialActions.likeVoteCommentSuccess({ commentId: action.commentId })),
            catchError((error) =>
              of(
                SocialActions.likeVoteCommentFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );
}
