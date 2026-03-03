import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable } from 'rxjs';

import { OutfitsActions } from '../../../core/state/outfit/outfit.actions';
import { selectAllOutfits, selectOutfitLoading } from '../../../core/state/outfit/outfit.selectors';
import { OutfitState } from '../../../core/state/outfit/outfit.reducer';
import { OutfitCardComponent } from '../../components/outfits/outfit-card/outfit-card.component';
import { Outfit } from '../../../domain/entities/outfit.entity';

@Component({
  selector: 'app-daily-suggestion',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    OutfitCardComponent,
  ],
  templateUrl: './daily-suggestion.component.html',
  styleUrl: './daily-suggestion.component.scss',
})
export class DailySuggestionComponent implements OnInit {
  private store = inject(Store<{ outfit: OutfitState }>);

  today = new Date();
  // For now, we'll use a subset of all outfits as suggestions if the backend doesn't provide them yet
  suggestions$: Observable<Outfit[]> = this.store.select(selectAllOutfits);
  loading$: Observable<boolean> = this.store.select(selectOutfitLoading);

  ngOnInit() {
    // In a real scenario, we'd dispatch generateSuggestions with actual weather/season data
    this.store.dispatch(OutfitsActions.loadOutfits());
  }

  onWearToday(outfitId: string) {
    this.store.dispatch(OutfitsActions.recordOutfitWear({ id: outfitId }));
  }

  onSaveToVault(outfit: Outfit) {
    // If it's a new suggestion not in vault, we'd save it.
    // For now, we'll just show success since suggestions are from existing outfits.
  }

  trackByOutfitId(index: number, outfit: any): string {
    return outfit.id;
  }
}
