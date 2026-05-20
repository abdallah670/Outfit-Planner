import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { TrendingOutfit } from '../../../domain/entities/outfit.entity';

export const TrendingActions = createActionGroup({
  source: 'trending',
  events: {
    'Load Trending': props<{ cursor?: string; pageSize?: number }>(),
    'Load Trending Success': props<{ outfits: TrendingOutfit[]; nextCursor?: string; hasMore: boolean }>(),
    'Load Trending Failure': props<{ error: string }>(),
  },
});

