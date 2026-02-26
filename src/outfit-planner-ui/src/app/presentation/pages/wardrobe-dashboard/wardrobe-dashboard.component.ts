import { Component, OnInit, inject, Signal, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import Swal from 'sweetalert2';

import { WardrobeActions } from '../../../core/state/wardrobe/wardrobe.actions';
import {
  selectAllItems,
  selectWardrobeLoading,
  selectWardrobeStats,
  WardrobeStats,
} from '../../../core/state/wardrobe/wardrobe.selectors';
import { ClothingCardComponent } from '../../components/clothing-card/clothing-card.component';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';
import { WardrobeState } from '../../../core/state/wardrobe/wardrobe.reducer';

@Component({
  selector: 'app-wardrobe-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatSnackBarModule,
    ClothingCardComponent,
  ],
  templateUrl: './wardrobe-dashboard.component.html',
  styleUrl: './wardrobe-dashboard.component.scss',
})
export class WardrobeDashboardComponent implements OnInit {
  private store = inject(Store<{ wardrobe: WardrobeState }>);
  private snackBar = inject(MatSnackBar);

  // Wardrobe Items from Store
  allItems: Signal<ClothingItem[]> = toSignal(this.store.select(selectAllItems), {
    initialValue: [] as ClothingItem[],
  });
  loading: Signal<boolean> = toSignal(this.store.select(selectWardrobeLoading), {
    initialValue: false,
  });
  stats: Signal<WardrobeStats> = toSignal(this.store.select(selectWardrobeStats), {
    initialValue: { totalItems: 0, totalCost: 0 } as WardrobeStats,
  });

  // Filter Signals
  activeCategory = signal<string>('All');
  activeColor = signal<string>('All');
  activeSeason = signal<string>('All');
  activeOccasion = signal<string>('All');

  // Filter Options
  categories = ['All', 'Tops', 'Bottoms', 'Dresses', 'Outerwear', 'Shoes', 'Accessories'];
  colors = ['All', 'Black', 'White', 'Blue', 'Red', 'Green', 'Pink', 'Beige'];
  seasons = ['All', 'Spring', 'Summer', 'Fall', 'Winter'];
  occasions = ['All', 'Casual', 'Work', 'Formal', 'Sport'];

  // Computed Filtered Items
  items = computed(() => {
    return this.allItems().filter((item) => {
      // Design 'Category' maps to App 'type'
      const categoryMatch =
        this.activeCategory() === 'All' ||
        item.type.toLowerCase() === this.activeCategory().toLowerCase() ||
        (this.activeCategory() === 'Shoes' && item.type.toLowerCase() === 'footwear');

      // Design 'Color' maps to App 'primaryColor'
      const colorMatch =
        this.activeColor() === 'All' ||
        item.primaryColor.toLowerCase().includes(this.activeColor().toLowerCase());

      // Design 'Occasion' maps to App 'category'
      const occasionMatch =
        this.activeOccasion() === 'All' ||
        item.category.toLowerCase() === this.activeOccasion().toLowerCase();

      // Season is currently not in the entity, so we skip it or match all
      const seasonMatch = true;

      return categoryMatch && colorMatch && occasionMatch && seasonMatch;
    });
  });

  ngOnInit() {
    this.store.dispatch(WardrobeActions.loadClothingItems());
  }

  setCategory(category: string) {
    this.activeCategory.set(category);
  }

  setColor(color: string) {
    this.activeColor.set(color);
  }

  setSeason(season: string) {
    this.activeSeason.set(season);
  }

  setOccasion(occasion: string) {
    this.activeOccasion.set(occasion);
  }

  clearFilters() {
    this.activeCategory.set('All');
    this.activeColor.set('All');
    this.activeSeason.set('All');
    this.activeOccasion.set('All');
  }

  onDelete(id: string) {
    Swal.fire({
      title: 'Are you sure?',
      text: "You won't be able to revert this! The clothing item will be permanently deleted.",
      icon: 'warning',
      showCancelButton: true,
      background: '#1f2937',
      color: '#f9fafb',
      confirmButtonColor: '#ef4444',
      cancelButtonColor: '#4b5563',
      confirmButtonText: 'Yes, delete it!',
    }).then((result) => {
      if (result.isConfirmed) {
        this.store.dispatch(WardrobeActions.deleteClothingItem({ id }));
      }
    });
  }

  onRecordWear(id: string) {
    this.store.dispatch(WardrobeActions.recordWear({ id }));
    (this.snackBar as any).open(`Wear recorded!`, 'Sweet!', { duration: 3000 });
  }
}
