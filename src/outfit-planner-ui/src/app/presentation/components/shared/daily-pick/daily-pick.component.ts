import { Component, inject, OnInit, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { OutfitsActions } from '../../../../core/state/outfit/outfit.actions';
import {
  selectTodaysOutfit,
  selectTodaysPickContext,
  selectTodaysPickLoading,
  selectTodaysPickError,
} from '../../../../core/state/outfit/outfit.selectors';
import { Outfit } from '../../../../domain/entities/outfit.entity';

@Component({
  selector: 'app-daily-pick',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './daily-pick.component.html',
  styleUrls: ['./daily-pick.component.scss'],
})
export class DailyPickComponent implements OnInit {
  private store = inject(Store);
  private router = inject(Router);

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

  ngOnInit(): void {
    // Load today's pick - location will be handled by the effect/default
    this.store.dispatch(OutfitsActions.loadTodaysPick({}));
  }

  onWearToday(): void {
    const outfit = this.outfit();
    if (outfit) {
      this.store.dispatch(OutfitsActions.recordOutfitWear({ id: outfit.id }));
    }
  }

  onViewOutfit(): void {
    const outfit = this.outfit();
    if (outfit) {
      this.router.navigate(['/outfits', outfit.id]);
    }
  }

  getMatchPercentage(): string {
    const score = this.context()?.matchScore;
    if (score === undefined || score === null) return '0';
    return Math.round(score).toString();
  }

  getRecommendationReason(): string {
    return this.context()?.recommendationReason || 'Based on your wardrobe';
  }

  getPrimaryItems() {
    const outfit = this.outfit();
    if (!outfit?.items) return [];
    return outfit.items
      .filter(item => item.role === 'primary' || item.isEssential)
      .slice(0, 3);
  }
}
