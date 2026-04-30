import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { CommandResponse } from '../entities/response.entity';
import { FeedPost } from '../entities/feed.entity';

export const OUTFIT_POSTS_REPOSITORY = new InjectionToken<OutfitPostsRepository>('OutfitPostsRepository');

export interface OutfitPostsRepository {
  createOutfitPost(dto: any): Observable<CommandResponse>;
  getOutfitPost(id: string): Observable<FeedPost>;
  updateOutfitPost(id: string, dto: any): Observable<CommandResponse>;
  deleteOutfitPost(id: string): Observable<void>;
}
