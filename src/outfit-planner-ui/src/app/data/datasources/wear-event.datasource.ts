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
} from '../../domain/entities/wear-event.entity';

// DTOs matching the API response structure
interface WearEventDto {
  id: string;
  userId: string;
  outfitId?: string;
  clothingItemId?: string;
  eventId?: string;
  wornAt: string;
  durationMinutes?: number;
  weatherCondition?: string;
  rating?: number;
  notes?: string;
  createdAt: string;
}

interface CalendarEventDto {
  id: string;
  outfitId: string;
  outfitName: string;
  outfitImageUrl?: string;
  scheduledDate: string;
  worn: boolean;
  occasion?: string;
  weather?: {
    temp: number;
    icon: string;
    condition: string;
  };
}

interface MonthlyStatsDto {
  wornCount: number;
  scheduledCount: number;
  favoriteCount: number;
}

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
    return this.http.get<MonthlyStatsDto>(`${this.apiUrl}/stats?year=${year}&month=${month}`);
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
    return this.http
      .put<WearEventDto>(`${this.apiUrl}/events/${eventId}/mark-worn`, {})
      .pipe(map((e: WearEventDto) => this.mapWearEventDtoToEntity(e)));
  }

  /**
   * Delete a wear event
   */
  deleteWearEvent(eventId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/events/${eventId}`);
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
      outfitImageUrl: dto.outfitImageUrl,
      scheduledDate: new Date(dto.scheduledDate),
      worn: dto.worn,
      occasion: dto.occasion,
      weather: dto.weather,
    };
  }
}
