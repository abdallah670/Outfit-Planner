import { createSelector, createFeatureSelector } from '@ngrx/store';
import { OutfitState } from './outfit.reducer';
import { Outfit } from '../../../domain/entities/outfit.entity';

export interface OutfitStats {
  totalOutfits: number;
  totalCost: number;
}

export interface TodaysPickContext {
  outfit: Outfit | null;
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
  loading: boolean;
  error: string | null;
}

export const selectOutfitState = createFeatureSelector<OutfitState>('outfit');

export const selectAllOutfits = createSelector(
  selectOutfitState,
  (state: OutfitState): Outfit[] => state?.outfits || [],
);

export const selectOutfitLoading = createSelector(
  selectOutfitState,
  (state: OutfitState): boolean => !!state?.loading,
);

export const selectOutfitError = createSelector(
  selectOutfitState,
  (state: OutfitState): string | null => state?.error,
);

export const selectSelectedItem = createSelector(
  selectOutfitState,
  (state: OutfitState): Outfit | null => state?.selectedItem || null,
);

export const selectTodaysOutfit = createSelector(
  selectOutfitState,
  (state: OutfitState): Outfit | null => state?.todaysOutfit || null,
);

export const selectTodaysPickLoading = createSelector(
  selectOutfitState,
  (state: OutfitState): boolean => state?.todaysOutfitLoading || false,
);

export const selectTodaysPickError = createSelector(
  selectOutfitState,
  (state: OutfitState): string | null => state?.todaysOutfitError || null,
);

export const selectTodaysPickContext = createSelector(
  selectOutfitState,
  (state: OutfitState) => state?.todaysPickContext || null,
);

export const selectOutfitStats = createSelector(
  selectAllOutfits,
  (outfits: Outfit[]): OutfitStats => {
    return {
      totalOutfits: outfits.length || 0,
      totalCost: 0,
    };
  },
);
