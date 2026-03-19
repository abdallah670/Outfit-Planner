import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { SearchActions } from './search.actions';
import { selectSearchFilters } from './search.selectors';
import { SearchService } from '../../services/search.service';
import { LocalStorageService } from '../../services/local-storage.service';
import { SearchFilters, SearchResults, SearchType } from '../../../domain/entities/search.entity';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  of,
  switchMap,
  tap,
  withLatestFrom,
} from 'rxjs';

// Define action payload types
interface SearchAction {
  query: string;
  filters?: SearchFilters;
  page?: number;
}

interface LoadSuggestionsAction {
  partialQuery: string;
}

interface RemoveRecentSearchAction {
  query: string;
}

// Functional effect for search - uses withLatestFrom to get filters from store
export const search$ = createEffect(
  (actions$ = inject(Actions), store = inject(Store), searchService = inject(SearchService)) => {
    return actions$.pipe(
      ofType(SearchActions.search),
      debounceTime(300),
      withLatestFrom(store.select(selectSearchFilters)),
      distinctUntilChanged(
        (prev: [SearchAction, SearchFilters], curr: [SearchAction, SearchFilters]) =>
          prev[0].query === curr[0].query &&
          JSON.stringify(prev[1]) === JSON.stringify(curr[1])
      ),
      switchMap(([action, filters]: [SearchAction, SearchFilters]) =>
        searchService.search(action.query, filters, action.page).pipe(
          map((results: SearchResults) => SearchActions.searchSuccess({ results })),
          catchError((error) =>
            of(SearchActions.searchFailure({ error: error.message }))
          )
        )
      )
    );
  },
  { functional: true }
);

// Functional effect for saving recent search
export const saveRecentSearch$ = createEffect(
  (actions$ = inject(Actions), store = inject(Store), localStorage = inject(LocalStorageService)) => {
    return actions$.pipe(
      ofType(SearchActions.searchSuccess),
      withLatestFrom(store.select((state: any) => state.search?.query ?? '')),
      tap(([_action, query]: [any, string]) => {
        if (query && query.trim()) {
          const RECENT_SEARCHES_KEY = 'recent_searches';
          const recent = localStorage.getItem<string[]>(RECENT_SEARCHES_KEY) || [];
          const filtered = recent.filter((s) => s !== query);
          const updated = [query, ...filtered].slice(0, 10);
          localStorage.setItem(RECENT_SEARCHES_KEY, updated);
        }
      })
    );
  },
  { functional: true, dispatch: false }
);

// Functional effect for load suggestions
export const loadSuggestions$ = createEffect(
  (actions$ = inject(Actions), searchService = inject(SearchService)) => {
    return actions$.pipe(
      ofType(SearchActions.loadSuggestions),
      debounceTime(300),
      distinctUntilChanged((prev: LoadSuggestionsAction, curr: LoadSuggestionsAction) => prev.partialQuery === curr.partialQuery),
      switchMap((action: LoadSuggestionsAction) =>
        searchService.getSuggestions(action.partialQuery).pipe(
          map((suggestions: string[]) => SearchActions.loadSuggestionsSuccess({ suggestions })),
          catchError((error) =>
            of(SearchActions.loadSuggestionsFailure({ error: error.message }))
          )
        )
      )
    );
  },
  { functional: true }
);

// Functional effect for load recent searches
export const loadRecentSearches$ = createEffect(
  (actions$ = inject(Actions), localStorage = inject(LocalStorageService)) => {
    return actions$.pipe(
      ofType(SearchActions.loadRecentSearches),
      map(() => {
        const RECENT_SEARCHES_KEY = 'recent_searches';
        const recent = localStorage.getItem<string[]>(RECENT_SEARCHES_KEY) || [];
        return SearchActions.loadRecentSearchesSuccess({ recentSearches: recent });
      })
    );
  },
  { functional: true }
);

// Functional effect for clear recent searches
export const clearRecentSearches$ = createEffect(
  (actions$ = inject(Actions), localStorage = inject(LocalStorageService)) => {
    return actions$.pipe(
      ofType(SearchActions.clearRecentSearches),
      tap(() => {
        const RECENT_SEARCHES_KEY = 'recent_searches';
        localStorage.removeItem(RECENT_SEARCHES_KEY);
      })
    );
  },
  { functional: true, dispatch: false }
);

// Functional effect for remove recent search
export const removeRecentSearch$ = createEffect(
  (actions$ = inject(Actions), store = inject(Store), localStorage = inject(LocalStorageService)) => {
    return actions$.pipe(
      ofType(SearchActions.removeRecentSearch),
      withLatestFrom(store.select((state: any) => state.search?.recentSearches ?? [])),
      tap(([action, recent]: [RemoveRecentSearchAction, string[]]) => {
        const RECENT_SEARCHES_KEY = 'recent_searches';
        const updated = recent.filter((s: string) => s !== action.query);
        localStorage.setItem(RECENT_SEARCHES_KEY, updated);
      })
    );
  },
  { functional: true, dispatch: false }
);
