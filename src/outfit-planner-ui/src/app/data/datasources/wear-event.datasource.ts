import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  WearEvent,
  RecordWearEventRequest,
  ScheduleOutfitRequest,
  CalendarEvent,
  MonthlyStats,
  CalendarEventItem,
  CreateCalendarEventRequest,
  UpdateCalendarEventRequest,
  CalendarEventType,
  WeatherData,
  WearEventDto,
  CalendarEventDto,
  MonthlyStatsDto,
  CalendarEventItemDto,
} from '../../domain/entities/wear-event.entity';


@Injectable({
  providedIn: 'root',
})
export class WearEventDataSource {
  private readonly apiUrl = `${environment.baseUrl}/calendar`;

  constructor(private http: HttpClient) {}

  /**
   * Get all wear events for the current user
   */
  getWearEvents(): Observable<WearEvent[]> {
    return this.http
      .get<WearEventDto[]>(`${this.apiUrl}/events`)
      .pipe(map((events: WearEventDto[]) => events.map((e) => this.mapWearEventDtoToEntity(e))));
  }

  /**
   * Get wear events for a specific month
   */
  getWearEventsByMonth(year: number, month: number): Observable<WearEvent[]> {
    return this.http
      .get<WearEventDto[]>(`${this.apiUrl}/events?year=${year}&month=${month}`)
      .pipe(map((events: WearEventDto[]) => events.map((e) => this.mapWearEventDtoToEntity(e))));
  }

  /**
   * Get scheduled outfits for calendar
   */
  getScheduledOutfits(year: number, month: number): Observable<CalendarEvent[]> {
    return this.http
      .get<CalendarEventDto[]>(`${this.apiUrl}/scheduled?year=${year}&month=${month}`)
      .pipe(
        map((events: CalendarEventDto[]) => events.map((e) => this.mapCalendarEventDtoToEntity(e))),
      );
  }

  /**
   * Get monthly statistics
   */
  getMonthlyStats(year: number, month: number): Observable<MonthlyStats> {
    return this.http.get<MonthlyStatsDto>(`${this.apiUrl}/stats?year=${year}&month=${month}`).pipe(
      map((dto: MonthlyStatsDto) => ({
        wornCount: dto.totalWorn,
        scheduledCount: dto.totalScheduled,
        favoriteCount: dto.uniqueOutfitsWorn // Using unique outfits as favorite count for now or mapping accordingly
      }))
    );
  }

  /**
   * Record a new wear event
   */
  recordWearEvent(request: RecordWearEventRequest): Observable<WearEvent> {
    return this.http
      .post<WearEventDto>(`${this.apiUrl}/events`, request)
      .pipe(map((e: WearEventDto) => this.mapWearEventDtoToEntity(e)));
  }

  /**
   * Schedule an outfit for a future date
   */
  scheduleOutfit(request: ScheduleOutfitRequest): Observable<CalendarEvent> {
    return this.http
      .post<CalendarEventDto>(`${this.apiUrl}/schedule`, request)
      .pipe(map((e: CalendarEventDto) => this.mapCalendarEventDtoToEntity(e)));
  }

  /**
   * Mark an outfit as worn
   */
  markAsWorn(eventId: string): Observable<WearEvent> {
    const request = {
      rating: 0,
      durationMinutes: 0,
      weatherCondition: '',
      notes: ''
    };
    return this.http
      .post<WearEventDto>(`${this.apiUrl}/events/${eventId}/mark-worn`, request)
      .pipe(map((e: WearEventDto) => this.mapWearEventDtoToEntity(e)));
  }

  /**
   * Delete a wear event
   */
  deleteWearEvent(eventId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/events/${eventId}`);
  }

  // ==================== Calendar Events (Time-based) ====================

  /**
   * Get calendar events for a specific date
   */
  getCalendarEventsByDate(date: Date): Observable<CalendarEventItem[]> {
    const dateStr = date.toISOString();
    return this.http
      .get<CalendarEventItemDto[]>(`${this.apiUrl}/calendar-events/by-date?date=${dateStr}`)
      .pipe(
        map((events: CalendarEventItemDto[]) =>
          events.map((e) => this.mapCalendarEventItemDtoToEntity(e))
        )
      );
  }

  /**
   * Get calendar events for a specific month
   */
  getCalendarEventsForMonth(year: number, month: number): Observable<CalendarEventItem[]> {
    return this.http
      .get<CalendarEventItemDto[]>(`${this.apiUrl}/calendar-events?year=${year}&month=${month}`)
      .pipe(
        map((events: CalendarEventItemDto[]) =>
          events.map((e) => this.mapCalendarEventItemDtoToEntity(e))
        )
      );
  }

  /**
   * Create a new calendar event
   */
  createCalendarEvent(request: CreateCalendarEventRequest): Observable<CalendarEventItem> {
    return this.http
      .post<CalendarEventItemDto>(`${this.apiUrl}/calendar-events`, request)
      .pipe(map((e: CalendarEventItemDto) => this.mapCalendarEventItemDtoToEntity(e)));
  }

  /**
   * Update an existing calendar event
   */
  updateCalendarEvent(id: string, request: UpdateCalendarEventRequest): Observable<CalendarEventItem> {
    return this.http
      .put<CalendarEventItemDto>(`${this.apiUrl}/calendar-events/${id}`, request)
      .pipe(map((e: CalendarEventItemDto) => this.mapCalendarEventItemDtoToEntity(e)));
  }

  /**
   * Delete a calendar event
   */
  deleteCalendarEvent(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/calendar-events/${id}`);
  }

  /**
   * Get a single calendar event by ID
   */
  getCalendarEventById(id: string): Observable<CalendarEventItem> {
    return this.http
      .get<CalendarEventItemDto>(`${this.apiUrl}/calendar-events/${id}`)
      .pipe(map((e: CalendarEventItemDto) => this.mapCalendarEventItemDtoToEntity(e)));
  }

  /**
   * Map WearEvent DTO to entity
   */
  private mapWearEventDtoToEntity(dto: WearEventDto): WearEvent {
    return {
      id: dto.id,
      userId: dto.userId,
      outfitId: dto.outfitId,
      clothingItemId: dto.clothingItemId,
      eventId: dto.eventId,
      wornAt: new Date(dto.wornAt),
      durationMinutes: dto.durationMinutes,
      weatherCondition: dto.weatherCondition,
      rating: dto.rating,
      notes: dto.notes,
      createdAt: new Date(dto.createdAt),
    };
  }

  /**
   * Map CalendarEvent DTO to entity
   */
  private mapCalendarEventDtoToEntity(dto: CalendarEventDto): CalendarEvent {
    return {
      id: dto.id,
      outfitId: dto.outfitId,
      outfitName: dto.outfitName,
      outfitImageUrl: this.getImageUrl(dto.outfitImageUrl),
      scheduledDate: new Date(dto.scheduledDate),
      worn: dto.worn,
      occasion: dto.occasion,
      weather: dto.weather,
    };
  }

  /**
   * Map CalendarEventItem DTO to entity
   */
  private mapCalendarEventItemDtoToEntity(dto: CalendarEventItemDto): CalendarEventItem {
    return {
      id: dto.id,
      title: dto.title,
      description: dto.description,
      location: dto.location,
      eventDate: new Date(dto.eventDate),
      startTime: dto.startTime,
      startTimeDisplay: dto.startTimeDisplay,
      endTime: dto.endTime,
      endTimeDisplay: dto.endTimeDisplay,
      eventType: dto.eventType as CalendarEventType,
      wearEventId: dto.wearEventId,
      notes: dto.notes,
      isRecurring: dto.isRecurring,
      outfitName: (dto as any).outfitName,
      outfitImageUrl: this.getImageUrl((dto as any).outfitImageUrl),
      hasOutfit: !!(dto as any).wearEventId || !!(dto as any).outfitName,
    };
  }

  /**
   * Resolve image URL (prepend base URL if relative)
   */
  private getImageUrl(url: string | undefined): string | undefined {
    if (!url) return undefined;
    if (url.startsWith('http')) return url;
    return `${environment.resourceBaseUrl}${url.startsWith('/') ? '' : '/'}${url}`;
  }
}
