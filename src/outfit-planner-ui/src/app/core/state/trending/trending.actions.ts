import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { TrendingOutfit } from '../../../domain/entities/outfit.entity';

export const TrendingActions = createActionGroup({
  source: 'trending',
  events: {
    'Load Trending': props<{ page?: number; pageSize?: number }>(),
    'Load Trending Success': props<{ outfits: TrendingOutfit[]; totalCount: number }>(),
    'Load Trending Failure': props<{ error: string }>(),
  },
});
