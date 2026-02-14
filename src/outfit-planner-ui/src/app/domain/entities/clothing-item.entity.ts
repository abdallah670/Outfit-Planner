export interface ClothingItem {
  id: string;
  userId: string;
  name: string;
  type: ClothingType;
  category: string;
  primaryColor: string;
  secondaryColors: string[];
  fabric: string;
  brand: string;
  purchasePrice: number;
  purchaseDate: Date;
  condition: string;
  imageUrl: string;
  tags: ClothingTag[];
  lastWorn: Date;
  wearCount: number;
  isActive: boolean;
  createdAt: Date;
}

export interface ClothingTag {
  id: string;
  name: string;
  source: 'ai' | 'manual' | 'community';
  confidence: number;
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
  Activewear = 'activewear'
}
