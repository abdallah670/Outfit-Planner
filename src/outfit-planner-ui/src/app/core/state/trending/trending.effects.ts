import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { TrendingActions } from './trending.actions';
import { catchError, map, mergeMap, of, Observable } from 'rxjs';
import { TrendingUseCases } from '../../../domain/usecases/trending.usecases';
import { TrendingOutfit } from '../../../domain/entities/outfit.entity';
import { CursorPagedResult } from '../../../domain/entities/response.entity';


@Injectable()
export class TrendingEffects {
  private actions$ = inject(Actions);
  private trendingUseCases = inject(TrendingUseCases);

  loadTrending$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(TrendingActions.loadTrending),
        mergeMap((action: ReturnType<typeof TrendingActions.loadTrending>) =>
          this.trendingUseCases.getTrendingOutfits(action.cursor, action.pageSize).pipe(
            map((response: CursorPagedResult<TrendingOutfit>) =>
              TrendingActions.loadTrendingSuccess({ 
                outfits: response.items, 
                nextCursor: response.nextCursor ?? undefined,
                hasMore: response.hasMore
              })
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
