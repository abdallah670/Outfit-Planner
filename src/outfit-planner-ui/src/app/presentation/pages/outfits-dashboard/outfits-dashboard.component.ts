import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';

import { OutfitsActions } from '../../../core/state/outfit/outfit.actions';
import { selectAllOutfits, selectOutfitLoading } from '../../../core/state/outfit/outfit.selectors';
import { OutfitState } from '../../../core/state/outfit/outfit.reducer';
import { OutfitCardComponent } from '../../components/outfits/outfit-card/outfit-card.component';
import { Outfit } from '../../../domain/entities/outfit.entity';
import { OccasionType, Season } from '../../../domain/entities/outfit.entity';

@Component({
  selector: 'app-outfits-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatChipsModule,
    FormsModule,
    OutfitCardComponent,
  ],
  templateUrl: './outfits-dashboard.component.html',
  styleUrl: './outfits-dashboard.component.scss',
})
export class OutfitsDashboardComponent implements OnInit {
  private store = inject(Store<{ outfit: OutfitState }>);

  outfits$: Observable<Outfit[]> = this.store.select(selectAllOutfits);
  loading$: Observable<boolean> = this.store.select(selectOutfitLoading);

  selectedOccasion: string = '';
  selectedSeason: string = '';

  occasions: string[] = Object.values(OccasionType);
  seasons: string[] = Object.values(Season);

  ngOnInit() {
    this.store.dispatch(OutfitsActions.loadOutfits());
  }

  trackByOutfitId(index: number, outfit: any): string {
    return outfit.id;
  }
}
