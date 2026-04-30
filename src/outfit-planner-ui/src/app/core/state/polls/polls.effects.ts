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
}
