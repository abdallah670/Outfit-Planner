import { createSelector, createFeatureSelector } from '@ngrx/store';
import { CalendarState } from './calendar.reducer';
import {
  CalendarEvent,
  CalendarEventItem,
  WeatherData,
} from '../../../domain/entities/wear-event.entity';

export const selectCalendarState = createFeatureSelector<CalendarState>('calendar');

export const selectEvents = createSelector(
  selectCalendarState,
  (state: CalendarState): CalendarEvent[] => state?.events || [],
);

export const selectStats = createSelector(selectCalendarState, (state) => state?.stats);

export const selectCurrentYear = createSelector(
  selectCalendarState,
  (state): number => state?.currentYear || new Date().getFullYear(),
);

export const selectCurrentMonth = createSelector(
  selectCalendarState,
  (state): number => state?.currentMonth || new Date().getMonth() + 1,
);

export const selectLoading = createSelector(
  selectCalendarState,
  (state): boolean => !!state?.loading,
);

export const selectError = createSelector(
  selectCalendarState,
  (state): string | null => state?.error || null,
);

// ==================== Calendar Events (Time-based) ====================

export const selectCalendarEvents = createSelector(
  selectCalendarState,
  (state: CalendarState): CalendarEventItem[] => state?.calendarEvents || [],
);

export const selectCalendarEventsLoading = createSelector(
  selectCalendarState,
  (state: CalendarState): boolean => !!state?.calendarEventsLoading,
);

export const selectSelectedDate = createSelector(
  selectCalendarState,
  (state: CalendarState): Date | null => state?.selectedDate || null,
);

// Get calendar events for a specific date
export const selectCalendarEventsByDate = (date: Date) =>
  createSelector(selectCalendarEvents, (events: CalendarEventItem[]): CalendarEventItem[] => {
    if (!date) return [];
    return events.filter((event) => {
      const eventDate = new Date(event.eventDate);
      return (
        eventDate.getDate() === date.getDate() &&
        eventDate.getMonth() === date.getMonth() &&
        eventDate.getFullYear() === date.getFullYear()
      );
    });
  });

// Get events grouped by date for calendar display
export const selectCalendarEventsGroupedByDate = createSelector(
  selectCalendarEvents,
  (events: CalendarEventItem[]): Map<string, CalendarEventItem[]> => {
    const grouped = new Map<string, CalendarEventItem[]>();
    events.forEach((event) => {
      const dateKey = new Date(event.eventDate).toISOString().split('T')[0];
      const existing = grouped.get(dateKey) || [];
      grouped.set(dateKey, [...existing, event]);
    });
    return grouped;
  },
);

// Get selected day events (combines date selection + events)
export const selectSelectedDayCalendarEvents = createSelector(
  selectCalendarEvents,
  selectSelectedDate,
  (events: CalendarEventItem[], selectedDate: Date | null): CalendarEventItem[] => {
    if (!selectedDate) return [];
    return events.filter((event) => {
      const eventDate = new Date(event.eventDate);
      return (
        eventDate.getDate() === selectedDate.getDate() &&
        eventDate.getMonth() === selectedDate.getMonth() &&
        eventDate.getFullYear() === selectedDate.getFullYear()
      );
    });
  },
);

// Get events for a specific date
export const selectEventsByDate = (date: Date) =>
  createSelector(selectEvents, (events: CalendarEvent[]): CalendarEvent[] => {
    return events.filter((event) => {
      const eventDate = new Date(event.scheduledDate);
      return (
        eventDate.getDate() === date.getDate() &&
        eventDate.getMonth() === date.getMonth() &&
        eventDate.getFullYear() === date.getFullYear()
      );
    });
  });

// Get events grouped by date for calendar display
export const selectEventsGroupedByDate = createSelector(
  selectEvents,
  (events: CalendarEvent[]): Map<string, CalendarEvent[]> => {
    const grouped = new Map<string, CalendarEvent[]>();
    events.forEach((event) => {
      const dateKey = new Date(event.scheduledDate).toISOString().split('T')[0];
      const existing = grouped.get(dateKey) || [];
      grouped.set(dateKey, [...existing, event]);
    });
    return grouped;
  },
);

// ==================== Weather Data ====================

export const selectWeatherData = createSelector(
  selectCalendarState,
  (state: CalendarState): Map<string, WeatherData> => state?.weatherData || new Map(),
);

export const selectWeatherLoading = createSelector(
  selectCalendarState,
  (state: CalendarState): boolean => !!state?.weatherLoading,
);

// Get weather for a specific date
export const selectWeatherByDate = (date: Date) =>
  createSelector(selectWeatherData, (weatherMap: Map<string, WeatherData>): WeatherData | null => {
    if (!date) return null;
    const dateKey = date.toISOString().split('T')[0];
    return weatherMap.get(dateKey) || null;
  });
