import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { ChatSession, ChatMessage, ChatResponse } from '../entities/ai.entity';

export const AI_REPOSITORY = new InjectionToken<AiRepository>('AiRepository');

export interface AiRepository {
  sendMessage(message: string, sessionId?: string, images?: File[], clothingItemIds?: string[]): Observable<ChatResponse>;
  getSessions(): Observable<ChatSession[]>;
  getSessionMessages(sessionId: string, page?: number, pageSize?: number): Observable<ChatMessage[]>;
  deleteSession(sessionId: string): Observable<void>;
}
