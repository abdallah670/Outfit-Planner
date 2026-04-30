import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { OutfitPostsRepository, OUTFIT_POSTS_REPOSITORY } from '../../domain/repositories/outfit-posts.repository';
import { OutfitPostsDataSource, CreateOutfitPostRequest, UpdateOutfitPostRequest } from '../datasources/outfit-posts.datasource';
import { CommandResponse } from '../../domain/entities/response.entity';
import { FeedPost } from '../../domain/entities/feed.entity';

@Injectable({
  providedIn: 'root',
})
export class OutfitPostsRepositoryImpl implements OutfitPostsRepository {
  private dataSource = inject(OutfitPostsDataSource);

  createOutfitPost(dto: CreateOutfitPostRequest): Observable<CommandResponse> {
    return this.dataSource.createOutfitPost(dto);
  }

  getOutfitPost(id: string): Observable<FeedPost> {
    return this.dataSource.getOutfitPost(id);
  }

  updateOutfitPost(id: string, dto: UpdateOutfitPostRequest): Observable<CommandResponse> {
    return this.dataSource.updateOutfitPost(id, dto);
  }

  deleteOutfitPost(id: string): Observable<void> {
    return this.dataSource.deleteOutfitPost(id);
  }
}

export const outfitPostsRepositoryProvider = {
  provide: OUTFIT_POSTS_REPOSITORY,
  useClass: OutfitPostsRepositoryImpl,
};
