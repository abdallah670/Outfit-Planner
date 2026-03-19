import { createFeatureSelector, createSelector } from '@ngrx/store';
import { SearchState } from '../../../domain/entities/search.entity';

export const selectSearchState = createFeatureSelector<SearchState>('search');

export const selectSearchQuery = createSelector(
  selectSearchState,
  (state) => state.query
);

export const selectSearchFilters = createSelector(
  selectSearchState,
  (state) => state.filters
);

export const selectSearchResults = createSelector(
  selectSearchState,
  (state) => state.results
);

export const selectSearchOutfits = createSelector(
  selectSearchResults,
  (results) => results.outfits
);

export const selectSearchWardrobeItems = createSelector(
  selectSearchResults,
  (results) => results.wardrobeItems
);

export const selectSearchTotalCount = createSelector(
  selectSearchResults,
  (results) => results.totalCount
);

export const selectSearchFacets = createSelector(
  selectSearchResults,
  (results) => results.facets
);

export const selectSearchLoading = createSelector(
  selectSearchState,
  (state) => state.loading
);

export const selectSearchError = createSelector(
  selectSearchState,
  (state) => state.error
);

export const selectSearchSuggestions = createSelector(
  selectSearchState,
  (state) => state.suggestions
);

export const selectRecentSearches = createSelector(
  selectSearchState,
  (state) => state.recentSearches
);

export const selectHasActiveFilters = createSelector(
  selectSearchFilters,
  (filters) =>
    filters.categories.length > 0 ||
    filters.seasons.length > 0 ||
    filters.occasions.length > 0 ||
    filters.color !== null ||
    filters.minPrice !== null ||
    filters.maxPrice !== null ||
    filters.type !== 'all'
);

export const selectActiveFiltersCount = createSelector(
  selectSearchFilters,
  (filters) =>
    filters.categories.length +
    filters.seasons.length +
    filters.occasions.length +
    (filters.color ? 1 : 0) +
    (filters.minPrice !== null ? 1 : 0) +
    (filters.maxPrice !== null ? 1 : 0) +
    (filters.type !== 'all' ? 1 : 0)
);
