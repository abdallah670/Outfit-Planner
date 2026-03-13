import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  WearEvent,
  RecordWearEventRequest,
  ScheduleOutfitRequest,
  CalendarEvent,
  MonthlyStats,
  CalendarEventItem,
  WeatherData,
} from '../../../domain/entities/wear-event.entity';

export const CalendarActions = createActionGroup({
  source: 'calendar',
  events: {
    // Load Scheduled Outfits
    'Load Scheduled Outfits': props<{ year: number; month: number }>(),
    'Load Scheduled Outfits Success': props<{ events: CalendarEvent[] }>(),
    'Load Scheduled Outfits Failure': props<{ error: string }>(),

    // Load Monthly Stats
    'Load Monthly Stats': props<{ year: number; month: number }>(),
    'Load Monthly Stats Success': props<{ stats: MonthlyStats }>(),
    'Load Monthly Stats Failure': props<{ error: string }>(),

    // Record Wear Event
    'Record Wear Event': props<{ request: RecordWearEventRequest }>(),
    'Record Wear Event Success': props<{ event: WearEvent }>(),
    'Record Wear Event Failure': props<{ error: string }>(),

    // Schedule Outfit
    'Schedule Outfit': props<{ request: ScheduleOutfitRequest }>(),
    'Schedule Outfit Success': props<{ event: CalendarEvent }>(),
    'Schedule Outfit Failure': props<{ error: string }>(),

    // Mark as Worn
    'Mark As Worn': props<{ eventId: string }>(),
    'Mark As Worn Success': props<{ event: WearEvent }>(),
    'Mark As Worn Failure': props<{ error: string }>(),

    // Delete Wear Event
    'Delete Wear Event': props<{ eventId: string }>(),
    'Delete Wear Event Success': props<{ eventId: string }>(),
    'Delete Wear Event Failure': props<{ error: string }>(),

    // Set Current Month
    'Set Current Month': props<{ year: number; month: number }>(),

    // NEW: Calendar Events (Time-based)
    'Load Calendar Events': props<{ year: number; month: number }>(),
    'Load Calendar Events Success': props<{ events: CalendarEventItem[] }>(),
    'Load Calendar Events Failure': props<{ error: string }>(),

    'Create Calendar Event': props<{ event: Partial<CalendarEventItem> }>(),
    'Create Calendar Event Success': props<{ event: CalendarEventItem }>(),
    'Create Calendar Event Failure': props<{ error: string }>(),

    'Update Calendar Event': props<{ eventId: string; event: Partial<CalendarEventItem> }>(),
    'Update Calendar Event Success': props<{ event: CalendarEventItem }>(),
    'Update Calendar Event Failure': props<{ error: string }>(),

    'Delete Calendar Event': props<{ eventId: string }>(),
    'Delete Calendar Event Success': props<{ eventId: string }>(),
    'Delete Calendar Event Failure': props<{ error: string }>(),

    // NEW: Weather
    'Load Weather Forecast': props<{ year: number; month: number }>(),
    'Load Weather Forecast Success': props<{ weatherData: Map<string, WeatherData> }>(),
    'Load Weather Forecast Failure': props<{ error: string }>(),

    // NEW: Select Date
    'Select Date': props<{ date: Date | null }>(),
  },
});
