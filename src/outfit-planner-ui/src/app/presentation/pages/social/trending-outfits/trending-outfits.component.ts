import { Component, OnInit, input, signal, inject, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { TrendingOutfit } from '../../../../domain/entities/outfit.entity';
import { TrendingUseCases } from '../../../../domain/usecases/trending.usecases';
import { CursorPagedResult } from '../../../../domain/entities/response.entity';

@Component({
  selector: 'app-trending-outfits',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule],
  templateUrl: './trending-outfits.component.html',
  styleUrl: './trending-outfits.component.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class TrendingOutfitsComponent implements OnInit {
  private trendingUseCases = inject(TrendingUseCases);
  private router = inject(Router);

  // Allow parent component to pass pre-loaded outfits
  outfits = input<TrendingOutfit[] | null>(null);
  loading = input<boolean>(false);

  // Local signals for standalone mode (when no input provided)
  private localOutfits = signal<TrendingOutfit[]>([]);
  private localLoading = signal(false);
  private localCursor = signal<string | null>(null);
  private localHasMore = signal(false);

  get trendingOutfits(): TrendingOutfit[] {
    return this.outfits() ?? this.localOutfits();
  }

  get isLoading(): boolean {
    return this.loading() || this.localLoading();
  }

  get hasMore(): boolean {
    return this.localHasMore();
  }

  ngOnInit(): void {
   
      this.loadTrending();
    
  }

  private loadTrending(cursor?: string): void {
    this.localLoading.set(true);
    this.trendingUseCases.getTrendingOutfits(cursor, 10).subscribe({
      next: (result: CursorPagedResult<TrendingOutfit>) => {
        if (cursor) {
          // Append to existing items
          this.localOutfits.update(current => [...current, ...result.items]);
        } else {
          this.localOutfits.set(result.items);
        }
        this.localCursor.set(result.nextCursor);
        this.localHasMore.set(result.hasMore);
        this.localLoading.set(false);
      },
      error: () => this.localLoading.set(false)
    });
  }

  loadMore(): void {
    if (this.localLoading() || !this.localHasMore()) return;
    const cursor = this.localCursor();
    if (cursor) {
      this.loadTrending(cursor);
    }
  }

  viewDetails(outfit: TrendingOutfit): void {
    if (outfit.postType === 'Poll') {
      this.router.navigate(['/social/polls', outfit.feedPostId]);
    } else {
      // "Outfit" from backend, routes to OutfitPostDetailComponent
      this.router.navigate(['/social/posts', outfit.feedPostId]);
    } 
  }
}