import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { TrendingOutfit } from '../../domain/entities/outfit.entity';
import { CursorPagedResult } from '../../domain/entities/response.entity';


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
  isFollowing?:boolean;
  isOwner?:boolean;
  isLiked:boolean;
}

@Injectable({
  providedIn: 'root',
})
export class TrendingDataSource {
  private readonly apiUrl = `${environment.baseUrl}/trending/outfits`;

  constructor(private http: HttpClient) {}

  getTrendingOutfits(cursor?: string, pageSize: number = 20): Observable<CursorPagedResult<TrendingOutfit>> {
    let url = `${this.apiUrl}?pageSize=${pageSize}`;
    if (cursor) {
      url += `&cursor=${encodeURIComponent(cursor)}`;
    }
    return this.http.get<any>(url).pipe(
      map((response: any) => ({
        items: response.items.map((o: TrendingOutfitDto) => this.mapTrendingOutfitDtoToEntity(o)),
        nextCursor: response.nextCursor,
        hasMore: response.hasMore,
        pageSize: response.pageSize
      }))
    );
  }


  private mapTrendingOutfitDtoToEntity(dto: TrendingOutfitDto): TrendingOutfit {
    return {
      id: dto.id,
      userId: dto.userId,
      userName: dto.userName,
      userAvatar: dto.userAvatar || 'assets/default-avatar.png',
      imageUrl:  dto.imageUrl && !dto.imageUrl.startsWith('http') 
        ? `${environment.resourceBaseUrl}${dto.imageUrl}` 
        : dto.imageUrl || 'assets/placeholder.png',
      likes: dto.voteCount,
      comments: dto.commentsCount,
      trendingScore: dto.trendingScore,
      createdAt: new Date(dto.createdAt),
      isfollowing:dto.isFollowing,
      isliked:dto.isLiked,
      isowner:dto.isOwner
    };
  }
}
