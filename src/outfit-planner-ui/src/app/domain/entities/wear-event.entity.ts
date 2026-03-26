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

/**
 * Calendar event types
 */
export enum CalendarEventType {
  General = 0,
  Work = 1,
  Meeting = 2,
  Social = 3,
  Date = 4,
  Party = 5,
  Sport = 6,
  Travel = 7,
  Appointment = 8
}

/**
 * Calendar event item (new events feature)
 */
export interface CalendarEventItem {
  id: string;
  title: string;
  description?: string;
  location?: string;
  eventDate: Date;
  startTime?: string; // Formatted as "2:00 PM"
  endTime?: string;   // Formatted as "4:00 PM"
  eventType: CalendarEventType;
  wearEventId?: string;
  notes?: string;
  isRecurring: boolean;
}

/**
 * Request to create a calendar event
 */
export interface CreateCalendarEventRequest {
  title: string;
  description?: string;
  location?: string;
  eventDate: Date;
  startTime?: string; // Format: "HH:mm:ss" (e.g., "14:30:00") for .NET TimeSpan compatibility
  endTime?: string;   // Format: "HH:mm:ss" (e.g., "16:00:00") for .NET TimeSpan compatibility
  eventType: CalendarEventType;
  outfitId?: string;
  notes?: string;
}

/**
 * Request to update a calendar event
 */
export interface UpdateCalendarEventRequest {
  title?: string;
  description?: string;
  location?: string;
  eventDate?: Date;
  startTime?: string; // Format: "HH:mm:ss" (e.g., "14:30:00") for .NET TimeSpan compatibility
  endTime?: string;   // Format: "HH:mm:ss" (e.g., "16:00:00") for .NET TimeSpan compatibility
  eventType?: CalendarEventType;
  outfitId?: string;
  notes?: string;
}

/**
 * Weather data for a specific date
 */
export interface WeatherData {
  date: Date;
  temperature: number;
  condition: string;
  icon: string;
  humidity?: number;
  windSpeed?: number;
}
