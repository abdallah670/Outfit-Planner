import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { OutfitsActions } from './outfit.actions';

import { catchError, map, mergeMap, of, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';

import { Outfit } from '../../../domain/entities/outfit.entity';

import { OutfitsUseCases } from '../../../domain/usecases/outfit.usecases';

@Injectable()
export class OutfitEffects {
  private actions$ = inject(Actions);
  private outfitsUseCases = inject(OutfitsUseCases);
  private router = inject(Router);

  loadOutfits$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.loadOutfits),
        mergeMap(() =>
          this.outfitsUseCases.getAllOutfits().pipe(
            map((outfits: Outfit[]) => OutfitsActions.loadOutfitsSuccess({ outfits })),
            catchError((error) =>
              of(
                OutfitsActions.loadOutfitsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  loadOutfitById$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.loadOutfitById),
        mergeMap((action: ReturnType<typeof OutfitsActions.loadOutfitById>) =>
          this.outfitsUseCases.getOutfitById(action.id).pipe(
            map((outfit: Outfit) => OutfitsActions.loadOutfitByIdSuccess({ outfit })),
            catchError((error) =>
              of(
                OutfitsActions.loadOutfitByIdFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  createOutfit$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.createOutfit),
        mergeMap((action: ReturnType<typeof OutfitsActions.createOutfit>) =>
          this.outfitsUseCases.createOutfit(action.outfit).pipe(
            map((outfit: Outfit) => OutfitsActions.createOutfitSuccess({ outfit })),
            catchError((err) => {
              let errorMessage = err.message || 'Failed to add outfit';
              if (err.error?.errors) {
                errorMessage = Object.values(err.error.errors).flat().join('\n');
              } else if (err.error?.title) {
                errorMessage = err.error.title;
              }
              return of(
                OutfitsActions.createOutfitFailure({
                  error: errorMessage,
                }),
              );
            }),
          ),
        ),
      ) as Observable<Action>,
  );

  createOutfitSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.createOutfitSuccess),
        tap(() => {
          Swal.fire({
            title: 'Success!',
            text: 'Your outfit has been added to the vault.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
          }).then(() => {
            this.router.navigate(['/outfits']);
          });
        }),
      ),
    { dispatch: false },
  );

  createOutfitFailure$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          OutfitsActions.createOutfitFailure,
          OutfitsActions.updateOutfitFailure,
          OutfitsActions.deleteOutfitFailure,
        ),
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

  updateOutfitSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.updateOutfitSuccess),
        tap(({ outfit }: { outfit: Outfit }) => {
          Swal.fire({
            title: 'Updated!',
            text: 'Your outfit has been updated.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
            timer: 2000,
            showConfirmButton: false,
          }).then(() => {
            this.router.navigate(['/outfits', outfit.id]);
          });
        }),
      ),
    { dispatch: false },
  );

  updateOutfit$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.updateOutfit),
        mergeMap((action: ReturnType<typeof OutfitsActions.updateOutfit>) =>
          this.outfitsUseCases.updateOutfit(action.id, action.outfit).pipe(
            map((outfit: Outfit) => OutfitsActions.updateOutfitSuccess({ outfit })),
            catchError((error) =>
              of(
                OutfitsActions.updateOutfitFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  deleteOutfit$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.deleteOutfit),
        mergeMap((action: ReturnType<typeof OutfitsActions.deleteOutfit>) =>
          this.outfitsUseCases.deleteOutfit(action.id).pipe(
            map(() => OutfitsActions.deleteOutfitSuccess({ id: action.id })),
            catchError((error) =>
              of(
                OutfitsActions.deleteOutfitFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  deleteOutfitSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.deleteOutfitSuccess),
        tap(() => {
          Swal.fire({
            title: 'Deleted!',
            text: 'Outfit removed correctly.',
            icon: 'success',
            background: '#ffffff',
            color: '#2d3436',
            confirmButtonColor: '#f8b4c4',
            timer: 2000,
            showConfirmButton: false,
          }).then(() => {
            this.router.navigate(['/outfits']);
          });
        }),
      ),
    { dispatch: false },
  );

  recordWear$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.recordOutfitWear),
        mergeMap((action: ReturnType<typeof OutfitsActions.recordOutfitWear>) =>
          this.outfitsUseCases.recordOutfitWear(action.id).pipe(
            map((outfit: Outfit) => OutfitsActions.recordOutfitWearSuccess({ outfit })),
            catchError((error) =>
              of(
                OutfitsActions.recordOutfitWearFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  recordOutfitWearSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.recordOutfitWearSuccess),
        tap(() => {
          Swal.fire({
            title: 'Nice Look!',
            text: 'Wear recorded successfully.',
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

  generateSuggestions$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.generateSuggestions),
        mergeMap((action: ReturnType<typeof OutfitsActions.generateSuggestions>) =>
          this.outfitsUseCases.getOutfitsSuggestions(action.request).pipe(
            map((outfits: Outfit[]) => OutfitsActions.generateSuggestionsSuccess({ outfits })),
            catchError((error) =>
              of(
                OutfitsActions.generateSuggestionsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  loadTodaysPick$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(OutfitsActions.loadTodaysPick),
        mergeMap((action: ReturnType<typeof OutfitsActions.loadTodaysPick>) =>
          this.outfitsUseCases.getTodaysPick(action.latitude, action.longitude, action.date).pipe(
            map((response: any) =>
              OutfitsActions.loadTodaysPickSuccess({
                outfit: response.outfit,
                context: {
                  weatherContext: response.weatherContext,
                  todayEvent: response.todayEvent,
                  matchScore: response.matchScore,
                  recommendationReason: response.recommendationReason,
                  isBestEffort: response.isBestEffort,
                },
              })
            ),
            catchError((error) =>
              of(
                OutfitsActions.loadTodaysPickFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );
}
