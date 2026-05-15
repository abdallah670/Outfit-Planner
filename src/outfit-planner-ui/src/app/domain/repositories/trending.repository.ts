import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { TrendingOutfit } from '../entities/outfit.entity';
import { CursorPagedResult } from '../entities/response.entity';


export const TRENDING_REPOSITORY = new InjectionToken<TrendingRepository>('TrendingRepository');

export interface TrendingRepository {
  getTrendingOutfits(cursor?: string, pageSize?: number): Observable<CursorPagedResult<TrendingOutfit>>;
}

