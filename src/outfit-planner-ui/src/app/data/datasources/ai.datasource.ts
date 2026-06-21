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

  sendMessage(message: string, sessionId?: string, images?: File[]): Observable<ChatResponse> {
    const formData = new FormData();
    formData.append('Message', message);
    if (sessionId) formData.append('SessionId', sessionId);
    if (images && images.length > 0) {
      images.forEach(img => formData.append('UploadedImages', img));
    }
    return this.http.post<ChatResponse>(`${this.apiUrl}/chat`, formData);
  }

  getSessions(): Observable<ChatSession[]> {
    return this.http.get<ChatSession[]>(`${this.apiUrl}/sessions`);
  }

  getSessionMessages(sessionId: string): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(`${this.apiUrl}/sessions/${sessionId}/messages`);
  }
}