import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ClothingItem } from '../../domain/entities/clothing-item.entity';
import { PagedResult } from '../../domain/entities/response.entity';

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
      .post<ClothingItem>(`${this.apiUrl}/${id}/wear/quick`, {})
      .pipe(map((item: ClothingItem) => this.fixItemUrls(item)));
  }

  getFilteredItems(
    filters: { 
      category?: string;         // e.g. "Casual", "Formal", "Sport"
      color?: string;            // color name: "Blue", "Red", "Black", etc.
      condition?: string;        // "good", "excellent", "fair", "poor"
      fabric?: string;           // "Cotton", "Polyester", etc.
      type?: string;             // Clothing type: "Top", "Bottom", "Dress", etc.
      size?: string;             // "M", "L", "XL", etc.
      minPrice?: number | null; 
      maxPrice?: number | null; 
      search?: string 
    },
    page: number,
    pageSize: number
  ): Observable<PagedResult<ClothingItem>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (filters.category) params = params.set('category', filters.category);
    if (filters.color) params = params.set('color', filters.color);
    if (filters.condition) params = params.set('condition', filters.condition);
    if (filters.fabric) params = params.set('fabric', filters.fabric);
    if (filters.type) params = params.set('type', filters.type);
    if (filters.size) params = params.set('size', filters.size);
    if (filters.minPrice !== null && filters.minPrice !== undefined) params = params.set('minPrice', filters.minPrice.toString());
    if (filters.maxPrice !== null && filters.maxPrice !== undefined) params = params.set('maxPrice', filters.maxPrice.toString());
    if (filters.search) params = params.set('search', filters.search);

    return this.http
      .get<PagedResult<ClothingItem>>(`${this.apiUrl}/filtered`, { params })
      .pipe(
        map((result: PagedResult<ClothingItem>) => ({
          ...result,
          items: result.items.map((item) => this.fixItemUrls(item))
        }))
      );
  }

  private fixItemUrls(item: ClothingItem): ClothingItem {
    const backendBase = environment.baseUrl.replace('/api', '');
    
    if (item.imageUrl) {
      item.imageUrl = this.fixImageUrl(item.imageUrl, backendBase, item.id);
    }
    if (item.thumbnailUrl) {
      item.thumbnailUrl = this.fixImageUrl(item.thumbnailUrl, backendBase, item.id);
    }
    
    return item;
  }

  /**
   * Fixes image URL to be a full URL.
   * Handles various formats:
   * - Full URLs (http://...) - returned as-is
   * - Paths starting with /uploads/ - prepends backend base
   * - Simple filenames - logs warning as backend should return full paths
   */
  private fixImageUrl(url: string, backendBase: string, itemId?: string): string {
    // If it's already a full URL, return as-is
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }

    // If it's already a path starting with /uploads/, prepend backend base
    if (url.startsWith('/uploads/') || url.startsWith('uploads/')) {
      const path = url.startsWith('/') ? url : `/${url}`;
      return `${backendBase}${path}`;
    }

    // If it's a simple filename without path separators,
    // the backend should return full paths. Log a warning.
    if (!url.includes('/')) {
      console.warn(
        `Simple filename detected without full path: ${url}. ` +
        `itemId: ${itemId}. ` +
        `Backend should return full paths like /uploads/{userId}/{itemId}/{filename}`
      );
      // Return as-is - it will fail to load and show fallback
      return url;
    }

    // For any other relative path, prepend backend base
    const path = url.startsWith('/') ? url : `/${url}`;
    return `${backendBase}${path}`;
  }
}
