import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AiRepository, AI_REPOSITORY } from '../../domain/repositories/ai.repository';
import { AiDataSource } from '../datasources/ai.datasource';
import { ChatSession, ChatMessage, ChatResponse } from '../../domain/entities/ai.entity';

@Injectable({
  providedIn: 'root'
})
export class AiRepositoryImpl implements AiRepository {
  constructor(private readonly dataSource: AiDataSource) {}

  sendMessage(message: string, sessionId?: string, images?: File[]): Observable<ChatResponse> {
    return this.dataSource.sendMessage(message, sessionId, images);
  }

  getSessions(): Observable<ChatSession[]> {
    return this.dataSource.getSessions();
  }

  getSessionMessages(sessionId: string, page: number = 1, pageSize: number = 20): Observable<ChatMessage[]> {
    return this.dataSource.getSessionMessages(sessionId, page, pageSize);
  }
}

export const aiRepositoryProvider = {
  provide: AI_REPOSITORY,
  useClass: AiRepositoryImpl,
};