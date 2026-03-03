import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Outfit } from '../../domain/entities/outfit.entity';
import { Observable, map } from 'rxjs';
import { OutfitSuggestionsRequest } from '../../domain/repositories/outfit.repository';

@Injectable({
  providedIn: 'root',
})
export class OutfitDataSource {
  private readonly apiUrl = `${environment.baseUrl}/outfits`;
  private readonly backendBase = environment.baseUrl.replace('/api', '');

  constructor(private http: HttpClient) {}

  getAllOutfits(): Observable<Outfit[]> {
    return this.http
      .get<Outfit[]>(this.apiUrl)
      .pipe(map((outfits: Outfit[]) => outfits.map((o: Outfit) => this.fixOutfitUrls(o))));
  }

  getOutfitById(id: string): Observable<Outfit> {
    return this.http
      .get<Outfit>(`${this.apiUrl}/${id}`)
      .pipe(map((o: Outfit) => this.fixOutfitUrls(o)));
  }

  createOutfit(outfit: Partial<Outfit>): Observable<Outfit> {
    return this.http
      .post<Outfit>(this.apiUrl, outfit)
      .pipe(map((o: Outfit) => this.fixOutfitUrls(o)));
  }

  updateOutfit(id: string, outfit: Partial<Outfit>): Observable<Outfit> {
    return this.http
      .put<Outfit>(`${this.apiUrl}/${id}`, outfit)
      .pipe(map((o: Outfit) => this.fixOutfitUrls(o)));
  }

  deleteOutfit(id: string): Observable<boolean> {
    return this.http.delete(`${this.apiUrl}/${id}`, { responseType: 'text' }).pipe(map(() => true));
  }

  getOutfitsSuggestions(request: OutfitSuggestionsRequest): Observable<Outfit[]> {
    return this.http
      .post<Outfit[]>(`${this.apiUrl}/suggestions`, request)
      .pipe(map((outfits: Outfit[]) => outfits.map((o: Outfit) => this.fixOutfitUrls(o))));
  }

  getTodaysOutfit(): Observable<Outfit> {
    return this.http
      .get<Outfit>(`${this.apiUrl}/today`)
      .pipe(map((o: Outfit) => this.fixOutfitUrls(o)));
  }

  recordOutfitWear(id: string): Observable<Outfit> {
    return this.http
      .post<Outfit>(`${this.apiUrl}/${id}/wear`, {})
      .pipe(map((o: Outfit) => this.fixOutfitUrls(o)));
  }

  private fixOutfitUrls(outfit: Outfit): Outfit {
    if (outfit.items) {
      outfit.items = outfit.items.map((item: any) => {
        if (
          item.clothingItemImageUrl &&
          (item.clothingItemImageUrl.startsWith('uploads/') ||
            item.clothingItemImageUrl.startsWith('/uploads/'))
        ) {
          const path = item.clothingItemImageUrl.startsWith('/')
            ? item.clothingItemImageUrl
            : `/${item.clothingItemImageUrl}`;
          item.clothingItemImageUrl = `${this.backendBase}${path}`;
        }
        return item;
      });
    }
    return outfit;
  }
}
