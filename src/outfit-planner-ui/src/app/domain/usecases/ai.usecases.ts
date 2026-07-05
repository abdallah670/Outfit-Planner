import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AI_REPOSITORY, AiRepository } from '../repositories/ai.repository';
import { ChatSession, ChatMessage, ChatResponse } from '../entities/ai.entity';

@Injectable({
  providedIn: 'root',
})
export class AiUseCases {
  constructor(@Inject(AI_REPOSITORY) private readonly aiRepository: AiRepository) {}

  sendMessage(message: string, sessionId?: string, images?: File[], clothingItemIds?: string[]): Observable<ChatResponse> {
    return this.aiRepository.sendMessage(message, sessionId, images, clothingItemIds);
  }

  getSessions(): Observable<ChatSession[]> {
    return this.aiRepository.getSessions();
  }

  getSessionMessages(sessionId: string, page: number = 1, pageSize: number = 20): Observable<ChatMessage[]> {
    return this.aiRepository.getSessionMessages(sessionId, page, pageSize);
  }

  deleteSession(sessionId: string): Observable<void> {
    return this.aiRepository.deleteSession(sessionId);
  }
}
