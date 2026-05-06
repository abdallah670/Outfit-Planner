import { createFeature, createReducer, on } from '@ngrx/store';
import { CalendarActions } from './calendar.actions';
import { CalendarEvent, MonthlyStats, CalendarEventItem, WeatherData } from '../../../domain/entities/wear-event.entity';

export interface CalendarState {
  events: CalendarEvent[];
  stats: MonthlyStats | null;
  currentYear: number;
  currentMonth: number;
  loading: boolean;
  error: string | null;
  
  // NEW: Calendar Events (time-based)
  calendarEvents: CalendarEventItem[];
  calendarEventsLoading: boolean;
  
  // NEW: Weather
  weatherData: Map<string, WeatherData>;
  weatherLoading: boolean;
  
  // NEW: Selected Date
  selectedDate: Date | null;
}

export const initialState: CalendarState = {
  events: [],
  stats: null,
  currentYear: new Date().getFullYear(),
  currentMonth: new Date().getMonth() + 1,
  loading: false,
  error: null,
  calendarEvents: [],
  calendarEventsLoading: false,
  weatherData: new Map(),
  weatherLoading: false,
  selectedDate: null,
};

export const calendarFeature = createFeature({
  name: 'calendar',
  reducer: createReducer(
    initialState,

    // Load Scheduled Outfits
    on(CalendarActions.loadScheduledOutfits, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(CalendarActions.loadScheduledOutfitsSuccess, (state, { events }) => ({
      ...state,
      events: events || [],
      loading: false,
    })),
    on(CalendarActions.loadScheduledOutfitsFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Monthly Stats
    on(CalendarActions.loadMonthlyStats, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(CalendarActions.loadMonthlyStatsSuccess, (state, { stats }) => ({
      ...state,
      stats,
      loading: false,
    })),
    on(CalendarActions.loadMonthlyStatsFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Record Wear Event
    on(CalendarActions.recordWearEvent, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(CalendarActions.recordWearEventSuccess, (state, { event }) => {
      // WearEvent needs to be converted to CalendarEvent format
      const calendarEvent: CalendarEvent = {
        id: event.id,
        outfitId: event.outfitId || '',
        outfitName: 'New Outfit',
        scheduledDate: event.wornAt,
        worn: true,
      };
      return {
        ...state,
        events: [...state.events, calendarEvent],
        loading: false,
      };
    }),
    on(CalendarActions.recordWearEventFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Schedule Outfit
    on(CalendarActions.scheduleOutfit, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(CalendarActions.scheduleOutfitSuccess, (state, { event }) => ({
      ...state,
      events: [...state.events, event],
      loading: false,
    })),
    on(CalendarActions.scheduleOutfitFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Mark As Worn
    on(CalendarActions.markAsWorn, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(CalendarActions.markAsWornSuccess, (state, { event }) => ({
      ...state,
      events: state.events.map((e) => (e.id === event.id ? { ...e, worn: true } : e)),
      loading: false,
    })),
    on(CalendarActions.markAsWornFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Delete Wear Event
    on(CalendarActions.deleteWearEvent, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(CalendarActions.deleteWearEventSuccess, (state, { eventId }) => ({
      ...state,
      events: state.events.filter((e) => e.id !== eventId),
      loading: false,
    })),
    on(CalendarActions.deleteWearEventFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Set Current Month
    on(CalendarActions.setCurrentMonth, (state, { year, month }) => ({
      ...state,
      currentYear: year,
      currentMonth: month,
    })),

    // NEW: Calendar Events (Time-based)
    on(CalendarActions.loadCalendarEvents, (state) => ({
      ...state,
      calendarEventsLoading: true,
      error: null,
    })),
    on(CalendarActions.loadCalendarEventsSuccess, (state, { events }) => ({
      ...state,
      calendarEvents: events || [],
      calendarEventsLoading: false,
    })),
    on(CalendarActions.loadCalendarEventsFailure, (state, { error }) => ({
      ...state,
      calendarEventsLoading: false,
      error,
    })),

    on(CalendarActions.createCalendarEventSuccess, (state, { event }) => ({
      ...state,
      calendarEvents: [...state.calendarEvents, event],
    })),

    on(CalendarActions.updateCalendarEventSuccess, (state, { event }) => ({
      ...state,
      calendarEvents: state.calendarEvents.map((e) => (e.id === event.id ? event : e)),
    })),

    on(CalendarActions.deleteCalendarEventSuccess, (state, { eventId }) => ({
      ...state,
      calendarEvents: state.calendarEvents.filter((e) => e.id !== eventId),
    })),

    // NEW: Weather
    on(CalendarActions.loadWeatherForecast, (state) => ({
      ...state,
      weatherLoading: true,
    })),
    on(CalendarActions.loadWeatherForecastSuccess, (state, { weatherData }) => ({
      ...state,
      weatherData,
      weatherLoading: false,
    })),
    on(CalendarActions.loadWeatherForecastFailure, (state, { error }) => ({
      ...state,
      weatherLoading: false,
      error,
    })),

    // NEW: Select Date
    on(CalendarActions.selectDate, (state, { date }) => ({
      ...state,
      selectedDate: date,
    })),
  ),
});

export const {
  selectEvents,
  reducer,
  selectStats,
  selectCurrentYear,
  selectCurrentMonth,
  selectLoading,
  selectError,
} = calendarFeature;

