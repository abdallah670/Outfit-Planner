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
   selectedHexColor = signal<string>('#ffffff'); // Default to white

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
      
      // Determine what to send for color filter:
      // - If we have a direct hex color from picker that's not a standard name, send it as-is
      // - Otherwise, use the activeColor signal (which contains color names)
      let colorFilter: string | undefined;
      const hexColor = this.selectedHexColor();
      
      // If the activeColor is a standard color name, use it
      // Otherwise, if we have a custom hex color, send that
      if (this.colors.includes(this.activeColor())) {
        // Standard color selected from preset circles
        colorFilter = this.activeColor() !== 'All' ? this.activeColor() : undefined;
      } else if (hexColor && hexColor !== '#ffffff' && this.activeColor() !== 'All') {
        // Custom hex color from picker (not default white)
        colorFilter = hexColor;
      }
      
      this.wardrobeService.getFilteredItems({
        category: this.activeCategory() !== 'All' ? this.activeCategory() : undefined,
        color: colorFilter,
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

    // Color picker handler
    onColorPickerChange(event: Event) {
      const hexColor = (event.target as HTMLInputElement).value;
      this.selectedHexColor.set(hexColor);
      
      // Convert hex to color name for backend filtering
      const colorName = this.hexToColorName(hexColor);
      if (colorName !== 'Black') { // Only set if we got a recognizable color
        this.setColor(colorName);
      } else if (hexColor !== '#000000') {
        // For unknown colors, still try to filter by the exact hex
        this.activeColor.set(hexColor);
        this.page.set(1);
        this.loadItems();
      }
    }

    // Helper: convert hex color to canonical color name (must match backend GetHexCodesForColor logic)
    private hexToColorName(hexColor: string): string {
      if (!hexColor) return 'Black';

      let hex = hexColor.replace('#', '').toLowerCase();

      // Remove leading zeros for comparison
      while (hex.length > 0 && hex[0] == '0') hex = hex.substring(1);
      if (hex.length == 0) hex = '000';

      // Pad to 6 digits if needed
      while (hex.length < 6) hex = hex + '0';

      // Normalize short forms (3-digit)
      if (hex.length == 3)
        hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];

      // Map common hex ranges to color names (must match backend)
      switch (hex) {
        // Black & very dark
        case '000000':
        case '010101':
        case '1f2937':
        case '111827':
        case '0f172a':
          return 'Black';

        // White & very light
        case 'ffffff':
        case 'f3f4f6':
        case 'f9fafb':
        case 'f5f5f5':
        case 'e5e7eb':
        case 'fafafa':
          return 'White';

        // Gray family
        case '808080':
        case '9ca3af':
        case 'd1d5db':
        case '6b7280':
          return 'Gray';

        // Blue family
        case '3b82f6':
        case '2563eb':
        case '1d4ed8':
        case '60a5fa':
        case '93c5fd':
        case 'dbeafe':
        case '0ea5e9':
          return 'Blue';

        // Red family
        case 'ef4444':
        case 'dc2626':
        case 'b91c1c':
        case 'f87171':
        case 'fecaca':
        case 'f43f5e':
          return 'Red';

        // Green family
        case '22c55e':
        case '16a34a':
        case '15803d':
        case '86efac':
        case '4ade80':
        case 'bbf7d0':
        case '9caf88':
        case '84cc16':
          return 'Green';

        // Pink family
        case 'ec4899':
        case 'db2777':
        case 'be185d':
        case 'f472b6':
        case 'fce7f3':
        case 'f8b4c4':
        case 'f43f5e':
          return 'Pink';

        // Beige / Tan / Light brown
        case 'd6b78a':
        case 'd6c4a0':
        case 'c9a66b':
        case 'b8956f':
        case 'a1887f':
        case 'e7d9c4':
        case 'f5deb3':
          return 'Beige';

        // Yellow
        case 'fbbf24':
        case 'f59e0b':
        case 'd97706':
        case 'fef08a':
        case 'fef9c3':
        case 'fde047':
          return 'Yellow';

        // Orange
        case 'fb923c':
        case 'f97316':
        case 'ea580c':
        case 'fed7aa':
        case 'ff8a00':
          return 'Orange';

        // Purple
        case '8b5cf6':
        case '7c3aed':
        case '6d28d9':
        case 'a78bfa':
        case 'd8b4fe':
        case 'c084fc':
          return 'Purple';

        // Navy / Dark blue
        case '1e3a5f':
        case '1e40af':
        case '1e3a8a':
        case '172554':
        case '0f172a':
          return 'Navy';

        default:
          return 'Black'; // default fallback
      }
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
