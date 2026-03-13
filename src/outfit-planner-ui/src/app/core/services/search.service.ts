import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SearchResults, SearchFilters } from '../../domain/entities/search.entity';

interface SearchApiResponse {
  outfits: OutfitSearchApiResult[];
  wardrobeItems: WardrobeItemSearchApiResult[];
  totalResults: number;
  facets: { [key: string]: number };
}

interface OutfitSearchApiResult {
  id: string;
  name: string;
  imageUrl?: string;
  tags: string[];
  occasion?: string;
  season?: string;
  relevanceScore: number;
}

interface WardrobeItemSearchApiResult {
  id: string;
  name: string;
  imageUrl?: string;
  brand: string;
  category: string;
  primaryColor: string;
  relevanceScore: number;
}

@Injectable({
  providedIn: 'root',
})
export class SearchService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.baseUrl}/search`;

  search(query: string, filters?: SearchFilters, page: number = 1): Observable<SearchResults> {
    let params = new HttpParams()
      .set('q', query)
      .set('page', page.toString())
      .set('pageSize', '20');

    if (filters) {
      params = params.set('type', filters.type);
      
      if (filters.categories?.length) {
        params = params.set('categories', filters.categories.join(','));
      }
      
      if (filters.seasons?.length) {
        params = params.set('seasons', filters.seasons.join(','));
      }
      
      if (filters.color) {
        params = params.set('color', filters.color);
      }
    }

    return this.http.get<SearchApiResponse>(this.baseUrl, { params }).pipe(
      map((response: SearchApiResponse) => this.mapApiResponseToEntity(response))
    );
  }

  getSuggestions(partialQuery: string): Observable<string[]> {
    if (!partialQuery || partialQuery.length < 2) {
      return new Observable((observer) => {
        observer.next([]);
        observer.complete();
      });
    }

    const params = new HttpParams().set('q', partialQuery);
    return this.http.get<string[]>(`${this.baseUrl}/suggestions`, { params });
  }

  getRecentSearches(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/recent`);
  }

  saveRecentSearch(query: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/recent`, { query });
  }

  clearRecentSearches(): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/recent`);
  }

  private mapApiResponseToEntity(response: SearchApiResponse): SearchResults {
    return {
      outfits: response.outfits.map((o) => ({
        id: o.id,
        name: o.name,
        imageUrl: o.imageUrl,
        tags: o.tags,
        occasion: o.occasion,
        season: o.season,
        relevanceScore: o.relevanceScore,
      })),
      wardrobeItems: response.wardrobeItems.map((w) => ({
        id: w.id,
        name: w.name,
        imageUrl: w.imageUrl,
        brand: w.brand,
        category: w.category,
        primaryColor: w.primaryColor,
        relevanceScore: w.relevanceScore,
      })),
      totalCount: response.totalResults,
      facets: {
        categories: this.parseFacets(response.facets, 'category_'),
        seasons: this.parseFacets(response.facets, 'season_'),
        colors: this.parseFacets(response.facets, 'color_'),
      },
    };
  }

  private parseFacets(facets: { [key: string]: number }, prefix: string): { name: string; count: number }[] {
    return Object.entries(facets)
      .filter(([key]) => key.startsWith(prefix))
      .map(([key, count]) => ({
        name: key.replace(prefix, ''),
        count,
      }));
  }
}
