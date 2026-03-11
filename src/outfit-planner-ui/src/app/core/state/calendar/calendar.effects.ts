import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { CalendarActions } from './calendar.actions';
import { catchError, map, mergeMap, of, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { WearEventUseCases } from '../../../domain/usecases/wear-event.usecases';
import { WearEvent, CalendarEvent, MonthlyStats } from '../../../domain/entities/wear-event.entity';

@Injectable()
export class CalendarEffects {
  private actions$ = inject(Actions);
  private wearEventUseCases = inject(WearEventUseCases);
  private router = inject(Router);

  loadScheduledOutfits$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.loadScheduledOutfits),
        mergeMap((action: ReturnType<typeof CalendarActions.loadScheduledOutfits>) =>
          this.wearEventUseCases.getScheduledOutfits(action.year, action.month).pipe(
            map((events: CalendarEvent[]) => CalendarActions.loadScheduledOutfitsSuccess({ events })),
            catchError((error) =>
              of(
                CalendarActions.loadScheduledOutfitsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  loadMonthlyStats$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.loadMonthlyStats),
        mergeMap((action: ReturnType<typeof CalendarActions.loadMonthlyStats>) =>
          this.wearEventUseCases.getMonthlyStats(action.year, action.month).pipe(
            map((stats: MonthlyStats) => CalendarActions.loadMonthlyStatsSuccess({ stats })),
            catchError((error) =>
              of(
                CalendarActions.loadMonthlyStatsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  recordWearEvent$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.recordWearEvent),
        mergeMap((action: ReturnType<typeof CalendarActions.recordWearEvent>) =>
          this.wearEventUseCases.recordWearEvent(action.request).pipe(
            map((event: WearEvent) => CalendarActions.recordWearEventSuccess({ event })),
            catchError((error) =>
              of(
                CalendarActions.recordWearEventFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  recordWearEventSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.recordWearEventSuccess),
        tap(() => {
          Swal.fire({
            title: 'Success!',
            text: 'Wear event recorded successfully.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          });
        }),
      ),
    { dispatch: false },
  );

  scheduleOutfit$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.scheduleOutfit),
        mergeMap((action: ReturnType<typeof CalendarActions.scheduleOutfit>) =>
          this.wearEventUseCases.scheduleOutfit(action.request).pipe(
            map((event: CalendarEvent) => CalendarActions.scheduleOutfitSuccess({ event })),
            catchError((error) =>
              of(
                CalendarActions.scheduleOutfitFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  scheduleOutfitSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.scheduleOutfitSuccess),
        tap(() => {
          Swal.fire({
            title: 'Success!',
            text: 'Outfit scheduled successfully.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          });
        }),
      ),
    { dispatch: false },
  );

  markAsWorn$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.markAsWorn),
        mergeMap((action: ReturnType<typeof CalendarActions.markAsWorn>) =>
          this.wearEventUseCases.markAsWorn(action.eventId).pipe(
            map((event: WearEvent) => CalendarActions.markAsWornSuccess({ event })),
            catchError((error) =>
              of(
                CalendarActions.markAsWornFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  deleteWearEvent$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.deleteWearEvent),
        mergeMap((action: ReturnType<typeof CalendarActions.deleteWearEvent>) =>
          this.wearEventUseCases.deleteWearEvent(action.eventId).pipe(
            map(() => CalendarActions.deleteWearEventSuccess({ eventId: action.eventId })),
            catchError((error) =>
              of(
                CalendarActions.deleteWearEventFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  deleteWearEventSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.deleteWearEventSuccess),
        tap(() => {
          Swal.fire({
            title: 'Deleted!',
            text: 'Event has been deleted.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          });
        }),
      ),
    { dispatch: false },
  );
}
