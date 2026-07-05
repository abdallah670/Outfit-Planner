import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChatSession, ChatMessage, ChatResponse } from '../../domain/entities/ai.entity';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AiDataSource {
  private readonly apiUrl = `${environment.baseUrl}/ai`;

  constructor(private readonly http: HttpClient) {}

  sendMessage(
    message: string,
    sessionId?: string,
    images?: File[],
    clothingItemIds?: string[]
  ): Observable<ChatResponse> {
    const formData = new FormData();
    formData.append('Message', message);
    if (sessionId) formData.append('SessionId', sessionId);

    if (images?.length) {
      images.forEach(img => formData.append('UploadedImages', img));
    }

    // Append each clothing item ID as a separate form field so that ASP.NET Core's
    // [FromForm] List<Guid> model binding works correctly (JSON.stringify would not work).
    if (clothingItemIds?.length) {
      clothingItemIds.forEach(id => formData.append('ClothingItemIds', id));
    }

    return this.http.post<ChatResponse>(`${this.apiUrl}/chat`, formData);
  }

  getSessions(): Observable<ChatSession[]> {
    return this.http.get<ChatSession[]>(`${this.apiUrl}/sessions`);
  }

  getSessionMessages(sessionId: string, page: number = 1, pageSize: number = 20): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(`${this.apiUrl}/sessions/${sessionId}/messages?page=${page}&pageSize=${pageSize}`);
  }

  deleteSession(sessionId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/sessions/${sessionId}`);
  }
}
