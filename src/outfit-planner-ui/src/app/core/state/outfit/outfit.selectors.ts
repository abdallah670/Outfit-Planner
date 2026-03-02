import { createSelector, createFeatureSelector } from '@ngrx/store';
import { WardrobeState } from './wardrobe.reducer';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';

export interface WardrobeStats {
  totalItems: number;
  totalCost: number;
}

export const selectWardrobeState = createFeatureSelector<WardrobeState>('wardrobe');

export const selectAllItems = createSelector(
  selectWardrobeState,
  (state: WardrobeState): ClothingItem[] => state?.items || [],
);

export const selectWardrobeLoading = createSelector(
  selectWardrobeState,
  (state: WardrobeState): boolean => !!state?.loading,
);

export const selectWardrobeError = createSelector(
  selectWardrobeState,
  (state: WardrobeState): string | null => state?.error,
);

export const selectSelectedItem = createSelector(
  selectWardrobeState,
  (state: WardrobeState): ClothingItem | null => state?.selectedItem || null,
);

export const selectWardrobeStats = createSelector(
  selectAllItems,
  (items: ClothingItem[]): WardrobeStats => {
    return {
      totalItems: items.length || 0,
      totalCost: items.reduce((acc, curr) => acc + (Number(curr.purchasePrice) || 0), 0),
    };
  },
);
