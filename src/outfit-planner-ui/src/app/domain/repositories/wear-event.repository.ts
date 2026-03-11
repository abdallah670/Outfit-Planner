import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import {
  WearEvent,
  RecordWearEventRequest,
  ScheduleOutfitRequest,
  CalendarEvent,
  MonthlyStats,
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
}
