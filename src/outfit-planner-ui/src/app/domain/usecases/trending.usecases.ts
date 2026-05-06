import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { TRENDING_REPOSITORY, TrendingRepository } from '../repositories/trending.repository';
import { TrendingOutfit } from '../entities/outfit.entity';

@Injectable({
  providedIn: 'root',
})
export class TrendingUseCases {
  constructor(
    @Inject(TRENDING_REPOSITORY) private readonly trendingRepository: TrendingRepository,
  ) {}

  getTrendingOutfits(page = 1, pageSize = 20): Observable<{ items: TrendingOutfit[]; totalCount: number }> {
    return this.trendingRepository.getTrendingOutfits(page, pageSize);
  }
}
