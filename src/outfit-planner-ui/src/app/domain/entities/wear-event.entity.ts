/**
 * Represents an event when an outfit was worn
 */
export interface WearEvent {
  id: string;
  outfitId: string;
  userId: string;
  wornAt: Date;
  weatherCondition?: string;
  eventId?: string;
  notes?: string;
  createdAt: Date;
}

/**
 * DTO for recording a new wear event
 */
export interface RecordWearEventRequest {
  outfitId: string;
  wornAt: Date;
  weatherCondition?: string;
  eventId?: string;
  notes?: string;
}
