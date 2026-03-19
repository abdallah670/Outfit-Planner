import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StyleRule, CreateStyleRuleRequest, UpdateStyleRuleRequest } from '../../domain/entities/user-profile.entity';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class StyleRuleDataSource {
  private readonly apiUrl = `${environment.baseUrl}/user`;

  constructor(private http: HttpClient) {}

  getStyleRules(): Observable<StyleRule[]> {
    return this.http.get<StyleRule[]>(`${this.apiUrl}/style-rules`);
  }

  createStyleRule(request: CreateStyleRuleRequest): Observable<StyleRule> {
    return this.http.post<StyleRule>(`${this.apiUrl}/style-rules`, request);
  }

  updateStyleRule(id: string, request: UpdateStyleRuleRequest): Observable<StyleRule> {
    return this.http.put<StyleRule>(`${this.apiUrl}/style-rules/${id}`, request);
  }

  deleteStyleRule(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/style-rules/${id}`);
  }
}
