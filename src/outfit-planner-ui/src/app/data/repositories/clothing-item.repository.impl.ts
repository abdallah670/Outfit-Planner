import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ClothingItem } from '../../domain/entities/clothing-item.entity';
import { ClothingItemRepository, CLOTHING_ITEM_REPOSITORY } from '../../domain/repositories/clothing-item.repository';
import { ClothingItemDataSource } from '../datasources/clothing-item.datasource';

@Injectable({
  providedIn: 'root'
})
export class ClothingItemRepositoryImpl implements ClothingItemRepository {
  constructor(private readonly dataSource: ClothingItemDataSource) {}

  getAll(): Observable<ClothingItem[]> {
    return this.dataSource.getAll();
  }

  getById(id: string): Observable<ClothingItem> {
    return this.dataSource.getById(id);
  }

  create(item: Partial<ClothingItem>): Observable<ClothingItem> {
    const formData = new FormData();
    Object.keys(item).forEach(key => {
      const value = item[key as keyof ClothingItem];
      if (value !== undefined && value !== null) {
        formData.append(key, value as string | Blob);
      }
    });
    return this.dataSource.create(formData);
  }

  update(id: string, item: Partial<ClothingItem>): Observable<ClothingItem> {
    return this.dataSource.update(id, item);
  }

  delete(id: string): Observable<boolean> {
    return this.dataSource.delete(id);
  }

  getByType(type: string): Observable<ClothingItem[]> {
    return this.dataSource.getByType(type);
  }

  recordWear(id: string): Observable<ClothingItem> {
    return this.dataSource.recordWear(id);
  }
}

export const clothingItemRepositoryProvider = {
  provide: CLOTHING_ITEM_REPOSITORY,
  useClass: ClothingItemRepositoryImpl
};
