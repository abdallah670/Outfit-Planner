import { createFeature, createReducer, on } from '@ngrx/store';
import { TrendingActions } from './trending.actions';
import { TrendingOutfit } from '../../../domain/entities/outfit.entity';

export interface TrendingState {
  outfits: TrendingOutfit[];
  nextCursor: string | null;
  hasMore: boolean;
  loading: boolean;
  error: string | null;
}

export const initialState: TrendingState = {
  outfits: [],
  nextCursor: null,
  hasMore: false,
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
    on(TrendingActions.loadTrendingSuccess, (state, { outfits, nextCursor, hasMore }) => ({
      ...state,
      outfits: state.nextCursor ? [...state.outfits, ...outfits] : outfits,
      nextCursor: nextCursor || null,
      hasMore,
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
  selectNextCursor,
  selectHasMore,
  selectLoading,
  selectError,
} = trendingFeature;

