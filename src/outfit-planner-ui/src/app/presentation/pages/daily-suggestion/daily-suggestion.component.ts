import { Component, OnInit, signal, inject, Signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { toSignal } from '@angular/core/rxjs-interop';

import { OutfitsActions } from '../../../core/state/outfit/outfit.actions';
import {
  selectTodaysOutfit,
  selectTodaysPickContext,
  selectTodaysPickLoading,
  selectTodaysPickError,
  selectSuggestions,
} from '../../../core/state/outfit/outfit.selectors';
import { OutfitState } from '../../../core/state/outfit/outfit.reducer';
import { Outfit, OutfitItem } from '../../../domain/entities/outfit.entity';
import { WeatherService, WeatherData as WeatherServiceData } from '../../../core/services/weather.service';
import { CalendarActions } from '../../../core/state/calendar/calendar.actions';
import {
  selectCalendarEvents,
  selectWeatherData,
} from '../../../core/state/calendar/calendar.selectors';
import { CalendarState } from '../../../core/state/calendar/calendar.reducer';
import { CalendarEventItem, WeatherData as CalendarWeatherData } from '../../../domain/entities/wear-event.entity';

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
  imageUrl?: string;
  items?: OutfitItem[];
  occasion?: string;
  season?: string;
  timesWorn?: number;
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
  store = inject(Store<{ outfit: OutfitState; calendar: CalendarState }>);
  OutfitsActions = OutfitsActions;
  weatherService = inject(WeatherService);

  constructor() {
    // Re-dispatch suggestions with weather context once it's available
    effect(() => {
      const ctx = this.context();
      if (ctx?.weatherContext) {
        this.store.dispatch(OutfitsActions.generateSuggestions({
          request: {
            season: ctx.weatherContext.season,
            weatherCondition: ctx.weatherContext.condition,
            occasion: ctx.todayEvent?.eventType,
            maxSuggestions: 5,
          }
        }));
      }
    });
  }

  // Selected date for navigation (defaults to today)
  selectedDate = signal<Date>(new Date());

  // Signals for today's pick data from store
  outfit: Signal<Outfit | null> = toSignal(this.store.select(selectTodaysOutfit), {
    initialValue: null,
  });

  context: Signal<{
    weatherContext: {
      condition: string;
      temperature: number;
      season: string;
      city: string;
    } | null;
    todayEvent: {
      title: string;
      eventType: string;
      eventDate: string;
    } | null;
    matchScore: number;
    recommendationReason: string;
    isBestEffort: boolean;
  } | null> = toSignal(this.store.select(selectTodaysPickContext), {
    initialValue: null,
  });

  loading: Signal<boolean> = toSignal(this.store.select(selectTodaysPickLoading), {
    initialValue: false,
  });

  error: Signal<string | null> = toSignal(this.store.select(selectTodaysPickError), {
    initialValue: null,
  });

  // Computed current outfit - derived from store data
  currentOutfit = signal<CurrentOutfit | null>(null);

  // Weather data from backend
  weather = signal<WeatherData>({
    temp: 22,
    condition: 'Partly Cloudy',
    feelsLike: 24,
    humidity: 45
  });

  // Calendar events from backend
  calendarEvents: Signal<CalendarEventItem[]> = toSignal(
    this.store.select(selectCalendarEvents),
    { initialValue: [] }
  );

  // Weather data from calendar state
  weatherForecast: Signal<Map<string, CalendarWeatherData> | null> = toSignal(
    this.store.select(selectWeatherData),
    { initialValue: null }
  );

  // Today's events from calendar
  todayEvents = signal<TodayEvent[]>([]);

  // Alternative suggestions from store
  suggestions: Signal<Outfit[]> = toSignal(this.store.select(selectSuggestions), {
    initialValue: [],
  });

  // Alternative suggestions - populated from store suggestions
  alternatives = signal<AlternativeOutfit[]>([]);

  ngOnInit() {
    this.loadDateData(this.selectedDate());
  }

  public loadDateData(date: Date): void {
    const year = date.getFullYear();
    const month = date.getMonth() + 1;
    const dateStr = date.toISOString().split('T')[0];

    // Load outfit pick for the selected date
    this.store.dispatch(OutfitsActions.loadTodaysPick({ date: dateStr }));

    // Load calendar events for the month
    this.store.dispatch(CalendarActions.loadCalendarEvents({ year, month }));

    // Load weather forecast
    this.store.dispatch(CalendarActions.loadWeatherForecast({ year, month }));

    // Fetch weather for the specific date
    this.weatherService.getWeatherForDate(date).subscribe({
      next: (data) => {
        if (data) {
          this.weather.set({
            temp: data.temperature,
            condition: data.condition,
            feelsLike: data.temperature + 2,
            humidity: data.humidity || 45
          });
        }
      },
      error: (err) => console.error('Failed to load weather:', err)
    });

    this.updateFromStore();
  }

  goToPrevDay(): void {
    const current = this.selectedDate();
    const prev = new Date(current);
    prev.setDate(prev.getDate() - 1);
    this.selectedDate.set(prev);
    this.loadDateData(prev);
  }

  goToNextDay(): void {
    const current = this.selectedDate();
    const next = new Date(current);
    next.setDate(next.getDate() + 1);
    this.selectedDate.set(next);
    this.loadDateData(next);
  }

  goToToday(): void {
    this.selectedDate.set(new Date());
    this.loadDateData(new Date());
  }

  isToday(): boolean {
    const selected = this.selectedDate();
    const now = new Date();
    return selected.toDateString() === now.toDateString();
  }

  getFormattedDate(): string {
    return this.selectedDate().toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  getDayLabel(): string {
    const selected = new Date(this.selectedDate());
    const now = new Date();
    selected.setHours(0, 0, 0, 0);
    now.setHours(0, 0, 0, 0);
    const diff = Math.round((selected.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
    if (diff === 0) return 'Today';
    if (diff === 1) return 'Tomorrow';
    if (diff === -1) return 'Yesterday';
    if (diff > 1) return `In ${diff} days`;
    return `${Math.abs(diff)} days ago`;
  }

  private updateFromStore(): void {
    const outfit = this.outfit();
    const ctx = this.context();

    if (outfit) {
      this.currentOutfit.set({
        id: outfit.id,
        name: outfit.name,
        matchPercentage: Math.round(ctx?.matchScore || 0),
        imageUrl: outfit.imageUrl,
        items: outfit.items,
        occasion: outfit.occasion,
        season: outfit.season,
        timesWorn: outfit.timesWorn
      });

      // Update weather from pick context if available
      if (ctx?.weatherContext) {
        this.weather.set({
          temp: ctx.weatherContext.temperature,
          condition: ctx.weatherContext.condition,
          feelsLike: ctx.weatherContext.temperature + 2,
          humidity: 45
        });
      }
    }

    // Update events from calendar state (from backend)
    const selectedDateStr = this.selectedDate().toISOString().split('T')[0];
    const calendarEvents = this.calendarEvents();

    // Find events for selected date
    const dateCalendarEvents = calendarEvents.filter(e => {
      const eventDate = new Date(e.eventDate).toISOString().split('T')[0];
      return eventDate === selectedDateStr;
    });

    if (dateCalendarEvents.length > 0) {
      this.todayEvents.set(dateCalendarEvents.map(e => ({
        time: e.startTime || new Date(e.eventDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        name: e.title
      })));
    } else if (ctx?.todayEvent) {
      // Fallback to context event if no calendar events
      const eventDate = new Date(ctx.todayEvent.eventDate);
      this.todayEvents.set([{
        time: eventDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        name: ctx.todayEvent.title
      }]);
    } else {
      this.todayEvents.set([]);
    }

    // Update weather from calendar state if available
    const weatherForecast = this.weatherForecast();
    if (weatherForecast && weatherForecast.has(selectedDateStr)) {
      const forecast = weatherForecast.get(selectedDateStr);
      if (forecast) {
        this.weather.set({
          temp: forecast.temperature,
          condition: forecast.condition,
          feelsLike: forecast.temperature + 2, // Approximate
          humidity: 45
        });
      }
    }

    // Update alternatives from suggestions (exclude current pick)
    const currentOutfitId = outfit?.id;
    const suggestionOutfits = this.suggestions()
      .filter(s => s.id !== currentOutfitId)
      .slice(0, 5);

    if (suggestionOutfits.length > 0) {
      this.alternatives.set(suggestionOutfits.map(s => ({
        id: s.id,
        name: s.name,
        description: s.occasion || 'Alternative option',
        matchPercentage: Math.round(ctx?.matchScore ? ctx.matchScore * 0.85 : 75),
        imageUrl: s.imageUrl || 'assets/placeholder.jpg'
      })));
    }
  }

  wearToday(): void {
    const outfit = this.currentOutfit();
    if (outfit) {
      this.store.dispatch(OutfitsActions.recordOutfitWear({ id: outfit.id }));
    }
  }

  showAlternatives(): void {
    const element = document.querySelector('.alternatives-section');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  }

  selectAlternative(alt: AlternativeOutfit): void {
    this.currentOutfit.set({
      id: alt.id,
      name: alt.name,
      matchPercentage: alt.matchPercentage,
      imageUrl: alt.imageUrl
    });
  }

  getMatchPercentage(): string {
    const score = this.context()?.matchScore;
    if (score === undefined || score === null) return '0';
    return Math.round(score).toString();
  }

  getRecommendationReason(): string {
    return this.context()?.recommendationReason || 'Based on your wardrobe and today\'s schedule';
  }

  getPrimaryItems(): OutfitItem[] {
    const outfit = this.outfit();
    if (!outfit?.items) return [];
    return outfit.items
      .filter(item => item.role === 'primary' || item.isEssential)
      .slice(0, 4);
  }

  hasValidImage(item: OutfitItem): boolean {
    return !!item.clothingItemImageUrl && item.clothingItemImageUrl.trim().length > 0;
  }

  getItemReason(item: OutfitItem): string {
    const weather = this.weather();
    const context = this.context();
    
    // Generate reasons based on item type, weather, and occasion
    const reasons = [];
    
    // Weather-based reasons
    if (weather && weather.condition) {
      if (weather.condition.toLowerCase().includes('cold') || weather.temp < 15) {
        if (item.clothingItemName?.toLowerCase().includes('jacket') || 
            item.clothingItemName?.toLowerCase().includes('coat') ||
            item.clothingItemName?.toLowerCase().includes('sweater')) {
          reasons.push('Perfect for cold weather');
        }
      } else if (weather.condition.toLowerCase().includes('hot') || weather.temp > 25) {
        if (item.clothingItemName?.toLowerCase().includes('t-shirt') || 
            item.clothingItemName?.toLowerCase().includes('short') ||
            item.clothingItemName?.toLowerCase().includes('light')) {
          reasons.push('Great for hot weather');
        }
      }
    }
    
    // Role-based reasons
    if (item.role === 'primary') {
      reasons.push('Essential piece for this look');
    }
    
    if (item.isEssential) {
      reasons.push('Wardrobe staple');
    }
    
    // Category-based reasons
    if (item.clothingItemName?.toLowerCase().includes('jeans')) {
      reasons.push('Versatile and comfortable');
    } else if (item.clothingItemName?.toLowerCase().includes('dress')) {
      reasons.push('Elegant and stylish');
    } else if (item.clothingItemName?.toLowerCase().includes('shirt')) {
      reasons.push('Classic and professional');
    }
    
    // Default reason if none matched
    return reasons.length > 0 ? reasons.join(' • ') : 'Great choice for today';
  }

  previewTomorrow(): void {
    this.goToNextDay();
  }
}
