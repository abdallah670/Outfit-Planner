import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ClothingItem } from '../entities/clothing-item.entity';
import { ClothingItemRepository, CLOTHING_ITEM_REPOSITORY } from '../repositories/clothing-item.repository';

@Injectable({
  providedIn: 'root'
})
export class ClothingItemUseCases {
  constructor(@Inject(CLOTHING_ITEM_REPOSITORY) private readonly repository: ClothingItemRepository) {}

  getAllClothingItems(): Observable<ClothingItem[]> {
    return this.repository.getAll();
  }

  getClothingItemById(id: string): Observable<ClothingItem> {
    return this.repository.getById(id);
  }

  createClothingItem(item: Partial<ClothingItem>): Observable<ClothingItem> {
    return this.repository.create(item);
  }

  updateClothingItem(id: string, item: Partial<ClothingItem>): Observable<ClothingItem> {
    return this.repository.update(id, item);
  }

  deleteClothingItem(id: string): Observable<boolean> {
    return this.repository.delete(id);
  }

  getClothingItemsByType(type: string): Observable<ClothingItem[]> {
    return this.repository.getByType(type);
  }

  recordClothingItemWear(id: string): Observable<ClothingItem> {
    return this.repository.recordWear(id);
  }
}
