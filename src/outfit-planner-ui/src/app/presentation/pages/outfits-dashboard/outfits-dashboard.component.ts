import { Component, OnInit, inject, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';

import { OutfitsActions } from '../../../core/state/outfit/outfit.actions';
import { selectAllOutfits, selectOutfitLoading } from '../../../core/state/outfit/outfit.selectors';
import { OutfitState } from '../../../core/state/outfit/outfit.reducer';
import { OutfitCardComponent } from '../../components/outfits/outfit-card/outfit-card.component';
import { Outfit } from '../../../domain/entities/outfit.entity';
import { OccasionType, Season } from '../../../domain/entities/outfit.entity';
import { OutfitsUseCases } from '../../../domain/usecases/outfit.usecases';
import { PagedResult } from '../../../domain/entities/response.entity';

@Component({
  selector: 'app-outfits-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    FormsModule,
    OutfitCardComponent,
  ],
  templateUrl: './outfits-dashboard.component.html',
  styleUrl: './outfits-dashboard.component.scss',
})
export class OutfitsDashboardComponent implements OnInit {
  private store = inject(Store<{ outfit: OutfitState }>);
  private outfitsUseCases = inject(OutfitsUseCases);

  // Pagination signals
  page = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);

  // Backend loaded outfits
  outfits = signal<Outfit[]>([]);
  loading = signal(false);

  // Filter state
  selectedOccasion = signal<string>('');
  selectedSeason = signal<string>('');
  searchQuery = signal<string>('');
  sortBy = signal<string>('recent');

  // Pagination computed
  hasPreviousPage = computed(() => this.page() > 1);
  hasNextPage = computed(() => this.page() * this.pageSize() < this.totalCount());
  totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()) || 1);

  // Filter options - matching backend enums
  occasions: string[] = [
    'Casual',
    'BusinessCasual',
    'Formal',
    'Athletic',
    'Social',
    'Work',
    'Date',
    'Travel',
  ];
  seasons: string[] = [ 'Spring', 'Summer', 'Autumn', 'Winter'];
  sortOptions = [
    { value: 'recent', label: 'Recently Created' },
    { value: 'mostWorn', label: 'Most Worn' },
    { value: 'name', label: 'Name (A-Z)' },
  ];

  // Load outfits from backend with filters and pagination
  private loadOutfits(): void {
    this.loading.set(true);
    this.outfitsUseCases.getFilteredOutfits({
      occasion: this.selectedOccasion() || undefined,
      season: this.selectedSeason() || undefined,
      search: this.searchQuery() || undefined,
      sortBy: this.sortBy()
    }, this.page(), this.pageSize()).subscribe({
      next: (result: PagedResult<Outfit>) => {
        this.outfits.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  ngOnInit() {
    this.loadOutfits();
  }

  // Filter handlers
  onOccasionChange(occasion: string) {
    this.selectedOccasion.set(occasion);
    this.page.set(1);
    this.loadOutfits();
  }

  onSeasonChange(season: string) {
    this.selectedSeason.set(season);
    this.page.set(1);
    this.loadOutfits();
  }

  onSearchChange(query: string) {
    this.searchQuery.set(query);
    this.page.set(1);
    this.loadOutfits();
  }

  onSortChange(sort: string) {
    this.sortBy.set(sort);
    this.page.set(1);
    this.loadOutfits();
  }

  clearFilters() {
    this.selectedOccasion.set('');
    this.selectedSeason.set('');
    this.searchQuery.set('');
    this.sortBy.set('recent');
    this.page.set(1);
    this.loadOutfits();
  }

  // Pagination methods
  onPreviousPage() {
    if (this.hasPreviousPage()) {
      this.page.update(p => p - 1);
      this.loadOutfits();
    }
  }

  onNextPage() {
    if (this.hasNextPage()) {
      this.page.update(p => p + 1);
      this.loadOutfits();
    }
  }

  onPageSizeChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.pageSize.set(parseInt(select.value, 10));
    this.page.set(1);
    this.loadOutfits();
  }

  trackByOutfitId(index: number, outfit: any): string {
    return outfit.id;
  }
}
