import { createSelector, createFeatureSelector } from '@ngrx/store';
import { OutfitState } from './outfit.reducer';
import { Outfit } from '../../../domain/entities/outfit.entity';

export interface OutfitStats {
  totalOutfits: number;
  totalCost: number;
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

export const selectOutfitStats = createSelector(
  selectAllOutfits,
  (outfits: Outfit[]): OutfitStats => {
    return {
      totalOutfits: outfits.length || 0,
      totalCost: 0, // Note: Outfit doesn't have a direct cost property; would need clothing items data to calculate
    };
  },
);
