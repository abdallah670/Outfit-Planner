import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { OutfitsActions } from '../../../core/state/outfit/outfit.actions';
import { selectAllOutfits, selectOutfitLoading } from '../../../core/state/outfit/outfit.selectors';
import { OutfitState } from '../../../core/state/outfit/outfit.reducer';
import { Outfit } from '../../../domain/entities/outfit.entity';

interface TodayEvent {
  time: string;
  name: string;
}

interface WeatherData {
  temp: number;
  condition: string;
  feelsLike: number;
  humidity: number;
}

interface AlternativeOutfit {
  id: string;
  name: string;
  description: string;
  matchPercentage: number;
  imageUrl: string;
}

interface CurrentOutfit {
  id: string;
  name: string;
  matchPercentage: number;
}

@Component({
  selector: 'app-daily-suggestion',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './daily-suggestion.component.html',
  styleUrl: './daily-suggestion.component.scss',
})
export class DailySuggestionComponent implements OnInit {
  private store = inject(Store<{ outfit: OutfitState }>);

  today = new Date();
  
  // Current outfit signal
  currentOutfit = signal<CurrentOutfit>({
    id: '1',
    name: 'Weekend Casual',
    matchPercentage: 94
  });
  
  // Weather signal
  weather = signal<WeatherData>({
    temp: 22,
    condition: 'Partly Cloudy',
    feelsLike: 24,
    humidity: 45
  });
  
  // Today's events
  todayEvents = signal<TodayEvent[]>([
    { time: '2:00 PM', name: 'Team Meeting' },
    { time: '7:00 PM', name: 'Dinner Date' }
  ]);
  
  // Alternatives
  alternatives = signal<AlternativeOutfit[]>([
    {
      id: '2',
      name: 'Business Casual',
      description: 'More formal option',
      matchPercentage: 88,
      imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/1eeab938-2762-4564-adfe-febac50e0beb.jpg'
    },
    {
      id: '3',
      name: 'Date Night Focus',
      description: 'Evening emphasis',
      matchPercentage: 85,
      imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/cd38abb9-7941-4e39-b78f-6025eb9c0b59.jpg'
    },
    {
      id: '4',
      name: 'Relaxed Weekend',
      description: 'Maximum comfort',
      matchPercentage: 82,
      imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/85e9add7-a591-43fc-b2b2-270f0d70db7e.jpg'
    },
    {
      id: '5',
      name: 'Smart Minimal',
      description: 'Clean & simple',
      matchPercentage: 78,
      imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/816794be-8274-4f45-ad19-f3ff94c73f10.jpg'
    }
  ]);

  ngOnInit() {
    // Load outfits
    this.store.dispatch(OutfitsActions.loadOutfits());
  }

  wearToday(): void {
    const outfit = this.currentOutfit();
    // Dispatch recordOutfitWear action
    this.store.dispatch(OutfitsActions.recordOutfitWear({ id: outfit.id }));
    console.log('Wearing outfit:', outfit.name);
  }

  showAlternatives(): void {
    // Scroll to alternatives section
    const element = document.querySelector('.alternatives-section');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  }

  selectAlternative(alt: AlternativeOutfit): void {
    this.currentOutfit.set({
      id: alt.id,
      name: alt.name,
      matchPercentage: alt.matchPercentage
    });
  }

  previewTomorrow(): void {
    console.log('Preview tomorrow suggestion');
  }
}
