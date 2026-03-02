import { Component, OnInit, inject, Signal, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
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

import { DragDropModule, CdkDragDrop } from '@angular/cdk/drag-drop';

@Component({
  selector: 'app-wardrobe-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    DragDropModule,
    ClothingCardComponent,
  ],
  templateUrl: './wardrobe-dashboard.component.html',
  styleUrl: './wardrobe-dashboard.component.scss',
})
export class WardrobeDashboardComponent implements OnInit {
  private store = inject(Store<{ wardrobe: WardrobeState }>);
  private snackBar = inject(MatSnackBar);

  // Outfit builder state
  outfitBoardItems = signal<ClothingItem[]>([]);
  savedOutfits = signal<any[]>([]); // Future implementation

  onDrop(event: CdkDragDrop<ClothingItem[]>) {
    // Determine which zone it droped into
    const droppedItem = event.item.data as ClothingItem;
    const targetId = event.container.id;

    if (targetId === 'outfit-board') {
      if (!this.outfitBoardItems().find((i) => i.id === droppedItem.id)) {
        this.outfitBoardItems.update((items) => [...items, droppedItem]);
        this.snackBar.open(`Added ${droppedItem.name} to outfit board`, 'OK', { duration: 2000 });
      } else {
        this.snackBar.open(`${droppedItem.name} is already on the board`, 'OK', { duration: 2000 });
      }
    }
  }

  removeFromBoard(id: string) {
    this.outfitBoardItems.update((items) => items.filter((i) => i.id !== id));
  }

  // Store selectors
  allItems: Signal<ClothingItem[]> = toSignal(this.store.select(selectAllItems), {
    initialValue: [] as ClothingItem[],
  });
  loading: Signal<boolean> = toSignal(this.store.select(selectWardrobeLoading), {
    initialValue: false,
  });
  stats: Signal<WardrobeStats> = toSignal(this.store.select(selectWardrobeStats), {
    initialValue: { totalItems: 0, totalCost: 0 } as WardrobeStats,
  });

  // Computed stats for right sidebar
  mostWornCount = computed(() => {
    const items = this.allItems();
    if (items.length === 0) return 0;
    return Math.max(...items.map((i) => i.wearCount || 0));
  });

  costPerWear = computed(() => {
    const items = this.allItems();
    const totalWears = items.reduce((sum, i) => sum + (i.wearCount || 0), 0);
    const totalCost = items.reduce((sum, i) => sum + (i.purchasePrice || 0), 0);
    return totalWears > 0 ? (totalCost / totalWears).toFixed(2) : '0.00';
  });

  // Filter signals
  activeCategory = signal<string>('All');
  activeColor = signal<string>('All');
  activeSeason = signal<string>('All');
  activeOccasion = signal<string>('All');

  // Filter options
  categories = ['All', 'Tops', 'Bottoms', 'Dresses', 'Outerwear', 'Shoes', 'Accessories'];
  colors = ['All', 'Black', 'White', 'Blue', 'Red', 'Green', 'Pink', 'Beige'];
  seasons = ['All', 'Spring', 'Summer', 'Fall', 'Winter'];
  occasions = ['All', 'Casual', 'Work', 'Formal', 'Sport'];

  // Collapsible filter state
  filtersOpen = {
    categories: true,
    colors: true,
    seasons: true,
    occasions: true,
  };

  // Search
  searchQuery = '';

  // View mode
  viewMode: 'grid' | 'list' = 'grid';

  // Select mode
  selectMode = false;
  selectedItems = new Set<string>();

  // Computed filtered items
  items = computed(() => {
    let filtered = this.allItems().filter((item) => {
      const categoryMatch =
        this.activeCategory() === 'All' ||
        item.type.toLowerCase() === this.activeCategory().toLowerCase() ||
        (this.activeCategory() === 'Shoes' && item.type.toLowerCase() === 'footwear');

      const colorMatch =
        this.activeColor() === 'All' ||
        item.primaryColor.toLowerCase().includes(this.activeColor().toLowerCase());

      const occasionMatch =
        this.activeOccasion() === 'All' ||
        item.category.toLowerCase() === this.activeOccasion().toLowerCase();

      const seasonMatch = true;

      return categoryMatch && colorMatch && occasionMatch && seasonMatch;
    });

    // Search filter
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      filtered = filtered.filter(
        (item) =>
          item.name.toLowerCase().includes(q) ||
          item.type.toLowerCase().includes(q) ||
          item.category.toLowerCase().includes(q) ||
          item.brand?.toLowerCase().includes(q) ||
          item.primaryColor.toLowerCase().includes(q),
      );
    }

    return filtered;
  });

  ngOnInit() {
    this.store.dispatch(WardrobeActions.loadClothingItems());
  }

  onSearch() {
    // Triggers re-computation via the computed signal since searchQuery changed
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
    this.searchQuery = '';
  }

  toggleSelectMode() {
    this.selectMode = !this.selectMode;
    if (!this.selectMode) {
      this.selectedItems.clear();
    }
  }

  toggleSelectItem(id: string) {
    if (this.selectedItems.has(id)) {
      this.selectedItems.delete(id);
    } else {
      this.selectedItems.add(id);
    }
  }

  onDelete(id: string) {
    Swal.fire({
      title: 'Delete Item?',
      text: 'This action cannot be undone.',
      icon: 'warning',
      showCancelButton: true,
      background: '#ffffff',
      color: '#2D3436',
      confirmButtonColor: '#E17055',
      cancelButtonColor: '#DFE6E9',
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
    }).then((result) => {
      if (result.isConfirmed) {
        this.store.dispatch(WardrobeActions.deleteClothingItem({ id }));
      }
    });
  }

  onDeleteSelected() {
    if (this.selectedItems.size === 0) return;
    Swal.fire({
      title: `Delete ${this.selectedItems.size} items?`,
      text: 'This action cannot be undone.',
      icon: 'warning',
      showCancelButton: true,
      background: '#ffffff',
      color: '#2D3436',
      confirmButtonColor: '#E17055',
      cancelButtonColor: '#DFE6E9',
      confirmButtonText: 'Delete All',
      cancelButtonText: 'Cancel',
    }).then((result) => {
      if (result.isConfirmed) {
        this.selectedItems.forEach((id) => {
          this.store.dispatch(WardrobeActions.deleteClothingItem({ id }));
        });
        this.selectedItems.clear();
        this.selectMode = false;
      }
    });
  }

  onRecordWear(id: string) {
    this.store.dispatch(WardrobeActions.recordWear({ id }));
    this.snackBar.open('Wear recorded! 👕', 'Nice!', { duration: 3000 });
  }

  getSelectedWearCount(): number {
    const items = this.allItems();
    return items
      .filter((i) => this.selectedItems.has(i.id))
      .reduce((sum, i) => sum + (i.wearCount || 0), 0);
  }

  getSelectedColor(): string | null {
    const items = this.allItems();
    const selected = items.find((i) => this.selectedItems.has(i.id));
    return selected ? selected.primaryColor : null;
  }

  trackByItem(index: number, item: ClothingItem): string {
    return item.id;
  }
}
