import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { SearchResults, SearchFilters, SearchType } from '../../../domain/entities/search.entity';

export const SearchActions = createActionGroup({
  source: 'Search',
  events: {
    // Search
    'Search': props<{ query: string; filters?: SearchFilters; page?: number }>(),
    'Search Success': props<{ results: SearchResults }>(),
    'Search Failure': props<{ error: string }>(),
    
    // Suggestions
    'Load Suggestions': props<{ partialQuery: string }>(),
    'Load Suggestions Success': props<{ suggestions: string[] }>(),
    'Load Suggestions Failure': props<{ error: string }>(),
    'Clear Suggestions': emptyProps(),
    
    // Filters
    'Update Filters': props<{ filters: Partial<SearchFilters> }>(),
    'Clear Filters': emptyProps(),
    
    // Recent Searches
    'Load Recent Searches': emptyProps(),
    'Load Recent Searches Success': props<{ recentSearches: string[] }>(),
    'Save Recent Search': props<{ query: string }>(),
    'Remove Recent Search': props<{ query: string }>(),
    'Clear Recent Searches': emptyProps(),
    
    // UI State
    'Set Loading': props<{ loading: boolean }>(),
    'Clear Search': emptyProps(),
  },
});
