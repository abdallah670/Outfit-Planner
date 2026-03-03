import { Injectable } from '@angular/core';
import {
  OutfitRepository,
  OUTFIT_REPOSITORY,
  OutfitSuggestionsRequest,
} from '../../domain/repositories/outfit.repository';
import { OutfitDataSource } from '../datasources/outfit.datasource';
import { Observable } from 'rxjs';
import { Outfit } from '../../domain/entities/outfit.entity';

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

  deleteOutfit(id: string): Observable<boolean> {
    return this.outfitDataSource.deleteOutfit(id);
  }

  getOutfitsSuggestions(request: OutfitSuggestionsRequest): Observable<Outfit[]> {
    return this.outfitDataSource.getOutfitsSuggestions(request);
  }

  getTodaysOutfit(): Observable<Outfit> {
    return this.outfitDataSource.getTodaysOutfit();
  }

  recordOutfitWear(id: string): Observable<Outfit> {
    return this.outfitDataSource.recordOutfitWear(id);
  }
}

export const outfitRepositoryProvider = {
  provide: OUTFIT_REPOSITORY,
  useClass: OutfitRepositoryImpl,
};
