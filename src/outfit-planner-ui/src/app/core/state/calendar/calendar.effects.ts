import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action, Store } from '@ngrx/store';
import { CalendarActions } from './calendar.actions';
import { catchError, map, mergeMap, of, Observable, tap, withLatestFrom } from 'rxjs';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { WearEventUseCases } from '../../../domain/usecases/wear-event.usecases';
import { WearEvent, CalendarEvent, MonthlyStats, CalendarEventItem, CreateCalendarEventRequest } from '../../../domain/entities/wear-event.entity';
import { WeatherService } from '../../services/weather.service';
import { selectCurrentYear, selectCurrentMonth } from './calendar.selectors';

@Injectable()
export class CalendarEffects {
  private actions$ = inject(Actions);
  private store = inject(Store);
  private wearEventUseCases = inject(WearEventUseCases);
  private weatherService = inject(WeatherService);
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

  // FIXED: Reload calendar data after scheduling outfit
  scheduleOutfitSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.scheduleOutfitSuccess),
        withLatestFrom(
          this.store.select(selectCurrentYear),
          this.store.select(selectCurrentMonth)
        ),
        tap(([_action, year, month]: [ReturnType<typeof CalendarActions.scheduleOutfitSuccess>, number, number]) => {
          Swal.fire({
            title: 'Success!',
            text: 'Outfit scheduled successfully.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          });
          // Reload calendar data
          this.store.dispatch(CalendarActions.loadScheduledOutfits({ year, month }));
          this.store.dispatch(CalendarActions.loadMonthlyStats({ year, month }));
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

  // FIXED: Reload calendar data after marking as worn
  markAsWornSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.markAsWornSuccess),
        withLatestFrom(
          this.store.select(selectCurrentYear),
          this.store.select(selectCurrentMonth)
        ),
        tap(([_action, year, month]: [ReturnType<typeof CalendarActions.markAsWornSuccess>, number, number]) => {
          Swal.fire({
            title: 'Success!',
            text: 'Outfit marked as worn successfully.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          });
          // Reload calendar data to refresh stats and view
          this.store.dispatch(CalendarActions.loadScheduledOutfits({ year, month }));
          this.store.dispatch(CalendarActions.loadMonthlyStats({ year, month }));
        }),
      ),
    { dispatch: false },
  );

  markAsWornFailure$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.markAsWornFailure),
        tap((action: ReturnType<typeof CalendarActions.markAsWornFailure>) => {
          Swal.fire({
            title: 'Error!',
            text: `Failed to mark outfit as worn: ${action.error}`,
            icon: 'error',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          });
        }),
      ),
    { dispatch: false },
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

  // FIXED: Reload calendar data after deleting wear event
  deleteWearEventSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.deleteWearEventSuccess),
        withLatestFrom(
          this.store.select(selectCurrentYear),
          this.store.select(selectCurrentMonth)
        ),
        tap(([_action, year, month]: [ReturnType<typeof CalendarActions.deleteWearEventSuccess>, number, number]) => {
          Swal.fire({
            title: 'Deleted!',
            text: 'Event has been deleted.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          });
          // Reload calendar data
          this.store.dispatch(CalendarActions.loadScheduledOutfits({ year, month }));
          this.store.dispatch(CalendarActions.loadMonthlyStats({ year, month }));
        }),
      ),
    { dispatch: false },
  );

  // ==================== Calendar Events (Time-based) ====================

  loadCalendarEvents$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.loadCalendarEvents),
        mergeMap((action: ReturnType<typeof CalendarActions.loadCalendarEvents>) =>
          this.wearEventUseCases.getCalendarEventsForMonth(action.year, action.month).pipe(
            map((events: CalendarEventItem[]) => CalendarActions.loadCalendarEventsSuccess({ events })),
            catchError((error) =>
              of(
                CalendarActions.loadCalendarEventsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  createCalendarEvent$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.createCalendarEvent),
        mergeMap((action: ReturnType<typeof CalendarActions.createCalendarEvent>) =>
          this.wearEventUseCases.createCalendarEvent(action.event as CreateCalendarEventRequest).pipe(
            map((event: CalendarEventItem) => CalendarActions.createCalendarEventSuccess({ event })),
            catchError((error) =>
              of(
                CalendarActions.createCalendarEventFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  // FIXED: Reload calendar events after creating event
  createCalendarEventSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.createCalendarEventSuccess),
        withLatestFrom(
          this.store.select(selectCurrentYear),
          this.store.select(selectCurrentMonth)
        ),
        tap(([_action, year, month]: [ReturnType<typeof CalendarActions.createCalendarEventSuccess>, number, number]) => {
          Swal.fire({
            title: 'Success!',
            text: 'Calendar event created successfully.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          });
          // Reload calendar events
          this.store.dispatch(CalendarActions.loadCalendarEvents({ year, month }));
        }),
      ),
    { dispatch: false },
  );

  updateCalendarEvent$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.updateCalendarEvent),
        mergeMap((action: ReturnType<typeof CalendarActions.updateCalendarEvent>) =>
          this.wearEventUseCases.updateCalendarEvent(action.eventId, action.event).pipe(
            map((event: CalendarEventItem) => CalendarActions.updateCalendarEventSuccess({ event })),
            catchError((error) =>
              of(
                CalendarActions.updateCalendarEventFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  // FIXED: Reload calendar events after updating event
  updateCalendarEventSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.updateCalendarEventSuccess),
        withLatestFrom(
          this.store.select(selectCurrentYear),
          this.store.select(selectCurrentMonth)
        ),
        tap(([_action, year, month]: [ReturnType<typeof CalendarActions.updateCalendarEventSuccess>, number, number]) => {
          Swal.fire({
            title: 'Updated!',
            text: 'Calendar event updated successfully.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          });
          // Reload calendar events
          this.store.dispatch(CalendarActions.loadCalendarEvents({ year, month }));
        }),
      ),
    { dispatch: false },
  );

  deleteCalendarEvent$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.deleteCalendarEvent),
        mergeMap((action: ReturnType<typeof CalendarActions.deleteCalendarEvent>) =>
          this.wearEventUseCases.deleteCalendarEvent(action.eventId).pipe(
            map(() => CalendarActions.deleteCalendarEventSuccess({ eventId: action.eventId })),
            catchError((error) =>
              of(
                CalendarActions.deleteCalendarEventFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  // FIXED: Reload calendar events after deleting event
  deleteCalendarEventSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.deleteCalendarEventSuccess),
        withLatestFrom(
          this.store.select(selectCurrentYear),
          this.store.select(selectCurrentMonth)
        ),
        tap(([_action, year, month]: [ReturnType<typeof CalendarActions.deleteCalendarEventSuccess>, number, number]) => {
          Swal.fire({
            title: 'Deleted!',
            text: 'Calendar event has been deleted.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          });
          // Reload calendar events
          this.store.dispatch(CalendarActions.loadCalendarEvents({ year, month }));
        }),
      ),
    { dispatch: false },
  );

  // ==================== Weather ====================

  loadWeatherForecast$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(CalendarActions.loadWeatherForecast),
        mergeMap((action: ReturnType<typeof CalendarActions.loadWeatherForecast>) =>
          this.weatherService.getWeatherForMonth(action.year, action.month).pipe(
            map((weatherData: Map<string, import('../../../domain/entities/wear-event.entity').WeatherData>) => 
              CalendarActions.loadWeatherForecastSuccess({ weatherData })
            ),
            catchError((error) =>
              of(
                CalendarActions.loadWeatherForecastFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );
}
