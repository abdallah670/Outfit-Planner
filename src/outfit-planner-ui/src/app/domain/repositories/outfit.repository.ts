import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { Outfit } from '../entities/outfit.entity';
import { PagedResult } from '../entities/response.entity';

export const OUTFIT_REPOSITORY = new InjectionToken<OutfitRepository>('OutfitRepository');

export interface OutfitSuggestionsRequest {
  occasion?: string;
  season?: string;
  weatherCondition?: string;
  maxSuggestions?: number;
}

export interface TodaysPickResponse {
  outfit: Outfit;
  weatherContext: {
    condition: string;
    temperature: number;
    season: string;
    city: string;
  } | null;
  todayEvent: {
    title: string;
    eventType: string;
    eventDate: string;
  } | null;
  matchScore: number;
  recommendationReason: string;
  isBestEffort: boolean;
}

export interface OutfitRepository {
  getAllOutfits(): Observable<Outfit[]>;
  getOutfitById(id: string): Observable<Outfit>;
  createOutfit(outfit: Partial<Outfit>): Observable<Outfit>;
  updateOutfit(id: string, outfit: Partial<Outfit>): Observable<Outfit>;
  deleteOutfit(id: string): Observable<boolean>;
  getOutfitsSuggestions(request: OutfitSuggestionsRequest): Observable<Outfit[]>;
  getTodaysPick(latitude?: number, longitude?: number): Observable<TodaysPickResponse>;
  recordOutfitWear(id: string): Observable<Outfit>;
  getFilteredOutfits(
    filters: { occasion?: string; season?: string; search?: string; sortBy?: string },
    page: number,
    pageSize: number
  ): Observable<PagedResult<Outfit>>;
}
