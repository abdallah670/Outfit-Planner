import { Component, OnInit, inject, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';
import { TrendingActions } from '../../../../core/state/trending/trending.actions';
import { selectTrendingOutfits, selectTrendingLoading } from '../../../../core/state/trending/trending.selectors';
import { TrendingOutfit } from '../../../../domain/entities/outfit.entity';

@Component({
  selector: 'app-trending-outfits',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule],
  templateUrl: './trending-outfits.component.html',
  styleUrl: './trending-outfits.component.scss'
})
export class TrendingOutfitsComponent implements OnInit {
  private store = inject(Store);
  private router = inject(Router);

  trendingOutfits: Signal<TrendingOutfit[]> = toSignal(
    this.store.select(selectTrendingOutfits), 
    { initialValue: [] as TrendingOutfit[] }
  );
  loading = toSignal(this.store.select(selectTrendingLoading), { initialValue: false });

  ngOnInit(): void {
    this.store.dispatch(TrendingActions.loadTrending({ page: 1, pageSize: 20 }));
  }

  react(outfit: TrendingOutfit, reactionType: string = 'Like'): void {
    // TODO: Implement reaction using FeedActions once TrendingOutfits are mapped to FeedPosts
    console.log('Reaction not yet implemented in new architecture', { outfitId: outfit.id, reactionType });
  }

  viewDetails(outfit: TrendingOutfit): void {
    this.router.navigate(['/outfits', outfit.id]);
  }
}
