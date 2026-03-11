import { createSelector, createFeatureSelector } from '@ngrx/store';
import { CalendarState } from './calendar.reducer';
import { CalendarEvent } from '../../../domain/entities/wear-event.entity';

export const selectCalendarState = createFeatureSelector<CalendarState>('calendar');

export const selectEvents = createSelector(
  selectCalendarState,
  (state: CalendarState): CalendarEvent[] => state?.events || [],
);

export const selectStats = createSelector(
  selectCalendarState,
  (state) => state?.stats,
);

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
