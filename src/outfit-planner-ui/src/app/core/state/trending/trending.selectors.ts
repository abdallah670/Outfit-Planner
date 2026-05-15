import { trendingFeature } from './trending.reducer';

export const {
  selectTrendingState,
  selectOutfits: selectTrendingOutfits,
  selectNextCursor: selectTrendingNextCursor,
  selectHasMore: selectTrendingHasMore,
  selectLoading: selectTrendingLoading,
  selectError: selectTrendingError,
} = trendingFeature;

