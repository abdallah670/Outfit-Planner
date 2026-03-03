import { Inject, Injectable } from '@angular/core';
import {
  OutfitRepository,
  OUTFIT_REPOSITORY,
  OutfitSuggestionsRequest,
} from '../repositories/outfit.repository';
import { Observable } from 'rxjs';
import { Outfit } from '../entities/outfit.entity';

@Injectable({
  providedIn: 'root',
})
export class OutfitsUseCases {
  constructor(@Inject(OUTFIT_REPOSITORY) private readonly outfitRepository: OutfitRepository) {}

  getAllOutfits(): Observable<Outfit[]> {
    return this.outfitRepository.getAllOutfits();
  }

  getOutfitById(id: string): Observable<Outfit> {
    return this.outfitRepository.getOutfitById(id);
  }

  createOutfit(outfit: Partial<Outfit>): Observable<Outfit> {
    return this.outfitRepository.createOutfit(outfit);
  }

  updateOutfit(id: string, outfit: Partial<Outfit>): Observable<Outfit> {
    return this.outfitRepository.updateOutfit(id, outfit);
  }

  deleteOutfit(id: string): Observable<boolean> {
    return this.outfitRepository.deleteOutfit(id);
  }

  getOutfitsSuggestions(request: OutfitSuggestionsRequest): Observable<Outfit[]> {
    return this.outfitRepository.getOutfitsSuggestions(request);
  }

  getTodaysOutfit(): Observable<Outfit> {
    return this.outfitRepository.getTodaysOutfit();
  }

  recordOutfitWear(id: string): Observable<Outfit> {
    return this.outfitRepository.recordOutfitWear(id);
  }
}
