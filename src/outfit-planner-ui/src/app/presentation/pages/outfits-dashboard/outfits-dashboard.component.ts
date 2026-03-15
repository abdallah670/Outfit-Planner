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
import { Observable, map } from 'rxjs';

import { OutfitsActions } from '../../../core/state/outfit/outfit.actions';
import { selectAllOutfits, selectOutfitLoading } from '../../../core/state/outfit/outfit.selectors';
import { OutfitState } from '../../../core/state/outfit/outfit.reducer';
import { OutfitCardComponent } from '../../components/outfits/outfit-card/outfit-card.component';
import { Outfit } from '../../../domain/entities/outfit.entity';
import { OccasionType, Season } from '../../../domain/entities/outfit.entity';

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

  outfits$: Observable<Outfit[]> = this.store.select(selectAllOutfits);
  loading$: Observable<boolean> = this.store.select(selectOutfitLoading);

  // Filter state
  selectedOccasion = signal<string>('');
  selectedSeason = signal<string>('');
  searchQuery = signal<string>('');
  sortBy = signal<string>('recent');

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

  // Convert Observable to signal for computed filtering
  private outfitsSignal = signal<Outfit[]>([]);

  // Computed filtered and sorted outfits
  filteredOutfits = computed(() => {
    let result = this.outfitsSignal();

    // Filter by occasion
    const occasion = this.selectedOccasion();
    if (occasion) {
      result = result.filter((outfit) => outfit.occasion?.toLowerCase() === occasion.toLowerCase());
    }

    // Filter by season
    const season = this.selectedSeason();
    if (season) {
      // Handle mapping between Fall (UI) and Autumn (backend)
      const seasonMap: { [key: string]: string } = {
        Fall: 'Autumn',
        Autumn: 'Autumn',
        Spring: 'Spring',
        Summer: 'Summer',
        Winter: 'Winter',
      
      };
      const mappedSeason = seasonMap[season] || season;
      result = result.filter(
        (outfit) => outfit.season?.toLowerCase() === mappedSeason.toLowerCase(),
      );
    }

    // Filter by search query
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      result = result.filter(
        (outfit) =>
          outfit.name?.toLowerCase().includes(query) ||
          outfit.occasion?.toLowerCase().includes(query) ||
          outfit.season?.toLowerCase().includes(query),
      );
    }

    // Sort results
    const sort = this.sortBy();
    switch (sort) {
      case 'mostWorn':
        result = [...result].sort((a, b) => (b.timesWorn || 0) - (a.timesWorn || 0));
        break;
      case 'name':
        result = [...result].sort((a, b) => (a.name || '').localeCompare(b.name || ''));
        break;
      case 'recent':
      default:
        result = [...result].sort(
          (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
        );
        break;
    }

    return result;
  });

  ngOnInit() {
    this.store.dispatch(OutfitsActions.loadOutfits());

    // Subscribe to outfits$ and update signal
    this.outfits$.subscribe((outfits: Outfit[]) => {
      this.outfitsSignal.set(outfits);
    });
  }

  // Filter handlers
  onOccasionChange(occasion: string) {
    this.selectedOccasion.set(occasion);
  }

  onSeasonChange(season: string) {
    this.selectedSeason.set(season);
  }

  onSearchChange(query: string) {
    this.searchQuery.set(query);
  }

  onSortChange(sort: string) {
    this.sortBy.set(sort);
  }

  clearFilters() {
    this.selectedOccasion.set('');
    this.selectedSeason.set('');
    this.searchQuery.set('');
    this.sortBy.set('recent');
  }

  trackByOutfitId(index: number, outfit: any): string {
    return outfit.id;
  }
}
