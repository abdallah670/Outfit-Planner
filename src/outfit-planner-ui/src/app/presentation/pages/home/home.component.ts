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

  categories = ['All Items', 'Tops', 'Bottoms', 'Shoes', 'Accessories'];
  selectedCategory = 'All Items';

  ngOnInit() {
    this.store.dispatch(WardrobeActions.loadClothingItems());
    // Load weather for Cairo (default for now)
    this.store.dispatch(WeatherActions.loadCurrentWeather({ city: 'Cairo' }));
  }

  selectCategory(category: string) {
    this.selectedCategory = category;
    // Note: Filtering logic using a selector or signal would go here
  }
}
