import { createReducer, on } from '@ngrx/store';
import { SearchActions } from './search.actions';
import { initialSearchState } from '../../../domain/entities/search.entity';

export const searchReducer = createReducer(
  initialSearchState,

  // Search
  on(SearchActions.search, (state, { query, filters }) => ({
    ...state,
    query,
    filters: filters ? { ...state.filters, ...filters } : state.filters,
    loading: true,
    error: null,
  })),

  on(SearchActions.searchSuccess, (state, { results }) => ({
    ...state,
    results,
    loading: false,
    error: null,
  })),

  on(SearchActions.searchFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Suggestions
  on(SearchActions.loadSuggestions, (state) => ({
    ...state,
    suggestions: [],
  })),

  on(SearchActions.loadSuggestionsSuccess, (state, { suggestions }) => ({
    ...state,
    suggestions,
  })),

  on(SearchActions.loadSuggestionsFailure, (state) => ({
    ...state,
    suggestions: [],
  })),

  on(SearchActions.clearSuggestions, (state) => ({
    ...state,
    suggestions: [],
  })),

  // Filters
  on(SearchActions.updateFilters, (state, { filters }) => ({
    ...state,
    filters: { ...state.filters, ...filters },
  })),

  on(SearchActions.clearFilters, (state) => ({
    ...state,
    filters: initialSearchState.filters,
  })),

  // Recent Searches
  on(SearchActions.loadRecentSearchesSuccess, (state, { recentSearches }) => ({
    ...state,
    recentSearches,
  })),

  on(SearchActions.saveRecentSearch, (state, { query }) => {
    const filtered = state.recentSearches.filter((s) => s !== query);
    return {
      ...state,
      recentSearches: [query, ...filtered].slice(0, 10),
    };
  }),

  on(SearchActions.removeRecentSearch, (state, { query }) => ({
    ...state,
    recentSearches: state.recentSearches.filter((s) => s !== query),
  })),

  on(SearchActions.clearRecentSearches, (state) => ({
    ...state,
    recentSearches: [],
  })),

  // UI State
  on(SearchActions.setLoading, (state, { loading }) => ({
    ...state,
    loading,
  })),

  on(SearchActions.clearSearch, () => initialSearchState)
);
