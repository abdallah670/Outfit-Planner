import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ClothingItem } from '../../domain/entities/clothing-item.entity';

@Injectable({
  providedIn: 'root',
})
export class WardrobeService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.baseUrl}/wardrobe`;

  getClothingItems(): Observable<ClothingItem[]> {
    return this.http
      .get<ClothingItem[]>(`${this.apiUrl}`)
      .pipe(map((items: ClothingItem[]) => items.map((item) => this.fixItemUrls(item))));
  }

  getClothingItemById(id: string): Observable<ClothingItem> {
    return this.http
      .get<ClothingItem>(`${this.apiUrl}/${id}`)
      .pipe(map((item: ClothingItem) => this.fixItemUrls(item)));
  }

  getClothingItemsByCategory(category: string): Observable<ClothingItem[]> {
    return this.http
      .get<ClothingItem[]>(`${this.apiUrl}/category/${category}`)
      .pipe(map((items: ClothingItem[]) => items.map((item) => this.fixItemUrls(item))));
  }

  createClothingItem(item: Partial<ClothingItem>, image?: File): Observable<ClothingItem> {
    const formData = new FormData();
    if (image) {
      formData.append('image', image);
    }
    Object.entries(item).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        if (Array.isArray(value)) {
          value.forEach((v) => formData.append(key, String(v)));
        } else {
          formData.append(key, String(value));
        }
      }
    });
    return this.http
      .post<ClothingItem>(`${this.apiUrl}`, formData)
      .pipe(map((item: ClothingItem) => this.fixItemUrls(item)));
  }

  updateClothingItem(
    id: string,
    item: Partial<ClothingItem>,
    image?: File,
  ): Observable<ClothingItem> {
    const formData = new FormData();
    if (image) {
      formData.append('image', image);
    }
    Object.entries(item).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        if (Array.isArray(value)) {
          value.forEach((v) => formData.append(key, String(v)));
        } else {
          formData.append(key, String(value));
        }
      }
    });
    return this.http
      .put<ClothingItem>(`${this.apiUrl}/${id}`, formData)
      .pipe(map((item: ClothingItem) => this.fixItemUrls(item)));
  }

  deleteClothingItem(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  recordWear(id: string): Observable<ClothingItem> {
    return this.http
      .post<ClothingItem>(`${this.apiUrl}/${id}/wear`, {})
      .pipe(map((item: ClothingItem) => this.fixItemUrls(item)));
  }

  private fixItemUrls(item: ClothingItem): ClothingItem {
    const backendBase = environment.baseUrl.replace('/api', '');
    if (
      item.imageUrl &&
      (item.imageUrl.startsWith('uploads/') || item.imageUrl.startsWith('/uploads/'))
    ) {
      const path = item.imageUrl.startsWith('/') ? item.imageUrl : `/${item.imageUrl}`;
      item.imageUrl = `${backendBase}${path}`;
    }
    if (
      item.thumbnailUrl &&
      (item.thumbnailUrl.startsWith('uploads/') || item.thumbnailUrl.startsWith('/uploads/'))
    ) {
      const path = item.thumbnailUrl.startsWith('/') ? item.thumbnailUrl : `/${item.thumbnailUrl}`;
      item.thumbnailUrl = `${backendBase}${path}`;
    }
    return item;
  }
}
