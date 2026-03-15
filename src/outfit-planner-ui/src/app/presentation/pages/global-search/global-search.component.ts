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

type SearchTab = 'all' | 'outfits' | 'items';

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

  // Active tab signal
  activeTab = signal<SearchTab>('all');

  // Price range signals
  minPrice = signal<number | null>(null);
  maxPrice = signal<number | null>(null);

  // Occasion selection signal
  selectedOccasions = signal<string[]>([]);

  // Signals from NgRx store
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
  localQuery = signal<string>('');

  // Search placeholder based on active tab
  searchPlaceholder = computed(() => {
    switch (this.activeTab()) {
      case 'all':
        return 'Search everything...';
      case 'outfits':
        return 'Search outfits by name or occasion...';
      case 'items':
        return 'Search wardrobe by item name or brand...';
      default:
        return 'Search...';
    }
  });

  // Show outfits section
  showOutfits = computed(() => {
    const tab = this.activeTab();
    return tab === 'all' || tab === 'outfits';
  });

  // Show items section
  showItems = computed(() => {
    const tab = this.activeTab();
    return tab === 'all' || tab === 'items';
  });

  // Filter visibility based on tab
  // 'all' tab shows Season only (common filter)
  // 'outfits' tab shows Season, Occasion
  // 'items' tab shows Category, Season, Color, Price
  showCategoryFilter = computed(() => this.activeTab() === 'items');
  showSeasonFilter = computed(() => true); // Show on all tabs
  showOccasionFilter = computed(() => this.activeTab() === 'outfits');
  showColorFilter = computed(() => this.activeTab() === 'items');
  showPriceFilter = computed(() => this.activeTab() === 'items');

  // Computed values
  hasResults = computed(() => {
    const r = this.results();
    return r.outfits.length > 0 || r.wardrobeItems.length > 0;
  });

  totalResults = computed(() => this.results().totalCount);

  // Filter options
  categoryOptions = ['Tops', 'Bottoms', 'Dresses', 'Shoes', 'Accessories', 'Outerwear'];
  seasonOptions = ['Spring', 'Summer', 'Fall', 'Winter'];
  occasionOptions = ['Casual', 'Formal', 'Work', 'Party', 'Sport', 'Date'];
  colorOptions = ['Black', 'White', 'Red', 'Blue', 'Green', 'Yellow', 'Purple', 'Pink', 'Gray', 'Brown'];

  ngOnInit(): void {
    // Load recent searches on init
    this.store.dispatch(SearchActions.loadRecentSearches());

    // Setup debounced search input
    this.searchInput$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((query: string) => {
        this.performSearch(query);
      });

    // Load all results on initial page load
    this.performSearch('');
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Tab change handler
  onTabChange(tab: SearchTab): void {
    this.activeTab.set(tab);

    // Update search type in filters
    const searchType: SearchType = tab === 'items' ? 'wardrobe' : tab === 'outfits' ? 'outfits' : 'all';
    this.store.dispatch(SearchActions.updateFilters({ filters: { type: searchType } }));

    // Trigger search with new filter
    const currentQuery = this.query();
    if (currentQuery) {
      this.performSearch(currentQuery);
    }
  }

  onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.localQuery.set(value);
    this.searchInput$.next(value);
  }

  onSearchKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.performSearch(this.localQuery());
    }
  }

  performSearch(query: string): void {
    this.store.dispatch(SearchActions.search({ query: query.trim() }));
    if (query.trim()) {
      this.store.dispatch(SearchActions.saveRecentSearch({ query: query.trim() }));
    }
  }

  onRecentSearchClick(search: string): void {
    this.localQuery.set(search);
    this.performSearch(search);
  }

  onRemoveRecentSearch(event: Event, search: string): void {
    event.stopPropagation();
    this.store.dispatch(SearchActions.removeRecentSearch({ query: search }));
  }

  onClearFilters(): void {
    this.store.dispatch(SearchActions.clearFilters());
    this.selectedOccasions.set([]);
    this.minPrice.set(null);
    this.maxPrice.set(null);

    const currentQuery = this.query();
    if (currentQuery) {
      this.performSearch(currentQuery);
    }
  }

  // Category filter
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

  isCategorySelected(category: string): boolean {
    return this.filters().categories.includes(category);
  }

  // Season filter
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

  isSeasonSelected(season: string): boolean {
    return this.filters().seasons.includes(season);
  }

  // Occasion filter
  onOccasionToggle(occasion: string): void {
    const currentOccasions = this.selectedOccasions();
    const updatedOccasions = currentOccasions.includes(occasion)
      ? currentOccasions.filter((o: string) => o !== occasion)
      : [...currentOccasions, occasion];

    this.selectedOccasions.set(updatedOccasions);

    const currentQuery = this.query();
    if (currentQuery) {
      this.performSearch(currentQuery);
    }
  }

  isOccasionSelected(occasion: string): boolean {
    return this.selectedOccasions().includes(occasion);
  }

  // Color filter
  onColorSelect(color: string | null): void {
    this.store.dispatch(SearchActions.updateFilters({ filters: { color } }));

    const currentQuery = this.query();
    if (currentQuery) {
      this.performSearch(currentQuery);
    }
  }

  isColorSelected(color: string | null): boolean {
    return this.filters().color === color;
  }

  // Price filter handlers
  onMinPriceChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.minPrice.set(value ? parseFloat(value) : null);
  }

  onMaxPriceChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.maxPrice.set(value ? parseFloat(value) : null);
  }

  getContrastColor(color: string): string {
    const lightColors = ['White', 'Yellow', 'Pink'];
    return lightColors.includes(color) ? '#000000' : '#FFFFFF';
  }
}
