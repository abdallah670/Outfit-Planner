import { createFeature, createReducer, on } from '@ngrx/store';
import { TrendingActions } from './trending.actions';
import { TrendingOutfit } from '../../../domain/entities/outfit.entity';

export interface TrendingState {
  outfits: TrendingOutfit[];
  totalCount: number;
  loading: boolean;
  error: string | null;
}

export const initialState: TrendingState = {
  outfits: [],
  totalCount: 0,
  loading: false,
  error: null,
};

export const trendingFeature = createFeature({
  name: 'trending',
  reducer: createReducer(
    initialState,
    on(TrendingActions.loadTrending, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(TrendingActions.loadTrendingSuccess, (state, { outfits, totalCount, append }) => ({
      ...state,
      outfits: append ? [...state.outfits, ...outfits] : outfits,
      totalCount,
      loading: false,
    })),
    on(TrendingActions.loadTrendingFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    }))
  ),
});

export const {
  name,
  reducer,
  selectTrendingState,
  selectOutfits,
  selectTotalCount,
  selectLoading,
  selectError,
} = trendingFeature;
