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

  sendMessage(message: string, sessionId?: string): Observable<ChatResponse> {
    const body: { message: string; sessionId?: string } = { message };
    if (sessionId) body.sessionId = sessionId;
    return this.http.post<ChatResponse>(`${this.apiUrl}/chat`, body);
  }

  getSessions(): Observable<ChatSession[]> {
    return this.http.get<ChatSession[]>(`${this.apiUrl}/sessions`);
  }

  getSessionMessages(sessionId: string): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(`${this.apiUrl}/sessions/${sessionId}/messages`);
  }
}