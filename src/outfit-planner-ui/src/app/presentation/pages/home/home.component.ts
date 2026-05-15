import { Component, inject, OnInit, Signal, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { Weather } from '../../../domain/entities/weather.entity';

import { WeatherDisplayComponent } from '../../components/weather-display/weather-display.component';
import { DailyPickComponent } from '../../components/shared/daily-pick/daily-pick.component';
import { WardrobeHealthComponent } from '../../components/shared/wardrobe-health/wardrobe-health.component';
import { ClothingCardComponent } from '../../components/clothing-card/clothing-card.component';
import { WardrobeActions } from '../../../core/state/wardrobe/wardrobe.actions';
import { selectAllItems } from '../../../core/state/wardrobe/wardrobe.selectors';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';
import { WeatherActions } from '../../../core/state/weather/weather.actions';
import {
  selectCurrentWeather,
  selectWeatherLoading,
} from '../../../core/state/weather/weather.selectors';
import { OutfitsActions } from '../../../core/state/outfit/outfit.actions';
import { TrendingActions } from '../../../core/state/trending/trending.actions';
import { selectTrendingOutfits, selectTrendingLoading } from '../../../core/state/trending/trending.selectors';
import { TrendingOutfit } from '../../../domain/entities/outfit.entity';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    WeatherDisplayComponent,
    DailyPickComponent,
    WardrobeHealthComponent,
    ClothingCardComponent,
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  private store = inject(Store);
  failedImages = new Set<string>();

  items: Signal<ClothingItem[]> = toSignal(this.store.select(selectAllItems), {
    initialValue: [] as ClothingItem[],
  });

  weather: Signal<Weather | null> = toSignal(this.store.select(selectCurrentWeather), {
    initialValue: null,
  });

  weatherLoading: Signal<boolean> = toSignal(this.store.select(selectWeatherLoading), {
    initialValue: false,
  });

  trendingOutfits: Signal<TrendingOutfit[]> = toSignal(this.store.select(selectTrendingOutfits), {
    initialValue: [],
  });

  trendingLoading: Signal<boolean> = toSignal(this.store.select(selectTrendingLoading), {
    initialValue: false,
  });

  categories = ['All Items', 'Tops', 'Bottoms', 'Shoes', 'Accessories'];
  selectedCategory = signal('All Items');

  // Computed filtered items based on selected category
  filteredItems = computed(() => {
    const items = this.items();
    const category = this.selectedCategory();

    if (category === 'All Items') {
      return items;
    }

    // Category mapping from backend type to display name
    const categoryMap: { [key: string]: string } = {
      footwear: 'Shoes',
      top: 'Tops',
      tops: 'Tops',
      bottom: 'Bottoms',
      bottoms: 'Bottoms',
      dress: 'Dresses',
      dresses: 'Dresses',
      outerwear: 'Outerwear',
      shoes: 'Shoes',
      accessory: 'Accessories',
      accessories: 'Accessories',
    };

    return items.filter((item) => {
      const itemType = item.type?.toLowerCase() || '';
      const mappedCategory = categoryMap[itemType] || itemType;
      return mappedCategory === category || (category === 'Shoes' && itemType === 'footwear');
    });
  });

  ngOnInit() {
    this.store.dispatch(WardrobeActions.loadClothingItems());
    
    // Load weather with geolocation
    this.loadWeather();
    
    // Load today's pick with geolocation
    this.loadTodaysPick();
    
    // Load trending outfits - only 3 for home page
    this.store.dispatch(TrendingActions.loadTrending({ pageSize: 3 }));

  }

  private loadWeather(): void {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          this.store.dispatch(
            WeatherActions.loadCurrentWeather({
              lat: position.coords.latitude,
              lon: position.coords.longitude,
            })
          );
        },
        (error) => {
          console.log('Geolocation denied or unavailable, using default location');
          this.store.dispatch(WeatherActions.loadCurrentWeather({ city: 'Cairo' }));
        }
      );
    } else {
      this.store.dispatch(WeatherActions.loadCurrentWeather({ city: 'Cairo' }));
    }
  }

  private loadTodaysPick(): void {
    // Try to get user's location for weather-based recommendations
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          this.store.dispatch(
            OutfitsActions.loadTodaysPick({
              latitude: position.coords.latitude,
              longitude: position.coords.longitude,
            })
          );
        },
        (error) => {
          // Fallback to default location (Cairo) if geolocation fails
          console.log('Geolocation denied or unavailable, using default location');
          this.store.dispatch(OutfitsActions.loadTodaysPick({}));
        }
      );
    } else {
      // Geolocation not supported, use default
      this.store.dispatch(OutfitsActions.loadTodaysPick({}));
    }
  }

  selectCategory(category: string) {
    this.selectedCategory.set(category);
  }

  onImageError(outfitId: string): void {
    this.failedImages.add(outfitId);
  }
}
