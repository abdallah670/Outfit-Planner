import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { TrendingOutfit } from '../../domain/entities/outfit.entity';

interface TrendingOutfitDto {
  id: string;
  name: string;
  description?: string;
  imageUrl?: string;
  userId: string;
  userName: string;
  userAvatar?: string;
  voteCount: number;
  commentsCount: number;
  trendingScore: number;
  createdAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class TrendingDataSource {
  private readonly apiUrl = `${environment.baseUrl}/trending/outfits`;

  constructor(private http: HttpClient) {}

  getTrendingOutfits(page = 1, pageSize = 20): Observable<{ items: TrendingOutfit[]; totalCount: number }> {
    return this.http
      .get<any>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}`)
      .pipe(
        map((response: any) => ({
          items: response.items.map((o: TrendingOutfitDto) => this.mapTrendingOutfitDtoToEntity(o)),
          totalCount: response.totalCount,
        })),
      );
  }

  private mapTrendingOutfitDtoToEntity(dto: TrendingOutfitDto): TrendingOutfit {
    return {
      id: dto.id,
      userId: dto.userId,
      userName: dto.userName,
      userAvatar: dto.userAvatar || 'assets/default-avatar.png',
      imageUrl: dto.imageUrl || 'assets/placeholder.png',
      likes: dto.voteCount,
      comments: dto.commentsCount,
      occasion: 'Trending',
      trendingScore: dto.trendingScore,
      voteId: '', // Legacy field for mapping compatibility
      createdAt: new Date(dto.createdAt),
    };
  }
}
