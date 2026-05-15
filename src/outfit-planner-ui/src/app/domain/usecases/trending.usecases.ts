import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { TRENDING_REPOSITORY, TrendingRepository } from '../repositories/trending.repository';
import { TrendingOutfit } from '../entities/outfit.entity';

import { CursorPagedResult } from '../entities/response.entity';

@Injectable({
  providedIn: 'root',
})
export class TrendingUseCases {
  constructor(
    @Inject(TRENDING_REPOSITORY) private readonly trendingRepository: TrendingRepository,
  ) {}

  getTrendingOutfits(cursor?: string, pageSize = 20): Observable<CursorPagedResult<TrendingOutfit>> {
    return this.trendingRepository.getTrendingOutfits(cursor, pageSize);
  }
}

