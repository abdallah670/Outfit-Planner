import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { AiActions } from './ai.actions';
import { catchError, map, mergeMap, of } from 'rxjs';
import { AiUseCases } from '../../../domain/usecases/ai.usecases';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ChatResponse } from '../../../domain/entities/ai.entity';

@Injectable()
export class AiEffects {
  private actions$ = inject(Actions);
  private aiUseCases = inject(AiUseCases);
  private snackBar = inject(MatSnackBar);

  sendMessage$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AiActions.sendMessage),
      mergeMap((action: ReturnType<typeof AiActions.sendMessage>) =>
        this.aiUseCases.sendMessage(action.message, action.sessionId).pipe(
          map((response: ChatResponse) => AiActions.sendMessageSuccess({ response })),
          catchError((error) => {
            this.snackBar.open('Failed to send message', 'Close', { duration: 3000 });
            return of(AiActions.sendMessageFailure({ error: error.message }));
          })
        )
      )
    )
  );

  loadSessions$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AiActions.loadSessions),
      mergeMap((action: ReturnType<typeof AiActions.loadSessions>) =>
        this.aiUseCases.getSessions().pipe(
          map((sessions) => AiActions.loadSessionsSuccess({ sessions })),
          catchError((error) => of(AiActions.loadSessionsFailure({ error: error.message })))
        )
      )
    )
  );
}