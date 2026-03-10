/**
 * Represents an event when an outfit was worn
 */
export interface WearEvent {
  id: string;
  userId: string;
  outfitId?: string;
  clothingItemId?: string;
  eventId?: string;
  wornAt: Date;
  durationMinutes?: number;
  weatherCondition?: string;
  rating?: number; // 1-5 scale
  notes?: string;
  createdAt: Date;
}

/**
 * DTO for recording a new wear event
 */
export interface RecordWearEventRequest {
  outfitId?: string;
  clothingItemId?: string;
  wornAt: Date;
  durationMinutes?: number;
  weatherCondition?: string;
  rating?: number;
  eventId?: string;
  notes?: string;
}

/**
 * DTO for scheduling an outfit on the calendar
 */
export interface ScheduleOutfitRequest {
  outfitId: string;
  scheduledDate: Date;
  occasion?: string;
  notes?: string;
}

/**
 * Calendar event representation
 */
export interface CalendarEvent {
  id: string;
  outfitId: string;
  outfitName: string;
  outfitImageUrl?: string;
  scheduledDate: Date;
  worn: boolean;
  occasion?: string;
  weather?: {
    temp: number;
    icon: string;
    condition: string;
  };
}

/**
 * Monthly statistics for calendar
 */
export interface MonthlyStats {
  wornCount: number;
  scheduledCount: number;
  favoriteCount: number;
}
