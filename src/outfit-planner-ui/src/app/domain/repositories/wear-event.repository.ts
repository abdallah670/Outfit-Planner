import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import {
  WearEvent,
  RecordWearEventRequest,
  ScheduleOutfitRequest,
  CalendarEvent,
  MonthlyStats,
  CalendarEventItem,
  CreateCalendarEventRequest,
  UpdateCalendarEventRequest,
} from '../entities/wear-event.entity';

export const WEAR_EVENT_REPOSITORY = new InjectionToken<WearEventRepository>('WearEventRepository');

/**
 * Repository interface for wear event operations
 */
export interface WearEventRepository {
  /**
   * Get all wear events for the current user
   */
  getWearEvents(): Observable<WearEvent[]>;

  /**
   * Get wear events for a specific month
   */
  getWearEventsByMonth(year: number, month: number): Observable<WearEvent[]>;

  /**
   * Get scheduled outfits for calendar
   */
  getScheduledOutfits(year: number, month: number): Observable<CalendarEvent[]>;

  /**
   * Get monthly statistics
   */
  getMonthlyStats(year: number, month: number): Observable<MonthlyStats>;

  /**
   * Record a new wear event
   */
  recordWearEvent(request: RecordWearEventRequest): Observable<WearEvent>;

  /**
   * Schedule an outfit for a future date
   */
  scheduleOutfit(request: ScheduleOutfitRequest): Observable<CalendarEvent>;

  /**
   * Mark an outfit as worn
   */
  markAsWorn(eventId: string): Observable<WearEvent>;

  /**
   * Delete a wear event
   */
  deleteWearEvent(eventId: string): Observable<void>;

  // ==================== Calendar Events (Time-based) ====================

  /**
   * Get calendar events for a specific date
   */
  getCalendarEventsByDate(date: Date): Observable<CalendarEventItem[]>;

  /**
   * Get calendar events for a specific month
   */
  getCalendarEventsForMonth(year: number, month: number): Observable<CalendarEventItem[]>;

  /**
   * Create a new calendar event
   */
  createCalendarEvent(request: CreateCalendarEventRequest): Observable<CalendarEventItem>;

  /**
   * Update an existing calendar event
   */
  updateCalendarEvent(id: string, request: UpdateCalendarEventRequest): Observable<CalendarEventItem>;

  /**
   * Delete a calendar event
   */
  deleteCalendarEvent(id: string): Observable<void>;

  /**
   * Get a single calendar event by ID
   */
  getCalendarEventById(id: string): Observable<CalendarEventItem>;
}
