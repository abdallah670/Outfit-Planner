import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { OUTFIT_POSTS_REPOSITORY, OutfitPostsRepository } from '../repositories/outfit-posts.repository';
import { CommandResponse } from '../entities/response.entity';
import { FeedPost } from '../entities/feed.entity';
import { CreateOutfitPostRequest, UpdateOutfitPostRequest } from '../entities/outfitpost.entity';

@Injectable({
  providedIn: 'root',
})
export class OutfitPostUseCases {
  constructor(
    @Inject(OUTFIT_POSTS_REPOSITORY) private readonly repository: OutfitPostsRepository,
  ) {}

  createOutfitPost(dto: CreateOutfitPostRequest): Observable<CommandResponse> {
    return this.repository.createOutfitPost(dto);
  }

  getOutfitPost(id: string): Observable<FeedPost> {
    return this.repository.getOutfitPost(id);
  }

  updateOutfitPost(id: string, dto: UpdateOutfitPostRequest): Observable<CommandResponse> {
    return this.repository.updateOutfitPost(id, dto);
  }

  deleteOutfitPost(id: string): Observable<void> {
    return this.repository.deleteOutfitPost(id);
  }

  getUserOutfitPosts(): Observable<FeedPost[]> {
    return this.repository.getUserOutfitPosts();
  }
}
