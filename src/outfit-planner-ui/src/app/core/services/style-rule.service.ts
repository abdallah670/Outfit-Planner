import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StyleRule, CreateStyleRuleRequest, UpdateStyleRuleRequest } from '../../domain/entities/user-profile.entity';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class StyleRuleService {
  private readonly apiUrl = `${environment.baseUrl}/user/style-rules`;

  constructor(private http: HttpClient) {}

  getStyleRules(): Observable<StyleRule[]> {
    return this.http.get<StyleRule[]>(this.apiUrl);
  }

  createStyleRule(rule: CreateStyleRuleRequest): Observable<StyleRule> {
    return this.http.post<StyleRule>(this.apiUrl, rule);
  }

  updateStyleRule(id: string, rule: UpdateStyleRuleRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, rule);
  }

  deleteStyleRule(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
