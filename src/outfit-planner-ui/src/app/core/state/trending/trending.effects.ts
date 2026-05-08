import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { TrendingActions } from './trending.actions';
import { catchError, map, mergeMap, of, Observable } from 'rxjs';
import { TrendingUseCases } from '../../../domain/usecases/trending.usecases';
import { TrendingOutfit } from '../../../domain/entities/outfit.entity';

@Injectable()
export class TrendingEffects {
  private actions$ = inject(Actions);
  private trendingUseCases = inject(TrendingUseCases);

  loadTrending$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(TrendingActions.loadTrending),
        mergeMap((action: ReturnType<typeof TrendingActions.loadTrending>) =>
          this.trendingUseCases.getTrendingOutfits(action.page, action.pageSize).pipe(
            map((response: { items: TrendingOutfit[]; totalCount: number }) =>
              TrendingActions.loadTrendingSuccess({ outfits: response.items, totalCount: response.totalCount })
            ),
            catchError((error) =>
              of(
                TrendingActions.loadTrendingFailure({
                  error: error.message,
                }),
              ),
            ),
          ),
        ),
      ),
  );
}
