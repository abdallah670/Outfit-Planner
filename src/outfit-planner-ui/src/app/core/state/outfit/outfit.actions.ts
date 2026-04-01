import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Outfit } from '../../../domain/entities/outfit.entity';
import { OutfitSuggestionsRequest } from '../../../domain/repositories/outfit.repository';

export interface TodaysPickContext {
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

export const OutfitsActions = createActionGroup({
  source: 'outfits',
  events: {
    'Load Outfits': emptyProps(),
    'Load Outfits Success': props<{ outfits: Outfit[] }>(),
    'Load Outfits Failure': props<{ error: string }>(),
    'Load Outfit By Id': props<{ id: string }>(),
    'Load Outfit By Id Success': props<{ outfit: Outfit }>(),
    'Load Outfit By Id Failure': props<{ error: string }>(),
    'Load Outfits By Category': props<{ category: string }>(),
    'Load Outfits By Category Success': props<{ outfits: Outfit[] }>(),
    'Load Outfits By Category Failure': props<{ error: string }>(),
    'Create Outfit': props<{ outfit: Partial<Outfit> }>(),
    'Create Outfit Success': props<{ outfit: Outfit }>(),
    'Create Outfit Failure': props<{ error: string }>(),
    'Update Outfit': props<{ id: string; outfit: Partial<Outfit> }>(),
    'Update Outfit Success': props<{ outfit: Outfit }>(),
    'Update Outfit Failure': props<{ error: string }>(),
    'Delete Outfit': props<{ id: string }>(),
    'Delete Outfit Success': props<{ id: string }>(),
    'Delete Outfit Failure': props<{ error: string }>(),
    'Record Outfit Wear': props<{ id: string }>(),
    'Record Outfit Wear Success': props<{ outfit: Outfit }>(),
    'Record Outfit Wear Failure': props<{ error: string }>(),
    'Generate Suggestions': props<{ request: OutfitSuggestionsRequest }>(),
    'Generate Suggestions Success': props<{ outfits: Outfit[] }>(),
    'Generate Suggestions Failure': props<{ error: string }>(),
    'Load Todays Pick': props<{ latitude?: number; longitude?: number }>(),
    'Load Todays Pick Success': props<{ outfit: Outfit; context: TodaysPickContext }>(),
    'Load Todays Pick Failure': props<{ error: string }>(),
  },
});
