import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { TrendingRepository, TRENDING_REPOSITORY } from '../../domain/repositories/trending.repository';
import { TrendingDataSource } from '../../data/datasources/trending.datasource';
import { TrendingOutfit } from '../../domain/entities/outfit.entity';

@Injectable({
  providedIn: 'root',
})
export class TrendingRepositoryImpl implements TrendingRepository {
  private trendingDataSource = inject(TrendingDataSource);

  getTrendingOutfits(page?: number, pageSize?: number): Observable<{ items: TrendingOutfit[]; totalCount: number }> {
    return this.trendingDataSource.getTrendingOutfits(page, pageSize);
  }
}

export const trendingRepositoryProvider = {
  provide: TRENDING_REPOSITORY,
  useClass: TrendingRepositoryImpl,
};
