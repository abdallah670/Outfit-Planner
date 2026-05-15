import { Injectable } from '@angular/core';
import {
  OutfitRepository,
  OUTFIT_REPOSITORY,
  OutfitSuggestionsRequest,
  TodaysPickResponse,
} from '../../domain/repositories/outfit.repository';
import { OutfitDataSource } from '../datasources/outfit.datasource';
import { Observable } from 'rxjs';
import { Outfit } from '../../domain/entities/outfit.entity';
import { PagedResult } from '../../domain/entities/response.entity';

@Injectable({
  providedIn: 'root',
})
export class OutfitRepositoryImpl implements OutfitRepository {
  constructor(private readonly outfitDataSource: OutfitDataSource) {}

  getAllOutfits(): Observable<Outfit[]> {
    return this.outfitDataSource.getAllOutfits();
  }

  getOutfitById(id: string): Observable<Outfit> {
    return this.outfitDataSource.getOutfitById(id);
  }

  createOutfit(outfit: Partial<Outfit>): Observable<Outfit> {
    return this.outfitDataSource.createOutfit(outfit);
  }

  updateOutfit(id: string, outfit: Partial<Outfit>): Observable<Outfit> {
    return this.outfitDataSource.updateOutfit(id, outfit);
  }

  createOutfitWithImage(imageFile: File): Observable<Outfit> {
    return this.outfitDataSource.createOutfitWithImage(imageFile);
  }
  deleteOutfit(id: string): Observable<boolean> {
    return this.outfitDataSource.deleteOutfit(id);
  }

  getOutfitsSuggestions(request: OutfitSuggestionsRequest): Observable<Outfit[]> {
    return this.outfitDataSource.getOutfitsSuggestions(request);
  }

  getTodaysPick(latitude?: number, longitude?: number, date?: string): Observable<TodaysPickResponse> {
    return this.outfitDataSource.getTodaysPick(latitude, longitude, date);
  }

  recordOutfitWear(id: string): Observable<Outfit> {
    return this.outfitDataSource.recordOutfitWear(id);
  }

  getFilteredOutfits(
    filters: { occasion?: string; season?: string; search?: string; sortBy?: string },
    page: number,
    pageSize: number
  ): Observable<PagedResult<Outfit>> {
    return this.outfitDataSource.getFilteredOutfits(filters, page, pageSize);
  }
}

export const outfitRepositoryProvider = {
  provide: OUTFIT_REPOSITORY,
  useClass: OutfitRepositoryImpl,
};
