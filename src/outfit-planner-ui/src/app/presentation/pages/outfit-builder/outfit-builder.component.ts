import { Component, OnInit, OnDestroy, inject, Signal, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import {
  CdkDragDrop,
  DragDropModule,
  moveItemInArray,
  transferArrayItem,
} from '@angular/cdk/drag-drop';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, Subscription, combineLatest, filter, first, take } from 'rxjs';

import { selectAllItems } from '../../../core/state/wardrobe/wardrobe.selectors';
import { WardrobeActions } from '../../../core/state/wardrobe/wardrobe.actions';
import { WardrobeState } from '../../../core/state/wardrobe/wardrobe.reducer';
import { OutfitsActions } from '../../../core/state/outfit/outfit.actions';
import { OutfitState } from '../../../core/state/outfit/outfit.reducer';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';
import { OccasionType, Season, OutfitItem, Outfit } from '../../../domain/entities/outfit.entity';
import { WeatherDisplayComponent } from '../../components/weather-display/weather-display.component';
import { WeatherActions } from '../../../core/state/weather/weather.actions';
import { toSignal } from '@angular/core/rxjs-interop';
import { Weather } from '../../../domain/entities/weather.entity';
import { WardrobeService } from '../../../core/services/wardrobe.service';
import {
  selectCurrentWeather,
  selectWeatherLoading,
} from '../../../core/state/weather/weather.selectors';
import { selectSelectedItem } from '../../../core/state/outfit/outfit.selectors';

@Component({
  selector: 'app-outfit-builder',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCardModule,
    MatDividerModule,
    MatChipsModule,
    MatSnackBarModule,
    DragDropModule,
    WeatherDisplayComponent,
  ],
  templateUrl: './outfit-builder.component.html',
  styleUrl: './outfit-builder.component.scss',
})
export class OutfitBuilderComponent implements OnInit, OnDestroy {
  private store = inject(Store<{ wardrobe: WardrobeState; outfit: OutfitState }>);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private wardrobeService = inject(WardrobeService);
  private snackBar = inject(MatSnackBar);
  private sub?: Subscription;

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  editingId: string | null = null;

  weather: Signal<Weather | null> = toSignal(this.store.select(selectCurrentWeather), {
    initialValue: null,
  });

  weatherLoading: Signal<boolean> = toSignal(this.store.select(selectWeatherLoading), {
    initialValue: false,
  });

  wardrobeItems$: Observable<ClothingItem[]> = this.store.select(selectAllItems);

  private getCurrentSeason(): Season {
    const month = new Date().getMonth();
    // Northern Hemisphere basic season check
    // 11, 0, 1 = Winter (Dec, Jan, Feb)
    if (month === 11 || month === 0 || month === 1) return Season.Winter;
    // 2, 3, 4 = Spring (Mar, Apr, May)
    if (month >= 2 && month <= 4) return Season.Spring;
    // 5, 6, 7 = Summer (Jun, Jul, Aug)
    if (month >= 5 && month <= 7) return Season.Summer;
    // 8, 9, 10 = Fall/Autumn (Sep, Oct, Nov)
    return Season.Autumn;
  }

  builderForm = this.fb.group({
    name: [`Outfit - ${new Date().toLocaleDateString()}`, [Validators.required]],
    occasion: [OccasionType.Casual, [Validators.required]],
    season: [this.getCurrentSeason(), [Validators.required]],
  });

  selectedItems: ClothingItem[] = [];
  itemSettings: Map<string, { role: 'primary' | 'secondary' | 'accent'; layer: number }> =
    new Map();

  // Dynamic stats calculated from wardrobe data
  colorHarmony = 0;
  stats: {
    totalItems: number;
    mostWornItem: ClothingItem | null;
    mostWorn: number;
    costPerWear: number;
    topCategory: string;
  } = {
    totalItems: 0,
    mostWornItem: null,
    mostWorn: 0,
    costPerWear: 0,
    topCategory: 'Casual',
  };
  searchQuery = '';
  occasions = Object.values(OccasionType);
  seasons = Object.values(Season);

  categories = ['Tops', 'Bottoms', 'Outerwear', 'Shoes', 'Accessories'];
  expandedCategories = new Set<string>(['Tops']);
  onSearch() {
    // Triggers re-computation via the computed signal since searchQuery changed
  }
  getCategoryItems(category: string, allItems: ClothingItem[]): ClothingItem[] {
    return allItems.filter((item) => {
      const type = (item.type || '').toLowerCase();
      const cat = (item.category || '').toLowerCase();
      const catLower = category.toLowerCase();

      if (catLower === 'shoes')
        return type === 'footwear' || type === 'shoes' || cat === 'shoes' || cat === 'footwear';
      if (catLower === 'accessories')
        return (
          type === 'accessory' ||
          type === 'accessories' ||
          cat === 'accessories' ||
          cat === 'accessory'
        );
      if (catLower === 'outerwear')
        return type === 'outerwear' || cat === 'outerwear' || type === 'jacket' || cat === 'jacket';
      if (catLower === 'tops')
        return (
          type === 'top' || type === 'tops' || cat === 'tops' || cat === 'top' || type === 'shirt'
        );
      if (catLower === 'bottoms')
        return (
          type === 'bottom' ||
          type === 'bottoms' ||
          cat === 'bottoms' ||
          cat === 'bottom' ||
          type === 'pants'
        );

      return type.includes(catLower.replace(/s$/, ''));
    });
  }

  toggleCategory(cat: string) {
    if (this.expandedCategories.has(cat)) {
      this.expandedCategories.delete(cat);
    } else {
      this.expandedCategories.add(cat);
    }
  }

  isItemSelected(item: ClothingItem): boolean {
    return !!this.selectedItems.find((i) => i.id === item.id);
  }

  ngOnInit() {
    this.store.dispatch(WardrobeActions.loadClothingItems());

    // Subscribe to wardrobe items to compute stats dynamically
    this.sub = this.wardrobeItems$.subscribe((items: ClothingItem[]) => {
      if (items && items.length > 0) {
        this.stats.totalItems = items.length;

        // Find most-worn item
        const sorted = [...items].sort((a, b) => (b.wearCount || 0) - (a.wearCount || 0));
        this.stats.mostWornItem = sorted[0] || null;
        this.stats.mostWorn = sorted[0]?.wearCount || 0;

        // Calculate average cost per wear
        const itemsWithCost = items.filter(
          (i: ClothingItem) => i.purchasePrice > 0 && i.wearCount > 0,
        );
        if (itemsWithCost.length > 0) {
          const totalCostPerWear = itemsWithCost.reduce(
            (sum: number, i: ClothingItem) => sum + i.purchasePrice / i.wearCount,
            0,
          );
          this.stats.costPerWear = totalCostPerWear / itemsWithCost.length;
        } else {
          this.stats.costPerWear = 0;
        }

        // Determine top category
        const catCount: Record<string, number> = {};
        items.forEach((i: ClothingItem) => {
          const cat = i.category || 'Other';
          catCount[cat] = (catCount[cat] || 0) + 1;
        });
        this.stats.topCategory =
          Object.entries(catCount).sort((a, b) => b[1] - a[1])[0]?.[0] || 'Casual';
      }
    });
    this.store.dispatch(WeatherActions.loadCurrentWeather({ city: 'Cairo' }));

    // Check for Edit Mode
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editingId = id;
      this.store.dispatch(OutfitsActions.loadOutfitById({ id }));

      // Wait for both wardrobe items and the outfit to be loaded, then populate
      combineLatest([
        this.wardrobeItems$.pipe(
          filter((items: ClothingItem[]) => items.length > 0),
          take(1),
        ),
        this.store.select(selectSelectedItem).pipe(
          filter((o: Outfit | null): o is Outfit => !!o && o.id === id),
          take(1),
        ),
      ]).subscribe(([wardrobeItems, outfit]: [ClothingItem[], Outfit]) => {
        if (outfit) {
          this.populateEditMode(outfit, wardrobeItems);
        }
      });
    }

    // Check for items from query params (from wardrobe selection)
    const itemsParam = this.route.snapshot.queryParamMap.get('items');
    if (itemsParam && !id) {
      const itemIds = itemsParam.split(',');
      // Wait for wardrobe items to load, then auto-add selected items
      this.wardrobeItems$.pipe(
        filter((items: ClothingItem[]) => items.length > 0),
        take(1),
      ).subscribe((wardrobeItems: ClothingItem[]) => {
        itemIds.forEach((id) => {
          const item = wardrobeItems.find((i) => i.id === id);
          if (item) {
            this.addItem(item);
          }
        });
        if (itemIds.length > 0) {
          this.snackBar.open(`Added ${itemIds.length} item${itemIds.length > 1 ? 's' : ''} from wardrobe`, 'OK', { duration: 3000 });
        }
      });
    }
  }

  private populateEditMode(outfit: any, wardrobeItems: ClothingItem[]) {
    this.builderForm.patchValue({
      name: outfit.name,
      occasion: outfit.occasion,
      season: outfit.season,
    });

    this.selectedItems = [];
    this.itemSettings.clear();

    if (outfit.items) {
      outfit.items.forEach((oi: any) => {
        const fullItem = wardrobeItems.find((i) => i.id === oi.clothingItemId);
        if (fullItem) {
          this.selectedItems.push(fullItem);
          this.itemSettings.set(oi.clothingItemId, {
            role: oi.role,
            layer: oi.layeringOrder,
          });
        }
      });
      this.recalculateHarmony();
    }
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  drop(event: CdkDragDrop<ClothingItem[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // Instead of transferArrayItem which removes it from the wardrobe list, we just add it to the canvas
      const item = event.item.data as ClothingItem;
      this.addItem(item);
    }
    this.recalculateHarmony();
  }

  addItem(item: ClothingItem) {
    if (!this.selectedItems.find((i) => i.id === item.id)) {
      this.selectedItems = [...this.selectedItems, item];
      this.itemSettings.set(item.id, { role: 'primary', layer: this.selectedItems.length - 1 });
      this.recalculateHarmony();
    }
  }

  removeItem(item: ClothingItem) {
    this.selectedItems = this.selectedItems.filter((i) => i.id !== item.id);
    this.itemSettings.delete(item.id);
    this.recalculateHarmony();
  }

  clearCanvas() {
    this.selectedItems = [];
    this.itemSettings.clear();
    this.colorHarmony = 0;
  }

  updateItemRole(itemId: string, role: 'primary' | 'secondary' | 'accent') {
    const settings = this.itemSettings.get(itemId);
    if (settings) {
      this.itemSettings.set(itemId, { ...settings, role });
    }
  }

  recalculateHarmony() {
    if (this.selectedItems.length === 0) {
      this.colorHarmony = 0;
      return;
    }
    if (this.selectedItems.length === 1) {
      this.colorHarmony = 100;
      return;
    }

    // Simple color harmony: score goes down if too many items share the same color
    const colors = this.selectedItems.map((i) => (i.primaryColor || '#000').toLowerCase());
    const uniqueColors = new Set(colors);
    const variety = uniqueColors.size / colors.length;
    // Base score: high variety = high harmony
    this.colorHarmony = Math.min(100, Math.round(60 + variety * 40));
  }

  saveOutfit() {
    if (this.selectedItems.length === 0) return;

    const formValues = this.builderForm.value;
    const outfitItems: Partial<OutfitItem>[] = this.selectedItems.map((item, index) => {
      const settings = this.itemSettings.get(item.id);
      return {
        clothingItemId: item.id,
        role: settings?.role || 'primary',
        layeringOrder: settings?.layer || index,
        isEssential: settings?.role === 'primary',
      };
    });

    const newOutfit: any = {
      name: formValues.name!,
      occasion: formValues.occasion as OccasionType,
      season: formValues.season as Season,
      items: outfitItems as OutfitItem[],
    };

    if (this.editingId) {
      newOutfit.id = this.editingId;
      this.store.dispatch(OutfitsActions.updateOutfit({ id: this.editingId, outfit: newOutfit }));
    } else {
      this.store.dispatch(OutfitsActions.createOutfit({ outfit: newOutfit }));
    }
  }

  cancel() {
    this.router.navigate(['/outfits']);
  }

  // Upload custom image as clothing item
  triggerFileUpload(): void {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    // Generate a default name based on timestamp
    const defaultName = `Custom Item ${new Date().toLocaleTimeString()}`;
    this.uploadCustomItem(file, defaultName);

    // Reset input for re-upload
    input.value = '';
  }

  private uploadCustomItem(file: File, name: string): void {
    const itemData: Partial<ClothingItem> = {
      name,
      category: 'Custom',
      type: 'Top',
      brand: 'Custom',
      fabric: 'Cotton',
      size: 'N/A',
      condition: 'Good',
      primaryColor: '#808080',
      purchasePrice: 0,
      currency: 'USD',
    };

    this.wardrobeService.createClothingItem(itemData, file).subscribe({
      next: (newItem: ClothingItem) => {
        // Add to canvas immediately
        this.addItem(newItem);

        // Refresh wardrobe list
        this.store.dispatch(WardrobeActions.loadClothingItems());

        // Show success message
        this.snackBar.open(`${newItem.name} added to outfit!`, 'Close', {
          duration: 3000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
        });
      },
      error: (err: unknown) => {
        console.error('Upload failed:', err);
        this.snackBar.open('Failed to upload image. Please try again.', 'Close', {
          duration: 3000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
        });
      },
    });
  }
}
