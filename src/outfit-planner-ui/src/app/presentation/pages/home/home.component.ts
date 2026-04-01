import { Component, inject, OnInit, Signal } from '@angular/core';
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
import { SocialActions } from '../../../core/state/social/social.actions';
import { selectTrendingOutfits, selectSocialLoading } from '../../../core/state/social/social.selectors';
import { TrendingOutfit } from '../../../domain/entities/social-engagement.entity';

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

  trendingLoading: Signal<boolean> = toSignal(this.store.select(selectSocialLoading), {
    initialValue: false,
  });

  categories = ['All Items', 'Tops', 'Bottoms', 'Shoes', 'Accessories'];
  selectedCategory = 'All Items';

  ngOnInit() {
    this.store.dispatch(WardrobeActions.loadClothingItems());
    // Load weather for Cairo (default for now)
    this.store.dispatch(WeatherActions.loadCurrentWeather({ city: 'Cairo' }));
    
    // Load today's pick with geolocation
    this.loadTodaysPick();
    
    // Load trending outfits - only 3 for home page
    this.store.dispatch(SocialActions.loadTrending({ page: 1, pageSize: 3 }));
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
    this.selectedCategory = category;
    // Note: Filtering logic using a selector or signal would go here
  }
}
