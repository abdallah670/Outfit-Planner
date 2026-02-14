import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ClothingItem } from '../../domain/entities/clothing-item.entity';

@Injectable({
  providedIn: 'root'
})
export class ClothingItemDataSource {
  private readonly apiUrl = 'api/wardrobe';

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<ClothingItem[]> {
    return this.http.get<ClothingItem[]>(`${this.apiUrl}/items`);
  }

  getById(id: string): Observable<ClothingItem> {
    return this.http.get<ClothingItem>(`${this.apiUrl}/items/${id}`);
  }

  create(item: FormData): Observable<ClothingItem> {
    return this.http.post<ClothingItem>(`${this.apiUrl}/items`, item);
  }

  update(id: string, item: Partial<ClothingItem>): Observable<ClothingItem> {
    return this.http.put<ClothingItem>(`${this.apiUrl}/items/${id}`, item);
  }

  delete(id: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/items/${id}`);
  }

  getByType(type: string): Observable<ClothingItem[]> {
    return this.http.get<ClothingItem[]>(`${this.apiUrl}/items`, { params: { type } });
  }

  recordWear(id: string): Observable<ClothingItem> {
    return this.http.put<ClothingItem>(`${this.apiUrl}/items/${id}/wear`, {});
  }
}
