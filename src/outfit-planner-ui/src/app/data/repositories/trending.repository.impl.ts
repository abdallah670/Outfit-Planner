import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { TrendingRepository, TRENDING_REPOSITORY } from '../../domain/repositories/trending.repository';
import { TrendingDataSource } from '../../data/datasources/trending.datasource';
import { TrendingOutfit } from '../../domain/entities/outfit.entity';

import { CursorPagedResult } from '../../domain/entities/response.entity';

@Injectable({
  providedIn: 'root',
})
export class TrendingRepositoryImpl implements TrendingRepository {
  private trendingDataSource = inject(TrendingDataSource);

  getTrendingOutfits(cursor?: string, pageSize?: number): Observable<CursorPagedResult<TrendingOutfit>> {
    return this.trendingDataSource.getTrendingOutfits(cursor, pageSize);
  }
}


export const trendingRepositoryProvider = {
  provide: TRENDING_REPOSITORY,
  useClass: TrendingRepositoryImpl,
};
