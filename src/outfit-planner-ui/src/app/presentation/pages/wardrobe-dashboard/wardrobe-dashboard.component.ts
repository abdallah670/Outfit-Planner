import { Component, OnInit, inject, Signal, signal, computed, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router, RouterModule } from '@angular/router';
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
  private router = inject(Router);

  @ViewChild('quickAddFileInput') quickAddFileInput!: ElementRef<HTMLInputElement>;

  // Outfit builder state
  outfitBoardItems = signal<ClothingItem[]>([]);
  savedOutfits = signal<any[]>([]); // Future implementation


  addSelectedToOutfit() {
    const selectedIds = Array.from(this.selectedItems);
    if (selectedIds.length === 0) return;

    // Navigate to outfit builder with selected item IDs as query params
    this.router.navigate(['/outfits/build'], {
      queryParams: { items: selectedIds.join(',') }
    });

    // Clear selection
    this.selectedItems.clear();
    this.selectMode = false;
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
  totalItems = computed(() => this.items().length);
  
  mostWornItem = computed(() => {
    const items = this.items();
    if (items.length === 0) return null;
    return items.reduce((max: ClothingItem | null, item: ClothingItem) => {
      if (!max || (item.wearCount || 0) > (max.wearCount || 0)) {
        return item;
      }
      return max;
    }, null);
  });
  
  mostWornCount = computed(() => this.mostWornItem()?.wearCount || 0);
  mostWornImage = computed(() => this.mostWornItem()?.imageUrl || this.mostWornItem()?.thumbnailUrl || 'assets/placeholder.jpg');

  costPerWear = computed(() => {
    const items = this.items();
    const totalWears = items.reduce((sum: number, i: ClothingItem) => sum + (i.wearCount || 0), 0);
    const totalCost = items.reduce((sum: number, i: ClothingItem) => sum + (i.purchasePrice || 0), 0);
    return totalWears > 0 ? (totalCost / totalWears).toFixed(2) : '0.00';
  });

  // Filter signals
  activeType = signal<string>('All');
  activeCategory = signal<string>('All');
  activeColor = signal<string>('All');
  activeCondition = signal<string>('All');
  activeFabric = signal<string>('All');
  activeSize = signal<string>('All');
  minPrice = signal<number | null>(null);
  maxPrice = signal<number | null>(null);
  searchQuery = signal<string>('');

  // Filter options
  // Clothing types (maps to Type enum: Top, Bottom, Dress, etc.)
  types = ['All', 'Top', 'Bottom', 'Dress', 'Outerwear', 'Footwear', 'Accessory', 'Undergarment', 'Swimwear', 'Activewear'];
  // Categories (e.g. Casual, Formal, Sport – stored as free-text)
  categories = ['All', 'Casual', 'Formal', 'Sport', 'Business', 'Work', 'Social', 'Date', 'Travel'];
  // Color names (matches GetColorName mapping)
  colors = ['All', 'Black', 'White', 'Blue', 'Red', 'Green', 'Pink', 'Beige', 'Gray', 'Yellow', 'Orange', 'Purple', 'Navy'];
  conditions = ['All', 'good', 'fair', 'poor', 'excellent'];
  fabrics = ['All', 'Cotton', 'Polyester', 'Wool', 'Silk', 'Linen', 'Leather', 'Denim', 'Nylon', 'Spandex', 'Rayon', 'Other'];
  sizes = ['All', 'XS', 'S', 'M', 'L', 'XL', 'XXL'];

   // Collapsible filter state
   filtersOpen = {
     types: true,
     colors: true,
     conditions: true,
     categories: true,   // now in right sidebar (advanced)
     fabrics: true,
     sizes: true,
     price: true,
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
       condition: this.activeCondition() !== 'All' ? this.activeCondition() : undefined,
       fabric: this.activeFabric() !== 'All' ? this.activeFabric() : undefined,
       type: this.activeType() !== 'All' ? this.activeType() : undefined,
       size: this.activeSize() !== 'All' ? this.activeSize() : undefined,
       minPrice: this.minPrice(),
       maxPrice: this.maxPrice(),
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


   setCategory(category: string) {
     this.activeCategory.set(category);
     this.page.set(1);
     this.loadItems();
   }

   setType(type: string) {
     this.activeType.set(type);
     this.page.set(1);
     this.loadItems();
   }
   setColor(color: string) {
     this.activeColor.set(color);
     this.page.set(1);
     this.loadItems();
   }
   setCondition(condition: string) {
     this.activeCondition.set(condition);
     this.page.set(1);
     this.loadItems();
   }
   setFabric(fabric: string) {
     this.activeFabric.set(fabric);
     this.page.set(1);
     this.loadItems();
   }
   setSearchQuery(query: string) {
    this.searchQuery.set(query);
    this.page.set(1);
    this.loadItems();
  }
  setSize(size: string) {
    this.activeSize.set(size);
     this.page.set(1);
     this.loadItems();
   }
  setMinPrice(price: number | null) {
    this.minPrice.set(price);
    this.page.set(1);
    this.loadItems();
  }
  setMaxPrice(price: number | null) {
    this.maxPrice.set(price);
    this.page.set(1);
    this.loadItems();
  }

   clearBasicFilters() {
     this.activeType.set('All');
     this.activeCategory.set('All');
     this.activeColor.set('All');
     this.activeCondition.set('All');
     this.page.set(1);
     this.loadItems();
   }

  clearAdvancedFilters() {
    this.activeFabric.set('All');
    this.activeSize.set('All');
    this.minPrice.set(null);
    this.maxPrice.set(null);
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

  // Quick Add with image only
  triggerQuickAdd() {
    this.quickAddFileInput.nativeElement.click();
  }

  onQuickAddFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    // Create item with defaults and image
    const itemData = {
      name: `New Item ${new Date().toLocaleTimeString()}`,
      type: 'Top',
      category: 'Casual',
      brand: 'Unknown',
      primaryColor: '#808080',
      description: '',
      purchasePrice: 0,
      size: 'M',
      condition: 'good',
      fabric: 'Cotton',
      currency: 'USD',
    };

    this.wardrobeService.createClothingItem(itemData, file).subscribe({
      next: () => {
        this.snackBar.open('Item added successfully!', 'OK', { duration: 3000 });
        this.loadItems(); // Refresh the list
      },
      error: () => {
        this.snackBar.open('Failed to add item', 'OK', { duration: 3000 });
      }
    });

    // Reset input
    input.value = '';
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
