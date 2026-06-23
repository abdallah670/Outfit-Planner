import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
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
    return this.dataSource.getSessionMessages(sessionId, page, pageSize).pipe(
      map(msgs => msgs.map(m => this.enrichMessage(m)))
    );
  }

  private enrichMessage(m: ChatMessage): ChatMessage {
    if (m.metadata && typeof m.metadata === 'string') {
      try {
        const parsed = JSON.parse(m.metadata);
        const outfitSuggestions = parsed.outfitSuggestions?.map((s: any) => ({
          rank: s.rank,
          totalScore: s.totalScore,
          scoreBreakdown: s.scoreBreakdown,
          items: s.items?.map((item: any) => ({
            id: item.Id,
            name: item.Name,
            type: item.Type,
            imageUrl: item.ImageUrl,
            hexColor: item.HexColor
          })) ?? []
        })) ?? [];
        const suggestedActions = parsed.suggestedActions ?? [];
        return { ...m, outfitSuggestions, suggestedActions };
      } catch {
        return m;
      }
    }
    // Also support when API returns outfitSuggestions already populated
    return m;
  }
}

export const aiRepositoryProvider = {
  provide: AI_REPOSITORY,
  useClass: AiRepositoryImpl,
};