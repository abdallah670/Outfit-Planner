import { Injectable } from '@angular/core';
import {
  WearEventRepository,
  WEAR_EVENT_REPOSITORY,
} from '../../domain/repositories/wear-event.repository';
import { WearEventDataSource } from '../datasources/wear-event.datasource';
import { Observable } from 'rxjs';
import {
  WearEvent,
  RecordWearEventRequest,
  ScheduleOutfitRequest,
  CalendarEvent,
  MonthlyStats,
} from '../../domain/entities/wear-event.entity';

@Injectable({
  providedIn: 'root',
})
export class WearEventRepositoryImpl implements WearEventRepository {
  constructor(private readonly wearEventDataSource: WearEventDataSource) {}

  getWearEvents(): Observable<WearEvent[]> {
    return this.wearEventDataSource.getWearEvents();
  }

  getWearEventsByMonth(year: number, month: number): Observable<WearEvent[]> {
    return this.wearEventDataSource.getWearEventsByMonth(year, month);
  }

  getScheduledOutfits(year: number, month: number): Observable<CalendarEvent[]> {
    return this.wearEventDataSource.getScheduledOutfits(year, month);
  }

  getMonthlyStats(year: number, month: number): Observable<MonthlyStats> {
    return this.wearEventDataSource.getMonthlyStats(year, month);
  }

  recordWearEvent(request: RecordWearEventRequest): Observable<WearEvent> {
    return this.wearEventDataSource.recordWearEvent(request);
  }

  scheduleOutfit(request: ScheduleOutfitRequest): Observable<CalendarEvent> {
    return this.wearEventDataSource.scheduleOutfit(request);
  }

  markAsWorn(eventId: string): Observable<WearEvent> {
    return this.wearEventDataSource.markAsWorn(eventId);
  }

  deleteWearEvent(eventId: string): Observable<void> {
    return this.wearEventDataSource.deleteWearEvent(eventId);
  }
}

export const wearEventRepositoryProvider = {
  provide: WEAR_EVENT_REPOSITORY,
  useClass: WearEventRepositoryImpl,
};
