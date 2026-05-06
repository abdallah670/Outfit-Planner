import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { TrendingOutfit } from '../entities/outfit.entity';

export const TRENDING_REPOSITORY = new InjectionToken<TrendingRepository>('TrendingRepository');

export interface TrendingRepository {
  getTrendingOutfits(page?: number, pageSize?: number): Observable<{ items: TrendingOutfit[]; totalCount: number }>;
}
