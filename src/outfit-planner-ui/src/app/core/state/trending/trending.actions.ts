import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { TrendingOutfit } from '../../../domain/entities/outfit.entity';

export const TrendingActions = createActionGroup({
  source: 'trending',
  events: {
    'Load Trending': props<{ page?: number; pageSize?: number }>(),
    'Load Trending Success': props<{ outfits: TrendingOutfit[]; totalCount: number; append: boolean }>(),
    'Load Trending Failure': props<{ error: string }>(),

    'React To Vote': props<{ voteId: string; reactionType: string }>(),
    'React To Vote Success': props<{ voteId: string }>(),
    'React To Vote Failure': props<{ error: string }>(),
  },
});
