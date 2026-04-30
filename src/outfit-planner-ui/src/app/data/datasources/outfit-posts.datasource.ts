import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CommandResponse } from '../../domain/entities/response.entity';
import { FeedPost } from '../../domain/entities/feed.entity';

export interface CreateOutfitPostRequest {
  outfitId: string;
  caption?: string;
  visibility: number;
}

export interface UpdateOutfitPostRequest {
  caption?: string;
  visibility: number;
}

@Injectable({
  providedIn: 'root',
})
export class OutfitPostsDataSource {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.baseUrl}/api/outfitposts`;

  createOutfitPost(dto: CreateOutfitPostRequest): Observable<CommandResponse> {
    return this.http.post<CommandResponse>(this.apiUrl, dto);
  }

  getOutfitPost(id: string): Observable<FeedPost> {
    return this.http.get<FeedPost>(`${this.apiUrl}/${id}`);
  }

  updateOutfitPost(id: string, dto: UpdateOutfitPostRequest): Observable<CommandResponse> {
    return this.http.put<CommandResponse>(`${this.apiUrl}/${id}`, dto);
  }

  deleteOutfitPost(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
