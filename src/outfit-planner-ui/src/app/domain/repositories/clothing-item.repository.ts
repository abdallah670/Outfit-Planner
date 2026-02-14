import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { ClothingItem } from '../entities/clothing-item.entity';

export const CLOTHING_ITEM_REPOSITORY = new InjectionToken<ClothingItemRepository>('ClothingItemRepository');

export interface ClothingItemRepository {
  getAll(): Observable<ClothingItem[]>;
  getById(id: string): Observable<ClothingItem>;
  create(item: Partial<ClothingItem>): Observable<ClothingItem>;
  update(id: string, item: Partial<ClothingItem>): Observable<ClothingItem>;
  delete(id: string): Observable<boolean>;
  getByType(type: string): Observable<ClothingItem[]>;
  recordWear(id: string): Observable<ClothingItem>;
}
