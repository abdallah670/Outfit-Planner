import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { CommandResponse } from '../entities/response.entity';
import { FeedPost } from '../entities/feed.entity';
import { CreateOutfitPostRequest, UpdateOutfitPostRequest } from '../entities/outfitpost.entity';

export const OUTFIT_POSTS_REPOSITORY = new InjectionToken<OutfitPostsRepository>('OutfitPostsRepository');

export interface OutfitPostsRepository {
  createOutfitPost(dto: CreateOutfitPostRequest): Observable<CommandResponse>;
  getOutfitPost(id: string): Observable<FeedPost>;
  getUserOutfitPosts(): Observable<FeedPost[]>;
  updateOutfitPost(id: string, dto: UpdateOutfitPostRequest): Observable<CommandResponse>;
  deleteOutfitPost(id: string): Observable<void>;
}
