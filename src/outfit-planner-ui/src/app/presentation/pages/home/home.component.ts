import { Component, inject, OnInit, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';

import { NavbarComponent } from '../../components/shared/navbar/navbar.component';
import { WeatherWidgetComponent } from '../../components/shared/weather-widget/weather-widget.component';
import { DailyPickComponent } from '../../components/shared/daily-pick/daily-pick.component';
import { WardrobeHealthComponent } from '../../components/shared/wardrobe-health/wardrobe-health.component';
import { ClothingCardComponent } from '../../components/clothing-card/clothing-card.component';
import { WardrobeActions } from '../../../core/state/wardrobe/wardrobe.actions';
import { selectAllItems } from '../../../core/state/wardrobe/wardrobe.selectors';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    NavbarComponent,
    WeatherWidgetComponent,
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

  categories = ['All Items', 'Tops', 'Bottoms', 'Shoes', 'Accessories'];
  selectedCategory = 'All Items';

  ngOnInit() {
    this.store.dispatch(WardrobeActions.loadClothingItems());
  }

  selectCategory(category: string) {
    this.selectedCategory = category;
    // Implementation for filtering could be added here or in selectors
  }
}
