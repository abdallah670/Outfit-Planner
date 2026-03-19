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

  // Local pending filter state (batch filtering)
  pendingFilters = signal<SearchFilters>({ ...initialSearchFilters });
  hasPendingChanges = signal<boolean>(false);

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
  showCategoryFilter = computed(() => this.activeTab() === 'items');
  showSeasonFilter = computed(() => this.activeTab() === 'outfits'); // Only show for outfits - items don't have seasons
  showOccasionFilter = computed(() => this.activeTab() === 'outfits');
  showColorFilter = computed(() => this.activeTab() === 'items');
  showPriceFilter = computed(() => false); // DISABLED - backend doesn't support price filtering

  // Computed values - check results based on active tab
  hasResults = computed(() => {
    const tab = this.activeTab();
    const r = this.results();
    
    if (tab === 'outfits') {
      return r.outfits.length > 0;
    } else if (tab === 'items') {
      return r.wardrobeItems.length > 0;
    } else {
      // 'all' tab - check both
      return r.outfits.length > 0 || r.wardrobeItems.length > 0;
    }
  });

  // Tab-specific result counts
  tabResultsCount = computed(() => {
    const tab = this.activeTab();
    const r = this.results();
    
    if (tab === 'outfits') {
      return r.outfits.length;
    } else if (tab === 'items') {
      return r.wardrobeItems.length;
    } else {
      return r.totalCount;
    }
  });

  totalResults = computed(() => this.results().totalCount);

  // Active filters list for badges
  activeFilterBadges = computed(() => {
    const badges: { type: string; value: string }[] = [];
    const f = this.filters();

    f.categories.forEach(c => badges.push({ type: 'category', value: c }));
    f.seasons.forEach(s => badges.push({ type: 'season', value: s }));
    f.occasions?.forEach(o => badges.push({ type: 'occasion', value: o }));
    if (f.color) badges.push({ type: 'color', value: f.color });

    return badges;
  });

  // Filter options - MUST match backend enum values exactly
  // ClothingType enum: Top, Bottom, Dress, Outerwear, Footwear, Accessory, Undergarment, Swimwear, Activewear
  categoryOptions = ['Top', 'Bottom', 'Dress', 'Footwear', 'Accessory', 'Outerwear', 'Activewear'];
  
  // Season enum: Spring, Summer, Autumn, Winter, AllSeason
  seasonOptions = ['Spring', 'Summer', 'Autumn', 'Winter'];
  
  // OccasionType enum: Casual, BusinessCasual, Formal, Athletic, Social, Work, Date, Travel
  occasionOptions = ['Casual', 'BusinessCasual', 'Formal', 'Work', 'Date', 'Social', 'Athletic', 'Travel'];
  
  colorOptions = ['Black', 'White', 'Red', 'Blue', 'Green', 'Yellow', 'Purple', 'Pink', 'Gray', 'Brown'];

  ngOnInit(): void {
    // Load recent searches on init
    this.store.dispatch(SearchActions.loadRecentSearches());

    // Setup debounced search input (500ms for better performance)
    this.searchInput$
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((query: string) => {
        this.performSearch(query);
      });

    // Initialize pending filters with current filters
    this.pendingFilters.set({ ...this.filters() });

    // Load all results on initial page load
    this.performSearch('');
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Tab change handler - clears incompatible filters when switching tabs
  onTabChange(tab: SearchTab): void {
    this.activeTab.set(tab);

    // Clear filters that don't apply to the new tab
    const currentFilters = { ...this.filters() };
    
    if (tab === 'outfits') {
      // Switching to outfits - clear item-specific filters
      currentFilters.categories = [];
      currentFilters.color = null;
    } else if (tab === 'items') {
      // Switching to items - clear outfit-specific filters
      currentFilters.seasons = [];
      currentFilters.occasions = [];
    }
    // For 'all' tab, keep all filters

    // Update pending filters to match
    this.pendingFilters.set({ ...currentFilters });
    this.hasPendingChanges.set(false);

    // Update search type in filters
    const searchType: SearchType = tab === 'items' ? 'wardrobe' : tab === 'outfits' ? 'outfits' : 'all';
    this.store.dispatch(SearchActions.updateFilters({ filters: { ...currentFilters, type: searchType } }));

    // Trigger search with new filter
    const currentQuery = this.query();
    this.performSearch(currentQuery);
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
    this.pendingFilters.set({ ...initialSearchFilters });
    this.hasPendingChanges.set(false);

    const currentQuery = this.query();
    this.performSearch(currentQuery);
  }

  // ========== BATCH FILTER METHODS ==========

  // Category filter (local only until applied)
  onCategoryToggle(category: string): void {
    const current = this.pendingFilters().categories;
    const updated = current.includes(category)
      ? current.filter((c: string) => c !== category)
      : [...current, category];

    this.pendingFilters.update(f => ({ ...f, categories: updated }));
    this.hasPendingChanges.set(true);
  }

  isCategorySelected(category: string): boolean {
    return this.pendingFilters().categories.includes(category);
  }

  // Season filter (local only until applied)
  onSeasonToggle(season: string): void {
    const current = this.pendingFilters().seasons;
    const updated = current.includes(season)
      ? current.filter((s: string) => s !== season)
      : [...current, season];

    this.pendingFilters.update(f => ({ ...f, seasons: updated }));
    this.hasPendingChanges.set(true);
  }

  isSeasonSelected(season: string): boolean {
    return this.pendingFilters().seasons.includes(season);
  }

  // Occasion filter (local only until applied)
  onOccasionToggle(occasion: string): void {
    const current = this.pendingFilters().occasions || [];
    const updated = current.includes(occasion)
      ? current.filter((o: string) => o !== occasion)
      : [...current, occasion];

    this.pendingFilters.update(f => ({ ...f, occasions: updated }));
    this.hasPendingChanges.set(true);
  }

  isOccasionSelected(occasion: string): boolean {
    return (this.pendingFilters().occasions || []).includes(occasion);
  }

  // Color filter (local only until applied)
  onColorSelect(color: string | null): void {
    const currentColor = this.pendingFilters().color;
    const newColor = currentColor === color ? null : color;

    this.pendingFilters.update(f => ({ ...f, color: newColor }));
    this.hasPendingChanges.set(true);
  }

  isColorSelected(color: string | null): boolean {
    return this.pendingFilters().color === color;
  }

  // Price filter (local only until applied)
  pendingMinPrice(): number | null {
    return this.pendingFilters().minPrice ?? null;
  }

  pendingMaxPrice(): number | null {
    return this.pendingFilters().maxPrice ?? null;
  }

  onMinPriceChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    const numValue = value ? parseFloat(value) : null;
    this.pendingFilters.update(f => ({ ...f, minPrice: numValue }));
    this.hasPendingChanges.set(true);
  }

  onMaxPriceChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    const numValue = value ? parseFloat(value) : null;
    this.pendingFilters.update(f => ({ ...f, maxPrice: numValue }));
    this.hasPendingChanges.set(true);
  }

  // Apply all pending filters at once
  onApplyFilters(): void {
    const filtersToApply: SearchFilters = {
      ...this.pendingFilters(),
    };

    this.store.dispatch(SearchActions.updateFilters({ filters: filtersToApply }));
    this.hasPendingChanges.set(false);

    // Trigger search with all filters applied
    const currentQuery = this.query();
    this.performSearch(currentQuery);
  }

  // Remove single filter badge
  onRemoveFilter(type: string, value: string): void {
    const currentFilters = { ...this.filters() };

    switch (type) {
      case 'category':
        currentFilters.categories = currentFilters.categories.filter(c => c !== value);
        break;
      case 'season':
        currentFilters.seasons = currentFilters.seasons.filter(s => s !== value);
        break;
      case 'occasion':
        currentFilters.occasions = (currentFilters.occasions || []).filter(o => o !== value);
        break;
      case 'color':
        currentFilters.color = null;
        break;
    }

    this.store.dispatch(SearchActions.updateFilters({ filters: currentFilters }));

    // Update pending filters to match
    this.pendingFilters.set({ ...currentFilters });

    // Trigger search
    const currentQuery = this.query();
    this.performSearch(currentQuery);
  }

  getContrastColor(color: string): string {
    const lightColors = ['White', 'Yellow', 'Pink'];
    return lightColors.includes(color) ? '#000000' : '#FFFFFF';
  }
}
