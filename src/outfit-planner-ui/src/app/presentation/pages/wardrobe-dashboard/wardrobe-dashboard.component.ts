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
import { WardrobeService } from '../../../core/services/wardrobe.service';
import { PagedResult } from '../../../domain/entities/response.entity';

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
  private wardrobeService = inject(WardrobeService);

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

  // Pagination signals
  page = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);

  // Backend loaded items
  items = signal<ClothingItem[]>([]);
  loading = signal(false);

  // Store stats (still from store for now)
  stats: Signal<WardrobeStats> = toSignal(this.store.select(selectWardrobeStats), {
    initialValue: { totalItems: 0, totalCost: 0 } as WardrobeStats,
  });

  // Pagination computed
  hasPreviousPage = computed(() => this.page() > 1);
  hasNextPage = computed(() => this.page() * this.pageSize() < this.totalCount());
  totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()) || 1);

  // Computed stats for right sidebar (using loaded items for now)
  mostWornCount = computed(() => {
    const items = this.items();
    if (items.length === 0) return 0;
    return Math.max(...items.map((i: ClothingItem) => i.wearCount || 0));
  });

  costPerWear = computed(() => {
    const items = this.items();
    const totalWears = items.reduce((sum: number, i: ClothingItem) => sum + (i.wearCount || 0), 0);
    const totalCost = items.reduce((sum: number, i: ClothingItem) => sum + (i.purchasePrice || 0), 0);
    return totalWears > 0 ? (totalCost / totalWears).toFixed(2) : '0.00';
  });

  // Filter signals
  activeCategory = signal<string>('All');
  activeColor = signal<string>('All');
  activeSeason = signal<string>('All');
  activeOccasion = signal<string>('All');
  searchQuery = signal<string>('');

  // Filter options - matching backend enums
  categories = ['All', 'Tops', 'Bottoms', 'Dresses', 'Outerwear', 'Shoes', 'Accessories'];
  colors = ['All', 'Black', 'White', 'Blue', 'Red', 'Green', 'Pink', 'Beige'];
  occasions = [
    'All',
    'Casual',
    'Work',
    'Formal',
    'Athletic',
    'BusinessCasual',
    'Social',
    'Date',
    'Travel',
  ];

  // Collapsible filter state
  filtersOpen = {
    categories: true,
    colors: true,
    seasons: true,
    occasions: true,
  };

  // View mode
  viewMode: 'grid' | 'list' = 'grid';

  // Select mode
  selectMode = false;
  selectedItems = new Set<string>();

  // Load items from backend with filters and pagination
  private loadItems(): void {
    this.loading.set(true);
    this.wardrobeService.getFilteredItems({
      category: this.activeCategory() !== 'All' ? this.activeCategory() : undefined,
      color: this.activeColor() !== 'All' ? this.activeColor() : undefined,
      occasion: this.activeOccasion() !== 'All' ? this.activeOccasion() : undefined,
      search: this.searchQuery() || undefined
    }, this.page(), this.pageSize()).subscribe({
      next: (result: PagedResult<ClothingItem>) => {
        this.items.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  ngOnInit() {
    this.loadItems();
  }

  onSearch() {
    this.page.set(1);
    this.loadItems();
  }

  onSearchChange(query: string) {
    this.searchQuery.set(query);
  }

  setCategory(category: string) {
    this.activeCategory.set(category);
    this.page.set(1);
    this.loadItems();
  }
  setColor(color: string) {
    this.activeColor.set(color);
    this.page.set(1);
    this.loadItems();
  }
  setOccasion(occasion: string) {
    this.activeOccasion.set(occasion);
    this.page.set(1);
    this.loadItems();
  }

  clearFilters() {
    this.activeCategory.set('All');
    this.activeColor.set('All');
    this.activeOccasion.set('All');
    this.searchQuery.set('');
    this.page.set(1);
    this.loadItems();
  }

  // Pagination methods
  onPreviousPage() {
    if (this.hasPreviousPage()) {
      this.page.update(p => p - 1);
      this.loadItems();
    }
  }

  onNextPage() {
    if (this.hasNextPage()) {
      this.page.update(p => p + 1);
      this.loadItems();
    }
  }

  onPageSizeChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.pageSize.set(parseInt(select.value, 10));
    this.page.set(1);
    this.loadItems();
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
    const items = this.items();
    return items
      .filter((i: ClothingItem) => this.selectedItems.has(i.id))
      .reduce((sum: number, i: ClothingItem) => sum + (i.wearCount || 0), 0);
  }

  getSelectedColor(): string | null {
    const items = this.items();
    const selected = items.find((i: ClothingItem) => this.selectedItems.has(i.id));
    return selected ? selected.primaryColor : null;
  }

  trackByItem(index: number, item: ClothingItem): string {
    return item.id;
  }
}
