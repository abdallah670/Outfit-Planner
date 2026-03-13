import { Component, OnInit, OnDestroy, inject, signal, computed, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { SearchActions } from '../../../core/state/search/search.actions';
import {
  selectSearchQuery,
  selectSearchFilters,
  selectSearchResults,
  selectSearchLoading,
  selectSearchError,
  selectRecentSearches,
  selectSearchSuggestions,
  selectHasActiveFilters,
  selectActiveFiltersCount,
  selectSearchOutfits,
  selectSearchWardrobeItems,
} from '../../../core/state/search/search.selectors';
import {
  SearchFilters,
  SearchResults,
  SearchType,
  OutfitSearchResult,
  WardrobeItemSearchResult,
  initialSearchResults,
  initialSearchFilters,
} from '../../../domain/entities/search.entity';

@Component({
  selector: 'app-global-search',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './global-search.component.html',
  styleUrls: ['./global-search.component.scss'],
})
export class GlobalSearchComponent implements OnInit, OnDestroy {
  private store = inject(Store);
  private destroy$ = new Subject<void>();
  private searchInput$ = new Subject<string>();

  // Signals from NgRx store with explicit types
  query: Signal<string> = toSignal(this.store.select(selectSearchQuery), { initialValue: '' });
  filters: Signal<SearchFilters> = toSignal(this.store.select(selectSearchFilters), {
    initialValue: initialSearchFilters,
  });
  results: Signal<SearchResults> = toSignal(this.store.select(selectSearchResults), {
    initialValue: initialSearchResults,
  });
  loading: Signal<boolean> = toSignal(this.store.select(selectSearchLoading), { initialValue: false });
  error: Signal<string | null> = toSignal(this.store.select(selectSearchError), { initialValue: null });
  recentSearches: Signal<string[]> = toSignal(this.store.select(selectRecentSearches), { initialValue: [] });
  suggestions: Signal<string[]> = toSignal(this.store.select(selectSearchSuggestions), { initialValue: [] });
  hasActiveFilters: Signal<boolean> = toSignal(this.store.select(selectHasActiveFilters), { initialValue: false });
  activeFiltersCount: Signal<number> = toSignal(this.store.select(selectActiveFiltersCount), { initialValue: 0 });
  outfits: Signal<OutfitSearchResult[]> = toSignal(this.store.select(selectSearchOutfits), { initialValue: [] });
  wardrobeItems: Signal<WardrobeItemSearchResult[]> = toSignal(this.store.select(selectSearchWardrobeItems), { initialValue: [] });

  // Local state
  showFilters = signal(false);
  showSuggestions = signal(false);
  localQuery = signal('');

  // Computed values
  hasResults = computed(() => {
    const r = this.results();
    return r.outfits.length > 0 || r.wardrobeItems.length > 0;
  });

  totalResults = computed(() => this.results().totalCount);

  // Filter options
  typeOptions: { value: SearchType; label: string; icon: string }[] = [
    { value: 'all', label: 'All', icon: 'search' },
    { value: 'outfits', label: 'Outfits', icon: 'checkroom' },
    { value: 'wardrobe', label: 'Wardrobe', icon: 'inventory_2' },
  ];

  categoryOptions = ['Tops', 'Bottoms', 'Dresses', 'Shoes', 'Accessories', 'Outerwear'];
  seasonOptions = ['Spring', 'Summer', 'Fall', 'Winter'];
  colorOptions = [
    'Black',
    'White',
    'Red',
    'Blue',
    'Green',
    'Yellow',
    'Purple',
    'Pink',
    'Gray',
    'Brown',
  ];

  ngOnInit(): void {
    // Load recent searches on init
    this.store.dispatch(SearchActions.loadRecentSearches());

    // Setup debounced search input
    this.searchInput$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((query: string) => {
        this.performSearch(query);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.localQuery.set(value);
    this.searchInput$.next(value);
    this.showSuggestions.set(value.length >= 2);
  }

  onSearchKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.showSuggestions.set(false);
      this.performSearch(this.localQuery());
    }
  }

  onSearchFocus(): void {
    if (this.localQuery().length >= 2) {
      this.showSuggestions.set(true);
    }
  }

  onSearchBlur(): void {
    // Delay to allow clicking on suggestions
    setTimeout(() => this.showSuggestions.set(false), 200);
  }

  performSearch(query: string): void {
    if (query.trim()) {
      this.store.dispatch(SearchActions.search({ query: query.trim() }));
      this.store.dispatch(SearchActions.saveRecentSearch({ query: query.trim() }));
    }
  }

  onRecentSearchClick(search: string): void {
    this.localQuery.set(search);
    this.performSearch(search);
    this.showSuggestions.set(false);
  }

  onRemoveRecentSearch(event: Event, search: string): void {
    event.stopPropagation();
    this.store.dispatch(SearchActions.removeRecentSearch({ query: search }));
  }

  onClearRecentSearches(): void {
    this.store.dispatch(SearchActions.clearRecentSearches());
  }

  onSuggestionClick(suggestion: string): void {
    this.localQuery.set(suggestion);
    this.performSearch(suggestion);
    this.showSuggestions.set(false);
  }

  toggleFilters(): void {
    this.showFilters.update((v) => !v);
  }

  onTypeChange(type: SearchType): void {
    this.store.dispatch(SearchActions.updateFilters({ filters: { type } }));
    const currentQuery = this.query();
    if (currentQuery) {
      this.performSearch(currentQuery);
    }
  }

  onCategoryToggle(category: string): void {
    const currentCategories = this.filters().categories;
    const updatedCategories = currentCategories.includes(category)
      ? currentCategories.filter((c: string) => c !== category)
      : [...currentCategories, category];

    this.store.dispatch(
      SearchActions.updateFilters({ filters: { categories: updatedCategories } }),
    );

    const currentQuery = this.query();
    if (currentQuery) {
      this.performSearch(currentQuery);
    }
  }

  onSeasonToggle(season: string): void {
    const currentSeasons = this.filters().seasons;
    const updatedSeasons = currentSeasons.includes(season)
      ? currentSeasons.filter((s: string) => s !== season)
      : [...currentSeasons, season];

    this.store.dispatch(SearchActions.updateFilters({ filters: { seasons: updatedSeasons } }));

    const currentQuery = this.query();
    if (currentQuery) {
      this.performSearch(currentQuery);
    }
  }

  onColorSelect(color: string | null): void {
    this.store.dispatch(SearchActions.updateFilters({ filters: { color } }));

    const currentQuery = this.query();
    if (currentQuery) {
      this.performSearch(currentQuery);
    }
  }

  onClearFilters(): void {
    this.store.dispatch(SearchActions.clearFilters());

    const currentQuery = this.query();
    if (currentQuery) {
      this.performSearch(currentQuery);
    }
  }

  onClearSearch(): void {
    this.localQuery.set('');
    this.store.dispatch(SearchActions.clearSearch());
  }

  isCategorySelected(category: string): boolean {
    return this.filters().categories.includes(category);
  }

  isSeasonSelected(season: string): boolean {
    return this.filters().seasons.includes(season);
  }

  isColorSelected(color: string | null): boolean {
    return this.filters().color === color;
  }

  getContrastColor(color: string): string {
    // Simple contrast calculation for light/dark text
    const lightColors = ['White', 'Yellow', 'Pink'];
    return lightColors.includes(color) ? '#000000' : '#FFFFFF';
  }
}
