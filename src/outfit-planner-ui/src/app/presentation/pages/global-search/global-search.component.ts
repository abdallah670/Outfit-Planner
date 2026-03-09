import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NavbarComponent } from '../../components/shared/navbar/navbar.component';

interface SearchResult {
  id: string;
  type: 'wardrobe' | 'outfit' | 'social';
  name: string;
  imageUrl: string;
  details: string;
}

interface OutfitSearchResult {
  id: string;
  name: string;
  imageUrl: string;
  tags: string[];
  items?: { name: string; imageUrl: string }[];
}

interface ItemSearchResult {
  id: string;
  name: string;
  imageUrl: string;
  brand: string;
  category: string;
}

interface RecentSearch {
  id: string;
  query: string;
  timestamp: Date;
}

@Component({
  selector: 'app-global-search',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatIconModule, MatButtonModule, NavbarComponent],
  templateUrl: './global-search.component.html',
  styleUrl: './global-search.component.scss'
})
export class GlobalSearchComponent implements OnInit {
  searchQuery = 'Summer Casual';
  activeFilter = 'all';
  
  // Filter signals
  selectedCategories = signal<string[]>(['all']);
  selectedSeasons = signal<string[]>(['summer']);
  selectedColor = signal<string>('white');
  
  // Recent searches
  recentSearches = signal<string[]>([
    'Office wear',
    'Blue jeans',
    'Winter jackets'
  ]);
  
  // Outfit results
  outfitResults = signal<OutfitSearchResult[]>([
    {
      id: '1',
      name: 'Weekend Summer Vibes',
      imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/a6233830-6211-4714-94b3-de5f6eea96d0.jpg',
      tags: ['Casual', 'Summer', 'Daytime'],
      items: [
        { name: 'White Sneakers', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/ac7af2a0-aaf5-4c5a-9d24-5f07577e0905.jpg' },
        { name: 'Denim Shorts', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/b37a94c8-07c8-4d6a-a77f-17ee073fbfb3.jpg' }
      ]
    },
    {
      id: '2',
      name: 'Beach Day Ready',
      imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/26585616-cb8c-4537-a58a-1f96490d4dc8.jpg',
      tags: ['Vacation', 'Summer'],
      items: [
        { name: 'Summer Sandals', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/1a5ce19a-3597-492c-902b-d3056c602c1e.jpg' },
        { name: 'Straw Hat', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/9c18fe15-9f97-450f-bce0-0d0fe839d35e.jpg' }
      ]
    },
    {
      id: '3',
      name: 'City Stroll',
      imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/221fa35c-5a4b-4765-b60d-fa190db59107.jpg',
      tags: ['Minimal', 'Summer'],
      items: [
        { name: 'Linen Shoes', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/6100ea92-8db6-4c1e-b932-3f9fb90f2875.jpg' },
        { name: 'Sunglasses', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/fedbcabf-2a71-4c73-a82a-a8f75942a942.jpg' }
      ]
    }
  ]);
  
  // Item results
  itemResults = signal<ItemSearchResult[]>([
    { id: '1', name: 'Basic White Tee', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/9be06a80-1d71-4278-89d6-8d1162f9bbb5.jpg', brand: 'Uniqlo', category: 'Tops' },
    { id: '2', name: 'Vintage Denim Shorts', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/08b426c9-dee0-4e3c-9ccb-7d6a2d81fa16.jpg', brand: "Levi's", category: 'Bottoms' },
    { id: '3', name: 'Floral Wrap Dress', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/c31fe2ba-f80a-425a-b337-b350bd7736f0.jpg', brand: 'Reformation', category: 'Dresses' },
    { id: '4', name: 'Straw Tote Bag', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/2cbbd65e-eef9-4eb4-8d05-ac34b8ee3f71.jpg', brand: 'H&M', category: 'Bags' },
    { id: '5', name: 'Blue Strappy Sandals', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/5cb744be-e0f7-4529-bddb-cb6e585e3ef8.jpg', brand: 'Zara', category: 'Shoes' }
  ]);

  ngOnInit(): void {
    // Initialize with search results
  }

  onSearch(): void {
    // In a real app, this would trigger an API call
    console.log('Searching for:', this.searchQuery);
  }

  setFilter(filter: string): void {
    this.activeFilter = filter;
  }

  getTotalResults(): number {
    return this.outfitResults().length + this.itemResults().length;
  }

  toggleCategory(category: string): void {
    const current = this.selectedCategories();
    if (current.includes(category)) {
      this.selectedCategories.set(current.filter(c => c !== category));
    } else {
      this.selectedCategories.set([...current, category]);
    }
  }

  toggleSeason(season: string): void {
    const current = this.selectedSeasons();
    if (current.includes(season)) {
      this.selectedSeasons.set(current.filter(s => s !== season));
    } else {
      this.selectedSeasons.set([...current, season]);
    }
  }

  selectColor(color: string): void {
    this.selectedColor.set(color);
  }

  clearFilters(): void {
    this.selectedCategories.set(['all']);
    this.selectedSeasons.set([]);
    this.selectedColor.set('');
  }

  selectSuggestion(suggestion: string): void {
    this.searchQuery = suggestion;
    this.onSearch();
  }
}
