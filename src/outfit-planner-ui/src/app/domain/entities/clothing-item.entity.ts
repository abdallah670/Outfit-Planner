import { OccasionType, WeatherCondition } from './outfit.entity';

export interface ClothingItem {
  id: string;
  userId: string;
  name: string;
  type: string;
  category: string;
  primaryColor: string;
  secondaryColors: string[];
  description?: string;
  fabric: string;
  brand: string;
  purchasePrice: number;
  currency: string;
  purchaseDate?: string | Date;
  size: string;
  condition: string;
  imageUrl: string;
  thumbnailUrl?: string;
  tags: ClothingTag[];
  lastWorn?: string | Date;
  wearCount: number;
  isActive: boolean;
  createdAt: string | Date;
}

export interface ClothingTag {
  id: string;
  name: string;
  source: 'ai' | 'manual' | 'community';
  confidence: number;
}
export interface RecordWear {
  id: string;
  userId: string;
  clothingItemId: string;
  date: Date;
  location: string;
  weather: WeatherCondition;
  occasion: OccasionType;
  outfitId: string;
  rating: number;
  feedback: string;
}

export enum ClothingType {
  Top = 'top',
  Bottom = 'bottom',
  Dress = 'dress',
  Outerwear = 'outerwear',
  Footwear = 'footwear',
  Accessory = 'accessory',
  Undergarment = 'undergarment',
  Swimwear = 'swimwear',
  Activewear = 'activewear',
}
