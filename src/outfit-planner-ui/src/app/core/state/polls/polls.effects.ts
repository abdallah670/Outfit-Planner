import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { PollsActions } from './polls.actions';
import { catchError, map, mergeMap, of, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { PollsUseCases } from '../../../domain/usecases/polls.usecases';
import { Poll } from '../../../domain/entities/poll.entity';
import { CommandResponse } from '../../../domain/entities/response.entity';

@Injectable()
export class PollsEffects {
  private actions$ = inject(Actions);
  private pollsUseCases = inject(PollsUseCases);
  private router = inject(Router);

  loadPolls$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.loadPolls),
        mergeMap(() =>
          this.pollsUseCases.getPolls().pipe(
            map((polls: Poll[]) => PollsActions.loadPollsSuccess({ polls })),
            catchError((error) =>
              of(
                PollsActions.loadPollsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  loadPollById$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.loadPollById),
        mergeMap((action: ReturnType<typeof PollsActions.loadPollById>) =>
          this.pollsUseCases.getPollById(action.id).pipe(
            map((poll: Poll) => PollsActions.loadPollByIdSuccess({ poll })),
            catchError((error) =>
              of(
                PollsActions.loadPollByIdFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  loadUserPolls$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.loadUserPolls),
        mergeMap(() =>
          this.pollsUseCases.getMyPolls().pipe(
            map((polls: Poll[]) => PollsActions.loadUserPollsSuccess({ polls })),
            catchError((error) =>
              of(
                PollsActions.loadUserPollsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  updatePoll$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.updatePoll),
        mergeMap((action: ReturnType<typeof PollsActions.updatePoll>) =>
          this.pollsUseCases.updatePoll(action.pollId, action.request).pipe(
            map((poll: Poll) => PollsActions.updatePollSuccess({ poll })),
            catchError((error) =>
              of(
                PollsActions.updatePollFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  updatePollSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.updatePollSuccess),
        tap(() => {
          Swal.fire({
            title: 'Success!',
            text: 'Your poll has been updated.',
            icon: 'success',
            timer: 2000,
            showConfirmButton: false,
          });
        }),
      ),
    { dispatch: false },
  );

  deletePoll$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.deletePoll),
        mergeMap((action: ReturnType<typeof PollsActions.deletePoll>) =>
          this.pollsUseCases.deletePoll(action.pollId).pipe(
            map(() => PollsActions.deletePollSuccess({ pollId: action.pollId })),
            catchError((error) =>
              of(
                PollsActions.deletePollFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  deletePollSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.deletePollSuccess),
        tap(() => {
          Swal.fire({
            title: 'Deleted!',
            text: 'Your poll has been deleted.',
            icon: 'success',
            timer: 2000,
            showConfirmButton: false,
          });
        }),
      ),
    { dispatch: false },
  );

  closePoll$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.closePoll),
        mergeMap((action: ReturnType<typeof PollsActions.closePoll>) =>
          this.pollsUseCases.closePoll(action.pollId).pipe(
            map(() => PollsActions.closePollSuccess({ pollId: action.pollId })),
            catchError((error) =>
              of(
                PollsActions.closePollFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  closePollSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.closePollSuccess),
        tap(() => {
          Swal.fire({
            title: 'Closed!',
            text: 'Your poll has been closed to new votes.',
            icon: 'info',
            timer: 2000,
            showConfirmButton: false,
          });
        }),
      ),
    { dispatch: false },
  );

  createPoll$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.createPoll),
        mergeMap((action: ReturnType<typeof PollsActions.createPoll>) =>
          this.pollsUseCases.createPoll(action.request).pipe(
            map((response: CommandResponse) => PollsActions.createPollSuccess({ pollId: response.id })),
            catchError((err) => {
              let errorMessage = err.message || 'Failed to create poll';
              if (err.error?.errors) {
                errorMessage = Object.values(err.error.errors).flat().join('\n');
              }
              return of(PollsActions.createPollFailure({ error: errorMessage }));
            }),
          ),
        ),
      ),
  );

  createPollSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.createPollSuccess),
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

  vote$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.vote),
        mergeMap((action: ReturnType<typeof PollsActions.vote>) =>
          this.pollsUseCases.voteOnPoll(action.pollId, action.request).pipe(
            map(() => PollsActions.voteSuccess({ pollId: action.pollId })),
            catchError((error) =>
              of(
                PollsActions.voteFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );

  voteSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.voteSuccess),
        tap(() => {
          Swal.fire({
            title: 'Vote Recorded!',
            text: 'Your vote has been submitted.',
            icon: 'success',
            timer: 1500,
            showConfirmButton: false,
          });
        }),
      ),
    { dispatch: false },
  );

  loadRecentPoll$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.loadRecentPoll),
        mergeMap((action) =>
          this.pollsUseCases.getRecentPollWithComments(undefined, action.commentsPageSize).pipe(
            map(({ poll, comments, commentsCursor, hasMoreComments }) => 
              PollsActions.loadRecentPollSuccess({ poll, comments, commentsCursor, hasMoreComments })),
            catchError((error) => of(PollsActions.loadRecentPollFailure({ error: error.message })))
          )
        )
      )
  );

  loadMorePollComments$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(PollsActions.loadMorePollComments),
        mergeMap((action) =>
          this.pollsUseCases.getRecentPollWithComments(action.cursor, action.pageSize).pipe(
            map(({ comments, commentsCursor, hasMoreComments }) => 
              PollsActions.loadMorePollCommentsSuccess({ comments, commentsCursor, hasMoreComments })),
            catchError((error) => of(PollsActions.loadMorePollCommentsFailure({ error: error.message })))
          )
        )
      )
  );
}
