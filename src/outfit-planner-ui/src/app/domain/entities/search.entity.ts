export type SearchType = 'all' | 'outfits' | 'wardrobe' | 'social';

export interface SearchFilters {
  type: SearchType;
  categories: string[];
  seasons: string[];
  color: string | null;
}

export interface SearchResults {
  outfits: OutfitSearchResult[];
  wardrobeItems: WardrobeItemSearchResult[];
  totalCount: number;
  facets: SearchFacets;
}

export interface OutfitSearchResult {
  id: string;
  name: string;
  imageUrl?: string;
  tags: string[];
  occasion?: string;
  season?: string;
  relevanceScore: number;
}

export interface WardrobeItemSearchResult {
  id: string;
  name: string;
  imageUrl?: string;
  brand: string;
  category: string;
  primaryColor: string;
  relevanceScore: number;
}

export interface SearchFacets {
  categories: { name: string; count: number }[];
  seasons: { name: string; count: number }[];
  colors: { name: string; count: number }[];
}

export interface SearchState {
  query: string;
  filters: SearchFilters;
  results: SearchResults;
  recentSearches: string[];
  suggestions: string[];
  loading: boolean;
  error: string | null;
}

export const initialSearchFilters: SearchFilters = {
  type: 'all',
  categories: [],
  seasons: [],
  color: null,
};

export const initialSearchResults: SearchResults = {
  outfits: [],
  wardrobeItems: [],
  totalCount: 0,
  facets: {
    categories: [],
    seasons: [],
    colors: [],
  },
};

export const initialSearchState: SearchState = {
  query: '',
  filters: initialSearchFilters,
  results: initialSearchResults,
  recentSearches: [],
  suggestions: [],
  loading: false,
  error: null,
};
