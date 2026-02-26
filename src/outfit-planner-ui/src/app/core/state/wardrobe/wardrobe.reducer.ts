import { createFeature, createReducer, on } from '@ngrx/store';
import { WardrobeActions } from './wardrobe.actions';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';

export interface WardrobeState {
  items: ClothingItem[];
  selectedItem: ClothingItem | null;
  loading: boolean;
  error: string | null;
  filter: { category: string | null };
}

export const initialState: WardrobeState = {
  items: [],
  selectedItem: null,
  loading: false,
  error: null,
  filter: { category: null },
};

export const wardrobeFeature = createFeature({
  name: 'wardrobe',
  reducer: createReducer(
    initialState,
    on(WardrobeActions.loadClothingItems, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(WardrobeActions.loadClothingItemsSuccess, (state, { items }) => ({
      ...state,
      items: items || [],
      loading: false,
    })),
    on(WardrobeActions.loadClothingItemsFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),
    on(WardrobeActions.loadClothingItemByIdSuccess, (state, { item }) => ({
      ...state,
      selectedItem: item,
      loading: false,
    })),
    on(WardrobeActions.loadClothingItemsByCategorySuccess, (state, { items }) => ({
      ...state,
      items: items || [],
      loading: false,
    })),
    on(WardrobeActions.createClothingItemSuccess, (state, { item }) => ({
      ...state,
      items: [...state.items, item],
      loading: false,
    })),
    on(WardrobeActions.updateClothingItemSuccess, (state, { item }) => ({
      ...state,
      items: state.items.map((i) => (i.id === item.id ? item : i)),
      loading: false,
    })),
    on(WardrobeActions.deleteClothingItemSuccess, (state, { id }) => ({
      ...state,
      items: state.items.filter((i) => i.id !== id),
      loading: false,
    })),
    on(WardrobeActions.recordWearSuccess, (state, { item }) => ({
      ...state,
      items: state.items.map((i) => (i.id === item.id ? item : i)),
      loading: false,
    })),
  ),
});

export const {
  name,
  reducer,
  selectWardrobeState,
  selectItems,
  selectSelectedItem,
  selectLoading,
  selectError,
  selectFilter,
} = wardrobeFeature;
