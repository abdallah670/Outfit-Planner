import { Inject, Injectable } from '@angular/core';
import {
  WearEventRepository,
  WEAR_EVENT_REPOSITORY,
} from '../repositories/wear-event.repository';
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

@Injectable({
  providedIn: 'root',
})
export class WearEventUseCases {
  constructor(
    @Inject(WEAR_EVENT_REPOSITORY) private readonly wearEventRepository: WearEventRepository,
  ) {}

  /**
   * Get all wear events for the current user
   */
  getWearEvents(): Observable<WearEvent[]> {
    return this.wearEventRepository.getWearEvents();
  }

  /**
   * Get wear events for a specific month
   */
  getWearEventsByMonth(year: number, month: number): Observable<WearEvent[]> {
    return this.wearEventRepository.getWearEventsByMonth(year, month);
  }

  /**
   * Get scheduled outfits for calendar
   */
  getScheduledOutfits(year: number, month: number): Observable<CalendarEvent[]> {
    return this.wearEventRepository.getScheduledOutfits(year, month);
  }

  /**
   * Get monthly statistics
   */
  getMonthlyStats(year: number, month: number): Observable<MonthlyStats> {
    return this.wearEventRepository.getMonthlyStats(year, month);
  }

  /**
   * Record a new wear event
   */
  recordWearEvent(request: RecordWearEventRequest): Observable<WearEvent> {
    return this.wearEventRepository.recordWearEvent(request);
  }

  /**
   * Schedule an outfit for a future date
   */
  scheduleOutfit(request: ScheduleOutfitRequest): Observable<CalendarEvent> {
    return this.wearEventRepository.scheduleOutfit(request);
  }

  /**
   * Mark an outfit as worn
   */
  markAsWorn(eventId: string): Observable<WearEvent> {
    return this.wearEventRepository.markAsWorn(eventId);
  }

  /**
   * Delete a wear event
   */
  deleteWearEvent(eventId: string): Observable<void> {
    return this.wearEventRepository.deleteWearEvent(eventId);
  }

  // ==================== Calendar Events (Time-based) ====================

  /**
   * Get calendar events for a specific date
   */
  getCalendarEventsByDate(date: Date): Observable<CalendarEventItem[]> {
    return this.wearEventRepository.getCalendarEventsByDate(date);
  }

  /**
   * Get calendar events for a specific month
   */
  getCalendarEventsForMonth(year: number, month: number): Observable<CalendarEventItem[]> {
    return this.wearEventRepository.getCalendarEventsForMonth(year, month);
  }

  /**
   * Create a new calendar event
   */
  createCalendarEvent(request: CreateCalendarEventRequest): Observable<CalendarEventItem> {
    return this.wearEventRepository.createCalendarEvent(request);
  }

  /**
   * Update an existing calendar event
   */
  updateCalendarEvent(id: string, request: UpdateCalendarEventRequest): Observable<CalendarEventItem> {
    return this.wearEventRepository.updateCalendarEvent(id, request);
  }

  /**
   * Delete a calendar event
   */
  deleteCalendarEvent(id: string): Observable<void> {
    return this.wearEventRepository.deleteCalendarEvent(id);
  }

  /**
   * Get a single calendar event by ID
   */
  getCalendarEventById(id: string): Observable<CalendarEventItem> {
    return this.wearEventRepository.getCalendarEventById(id);
  }
}
