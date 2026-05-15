import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Outfit } from '../../domain/entities/outfit.entity';
import { Observable, map } from 'rxjs';
import { OutfitSuggestionsRequest, TodaysPickResponse } from '../../domain/repositories/outfit.repository';
import { PagedResult } from '../../domain/entities/response.entity';

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
  createOutfitWithImage(imageFile: File): Observable<Outfit> {
    const formData = new FormData();
    formData.append('image', imageFile);
    return this.http.post<Outfit>(`${this.apiUrl}/with-photo`, formData).pipe(map((o: Outfit) => this.fixOutfitUrls(o)));
  }

  getOutfitsSuggestions(request: OutfitSuggestionsRequest): Observable<Outfit[]> {
    return this.http
      .post<Outfit[]>(`${this.apiUrl}/suggestions`, request)
      .pipe(map((outfits: Outfit[]) => outfits.map((o: Outfit) => this.fixOutfitUrls(o))));
  }

  getTodaysPick(latitude?: number, longitude?: number, date?: string): Observable<TodaysPickResponse> {
    let params: any = {};
    if (latitude !== undefined && longitude !== undefined) {
      params = { lat: latitude.toString(), lon: longitude.toString() };
    }
    if (date) {
      params = { ...params, date };
    }
    return this.http
      .get<TodaysPickResponse>(`${this.apiUrl}/today`, { params })
      .pipe(
        map((response: TodaysPickResponse) => {
          if (response.outfit) {
            response.outfit = this.fixOutfitUrls(response.outfit);
          }
          return response;
        })
      );
  }

  recordOutfitWear(id: string): Observable<Outfit> {
    return this.http
      .post<Outfit>(`${this.apiUrl}/${id}/wear`, {})
      .pipe(map((o: Outfit) => this.fixOutfitUrls(o)));
  }

  getFilteredOutfits(
    filters: { occasion?: string; season?: string; search?: string; sortBy?: string },
    page: number,
    pageSize: number
  ): Observable<PagedResult<Outfit>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (filters.occasion) params = params.set('occasion', filters.occasion);
    if (filters.season) params = params.set('season', filters.season);
    if (filters.search) params = params.set('search', filters.search);
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);

    return this.http
      .get<PagedResult<Outfit>>(`${this.apiUrl}/filtered`, { params })
      .pipe(
        map((result: PagedResult<Outfit>) => ({
          ...result,
          items: result.items.map((o: Outfit) => this.fixOutfitUrls(o))
        }))
      );
  }

  private fixOutfitUrls(outfit: Outfit): Outfit {
    // Fix the main outfit image URL if present
    if (outfit.imageUrl) {
      outfit.imageUrl = this.fixOutfitImageUrl(outfit.imageUrl);
    }

    if (outfit.items) {
      outfit.items = outfit.items.map((item: any) => {
        if (item.clothingItemImageUrl) {
          item.clothingItemImageUrl = this.fixImageUrl(
            item.clothingItemImageUrl,
            item.clothingItemId,
          );
        }
        return item;
      });
    }
    return outfit;
  }

  /**
   * Fixes outfit image URL to be a full URL
   */
  private fixOutfitImageUrl(url: string): string {
    // If it's already a full URL, return as-is
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }

    // If it's a path starting with /uploads/, prepend backend base
    if (url.startsWith('/uploads/') || url.startsWith('uploads/')) {
      const path = url.startsWith('/') ? url : `/${url}`;
      return `${this.backendBase}${path}`;
    }

    // For any other relative path, prepend backend base
    const path = url.startsWith('/') ? url : `/${url}`;
    return `${this.backendBase}${path}`;
  }

  /**
   * Fixes image URL to be a full URL.
   * Handles various formats:
   * - Full URLs (http://...) - returned as-is
   * - Paths starting with /uploads/ - prepends backend base
   * - Simple filenames - attempts to construct full path using clothingItemId
   */
  private fixImageUrl(url: string, clothingItemId?: string): string {
    // If it's already a full URL, return as-is
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }

    // If it's already a path starting with /uploads/, prepend backend base
    if (url.startsWith('/uploads/') || url.startsWith('uploads/')) {
      const path = url.startsWith('/') ? url : `/${url}`;
      return `${this.backendBase}${path}`;
    }

    // If it's a simple filename and we have the clothingItemId,
    // we can't construct the full path without the userId.
    // The backend should return full paths. Log a warning.
    if (!url.includes('/')) {
      console.warn(
        `Simple filename detected without full path: ${url}. ` +
          `clothingItemId: ${clothingItemId}. ` +
          `Backend should return full paths like /uploads/{userId}/{clothingItemId}/{filename}`,
      );
      // Return as-is - it will fail to load and show fallback
      return url;
    }

    // For any other relative path, prepend backend base
    const path = url.startsWith('/') ? url : `/${url}`;
    return `${this.backendBase}${path}`;
  }
}
