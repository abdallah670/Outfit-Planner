import { trendingFeature } from './trending.reducer';

export const {
  selectTrendingState,
  selectOutfits: selectTrendingOutfits,
  selectTotalCount: selectTrendingTotalCount,
  selectLoading: selectTrendingLoading,
  selectError: selectTrendingError,
} = trendingFeature;
