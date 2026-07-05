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

  sendMessage(message: string, sessionId?: string, images?: File[], clothingItemIds?: string[]): Observable<ChatResponse> {
    return this.dataSource.sendMessage(message, sessionId, images, clothingItemIds);
  }

  getSessions(): Observable<ChatSession[]> {
    return this.dataSource.getSessions();
  }

  getSessionMessages(sessionId: string, page: number = 1, pageSize: number = 20): Observable<ChatMessage[]> {
    return this.dataSource.getSessionMessages(sessionId, page, pageSize).pipe(
      map(msgs => msgs.map(m => this.enrichMessage(m)))
    );
  }

  deleteSession(sessionId: string): Observable<void> {
    return this.dataSource.deleteSession(sessionId);
  }

  private enrichMessage(m: ChatMessage): ChatMessage {
    if (m.metadata && typeof m.metadata === 'string') {
      try {
        const parsed = JSON.parse(m.metadata);
        const rawSuggestions = parsed.outfitSuggestions || parsed.OutfitSuggestions;
        const outfitSuggestions = rawSuggestions?.map((s: any) => ({
          rank: s.rank ?? s.Rank,
          totalScore: s.totalScore ?? s.TotalScore ?? 0,
          scoreBreakdown: s.scoreBreakdown ?? s.ScoreBreakdown,
          items: (s.items || s.Items)?.map((item: any) => ({
            id: item.id ?? item.Id,
            name: item.name ?? item.Name ?? '',
            type: item.type ?? item.Type ?? '',
            imageUrl: item.imageUrl ?? item.ImageUrl ?? '',
            hexColor: item.hexColor ?? item.HexColor ?? '#ccc'
          })) ?? []
        })) ?? [];
        const uploadedImageUrls = parsed.uploadedImageUrls || parsed.UploadedImageUrls;
        return { ...m, outfitSuggestions, images: uploadedImageUrls ?? m.images };
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