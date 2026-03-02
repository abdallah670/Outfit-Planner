import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';

import { catchError, map, mergeMap, of, Observable, tap } from 'rxjs';
import { WardrobeActions } from './wardrobe.actions';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { WardrobeService } from '../../services/wardrobe.service';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';

@Injectable()
export class WardrobeEffects {
  private actions$ = inject(Actions);
  private wardrobeService = inject(WardrobeService);
  private router = inject(Router);

  loadClothingItems$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WardrobeActions.loadClothingItems),
        mergeMap(() =>
          this.wardrobeService.getClothingItems().pipe(
            map((items: ClothingItem[]) => WardrobeActions.loadClothingItemsSuccess({ items })),
            catchError((error) =>
              of(
                WardrobeActions.loadClothingItemsFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  loadClothingItemById$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WardrobeActions.loadClothingItemById),
        mergeMap((action: ReturnType<typeof WardrobeActions.loadClothingItemById>) =>
          this.wardrobeService.getClothingItemById(action.id).pipe(
            map((item: ClothingItem) => WardrobeActions.loadClothingItemByIdSuccess({ item })),
            catchError((error) =>
              of(
                WardrobeActions.loadClothingItemByIdFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  loadClothingItemsByCategory$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WardrobeActions.loadClothingItemsByCategory),
        mergeMap((action: ReturnType<typeof WardrobeActions.loadClothingItemsByCategory>) =>
          this.wardrobeService.getClothingItemsByCategory(action.category).pipe(
            map((items: ClothingItem[]) =>
              WardrobeActions.loadClothingItemsByCategorySuccess({ items }),
            ),
            catchError((error) =>
              of(
                WardrobeActions.loadClothingItemsByCategoryFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  createClothingItem$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WardrobeActions.createClothingItem),
        mergeMap((action: ReturnType<typeof WardrobeActions.createClothingItem>) =>
          this.wardrobeService.createClothingItem(action.item, action.image).pipe(
            map((item: ClothingItem) => WardrobeActions.createClothingItemSuccess({ item })),
            catchError((err) => {
              let errorMessage = err.message || 'Failed to add clothing item';
              if (err.error?.errors) {
                errorMessage = Object.values(err.error.errors).flat().join('\n');
              } else if (err.error?.title) {
                errorMessage = err.error.title;
              }
              return of(
                WardrobeActions.createClothingItemFailure({
                  error: errorMessage,
                }),
              );
            }),
          ),
        ),
      ) as Observable<Action>,
  );

  createClothingItemSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WardrobeActions.createClothingItemSuccess),
        tap(() => {
          Swal.fire({
            title: 'Success!',
            text: 'Your clothing item has been added to the vault.',
            icon: 'success',
            background: '#1f2937',
            color: '#f9fafb',
            confirmButtonColor: '#3b82f6',
          }).then(() => {
            this.router.navigate(['/']);
          });
        }),
      ),
    { dispatch: false },
  );

  createClothingItemFailure$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          WardrobeActions.createClothingItemFailure,
          WardrobeActions.updateClothingItemFailure,
          WardrobeActions.deleteClothingItemFailure,
        ),
        tap(({ error }: { error: string }) => {
          Swal.fire({
            title: 'Error!',
            text: error,
            icon: 'error',
            background: '#1f2937',
            color: '#f9fafb',
            confirmButtonColor: '#ef4444',
          });
        }),
      ),
    { dispatch: false },
  );

  updateClothingItemSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WardrobeActions.updateClothingItemSuccess),
        tap(() => {
          Swal.fire({
            title: 'Updated!',
            text: 'Your clothing item has been updated.',
            icon: 'success',
            background: '#1f2937',
            color: '#f9fafb',
            confirmButtonColor: '#3b82f6',
            timer: 2000,
            showConfirmButton: false,
          }).then(() => {
            this.router.navigate(['/']);
          });
        }),
      ),
    { dispatch: false },
  );

  updateClothingItem$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WardrobeActions.updateClothingItem),
        mergeMap((action: ReturnType<typeof WardrobeActions.updateClothingItem>) =>
          this.wardrobeService.updateClothingItem(action.id, action.item, action.image).pipe(
            map((item: ClothingItem) => WardrobeActions.updateClothingItemSuccess({ item })),
            catchError((error) =>
              of(
                WardrobeActions.updateClothingItemFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  deleteClothingItem$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WardrobeActions.deleteClothingItem),
        mergeMap((action: ReturnType<typeof WardrobeActions.deleteClothingItem>) =>
          this.wardrobeService.deleteClothingItem(action.id).pipe(
            map(() => WardrobeActions.deleteClothingItemSuccess({ id: action.id })),
            catchError((error) =>
              of(
                WardrobeActions.deleteClothingItemFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  deleteClothingItemSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WardrobeActions.deleteClothingItemSuccess),
        tap(() => {
          Swal.fire({
            title: 'Deleted!',
            text: 'Clothing item removed correctly.',
            icon: 'success',
            background: '#1f2937',
            color: '#f9fafb',
            confirmButtonColor: '#3b82f6',
            timer: 2000,
            showConfirmButton: false,
          }).then(() => {
            this.router.navigate(['/']);
          });
        }),
      ),
    { dispatch: false },
  );

  recordWear$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WardrobeActions.recordWear),
        mergeMap((action: ReturnType<typeof WardrobeActions.recordWear>) =>
          this.wardrobeService.recordWear(action.id).pipe(
            map((item: ClothingItem) => WardrobeActions.recordWearSuccess({ item })),
            catchError((error) =>
              of(
                WardrobeActions.recordWearFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ) as Observable<Action>,
  );
}
