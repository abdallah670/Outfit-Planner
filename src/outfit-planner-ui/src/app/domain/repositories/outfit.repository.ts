import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { Outfit } from '../entities/outfit.entity';

export const OUTFIT_REPOSITORY = new InjectionToken<OutfitRepository>('OutfitRepository');

export interface OutfitSuggestionsRequest {
  occasion?: string;
  season?: string;
  weatherCondition?: string;
  maxSuggestions?: number;
}

export interface OutfitRepository {
  getAllOutfits(): Observable<Outfit[]>;
  getOutfitById(id: string): Observable<Outfit>;
  createOutfit(outfit: Partial<Outfit>): Observable<Outfit>;
  updateOutfit(id: string, outfit: Partial<Outfit>): Observable<Outfit>;
  deleteOutfit(id: string): Observable<boolean>;
  getOutfitsSuggestions(request: OutfitSuggestionsRequest): Observable<Outfit[]>;
  getTodaysOutfit(): Observable<Outfit>;
  recordOutfitWear(id: string): Observable<Outfit>;
}
