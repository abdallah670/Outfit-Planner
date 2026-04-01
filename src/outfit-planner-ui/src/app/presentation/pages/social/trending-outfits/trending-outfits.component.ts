import { Component, OnInit, inject, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';
import { SocialActions } from '../../../../core/state/social/social.actions';
import { selectTrendingOutfits, selectSocialLoading } from '../../../../core/state/social/social.selectors';
import { TrendingOutfit } from '../../../../domain/entities/social-engagement.entity';

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
  loading = toSignal(this.store.select(selectSocialLoading), { initialValue: false });

  ngOnInit(): void {
    this.store.dispatch(SocialActions.loadTrending());
  }

  react(outfit: TrendingOutfit, reactionType: string = 'Like'): void {
    this.store.dispatch(SocialActions.reactToVote({ voteId: outfit.voteId, reactionType }));
  }

  viewDetails(outfit: TrendingOutfit): void {
    this.router.navigate(['/outfits', outfit.id]);
  }
}
