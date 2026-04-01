import { createFeature, createReducer, on } from '@ngrx/store';
import { OutfitsActions } from './outfit.actions';
import { Outfit } from '../../../domain/entities/outfit.entity';

export interface OutfitState {
  outfits: Outfit[];
  selectedItem: Outfit | null;
  loading: boolean;
  error: string | null;
  filter: { category: string | null };
  suggestions: Outfit[];
  todaysOutfit: Outfit | null;
  todaysOutfitLoading: boolean;
  todaysOutfitError: string | null;
  todaysPickContext: {
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
  } | null;
}

export const initialState: OutfitState = {
  outfits: [],
  selectedItem: null,
  loading: false,
  error: null,
  filter: { category: null },
  suggestions: [],
  todaysOutfit: null,
  todaysOutfitLoading: false,
  todaysOutfitError: null,
  todaysPickContext: null,
};

export const outfitFeature = createFeature({
  name: 'outfit',
  reducer: createReducer(
    initialState,

    // Load All Outfits
    on(OutfitsActions.loadOutfits, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OutfitsActions.loadOutfitsSuccess, (state, { outfits }) => ({
      ...state,
      outfits: outfits || [],
      loading: false,
    })),
    on(OutfitsActions.loadOutfitsFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Outfit By Id
    on(OutfitsActions.loadOutfitById, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OutfitsActions.loadOutfitByIdSuccess, (state, { outfit }) => ({
      ...state,
      selectedItem: outfit,
      loading: false,
    })),
    on(OutfitsActions.loadOutfitByIdFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Outfits By Category
    on(OutfitsActions.loadOutfitsByCategory, (state, { category }) => ({
      ...state,
      loading: true,
      error: null,
      filter: { category },
    })),
    on(OutfitsActions.loadOutfitsByCategorySuccess, (state, { outfits }) => ({
      ...state,
      outfits: outfits || [],
      loading: false,
    })),
    on(OutfitsActions.loadOutfitsByCategoryFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Create Outfit
    on(OutfitsActions.createOutfit, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OutfitsActions.createOutfitSuccess, (state, { outfit }) => ({
      ...state,
      outfits: [...state.outfits, outfit],
      loading: false,
    })),
    on(OutfitsActions.createOutfitFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Update Outfit
    on(OutfitsActions.updateOutfit, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OutfitsActions.updateOutfitSuccess, (state, { outfit }) => ({
      ...state,
      outfits: state.outfits.map((i) => (i.id === outfit.id ? outfit : i)),
      selectedItem: state.selectedItem?.id === outfit.id ? outfit : state.selectedItem,
      loading: false,
    })),
    on(OutfitsActions.updateOutfitFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Delete Outfit
    on(OutfitsActions.deleteOutfit, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OutfitsActions.deleteOutfitSuccess, (state, { id }) => ({
      ...state,
      outfits: state.outfits.filter((i) => i.id !== id),
      selectedItem: state.selectedItem?.id === id ? null : state.selectedItem,
      loading: false,
    })),
    on(OutfitsActions.deleteOutfitFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Record Outfit Wear
    on(OutfitsActions.recordOutfitWear, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OutfitsActions.recordOutfitWearSuccess, (state, { outfit }) => ({
      ...state,
      outfits: state.outfits.map((i) => (i.id === outfit.id ? outfit : i)),
      selectedItem: state.selectedItem?.id === outfit.id ? outfit : state.selectedItem,
      loading: false,
    })),
    on(OutfitsActions.recordOutfitWearFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Generate Suggestions
    on(OutfitsActions.generateSuggestions, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OutfitsActions.generateSuggestionsSuccess, (state, { outfits }) => ({
      ...state,
      suggestions: outfits || [],
      loading: false,
    })),
    on(OutfitsActions.generateSuggestionsFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Todays Pick
    on(OutfitsActions.loadTodaysPick, (state) => ({
      ...state,
      todaysOutfitLoading: true,
      todaysOutfitError: null,
    })),
    on(OutfitsActions.loadTodaysPickSuccess, (state, { outfit, context }) => ({
      ...state,
      todaysOutfit: outfit,
      todaysPickContext: context,
      todaysOutfitLoading: false,
      todaysOutfitError: null,
    })),
    on(OutfitsActions.loadTodaysPickFailure, (state, { error }) => ({
      ...state,
      todaysOutfitLoading: false,
      todaysOutfitError: error,
    })),
  ),
});

export const {
  name,
  reducer,
  selectOutfitState,
  selectOutfits,
  selectSelectedItem,
  selectLoading,
  selectError,
  selectFilter,
} = outfitFeature;
