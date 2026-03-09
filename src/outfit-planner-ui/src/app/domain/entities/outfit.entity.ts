export interface Outfit {
  id: string;
  userId: string;
  name: string;
  items: OutfitItem[];
  occasion: OccasionType;
  suitableWeather: WeatherCondition;
  season: Season;
  comfortLevel: number;
  styleRating: number;
  createdAt: Date;
  lastWorn: Date;
  timesWorn: number;
  status: OutfitStatus;
  feedback: OutfitFeedback[];
}

export interface OutfitItem {
  id: string;
  outfitId: string;
  clothingItemId: string;
  clothingItemName: string;
  clothingItemImageUrl: string;
  clothingItemType: ClothingType;
  clothingItemCategory: string;
  role: 'primary' | 'secondary' | 'accent';
  layeringOrder: number;
  isEssential: boolean;
}

export enum ClothingType {
  Top = 'Top',
  Bottom = 'Bottom',
  Dress = 'Dress',
  Outerwear = 'Outerwear',
  Footwear = 'Footwear',
  Accessory = 'Accessory',
  Undergarment = 'Undergarment',
  Swimwear = 'Swimwear',
  Activewear = 'Activewear',
}

export interface WeatherCondition {
  temperature: number;
  condition: string;
  precipitationProbability: number;
  humidity: number;
  windSpeed: number;
}

export interface OutfitFeedback {
  id: string;
  outfitId: string;
  userId: string;
  rating: number;
  comment: string;
  createdAt: Date;
}

export enum OccasionType {
  Casual = 'Casual',
  BusinessCasual = 'BusinessCasual',
  Formal = 'Formal',
  Athletic = 'Athletic',
  Social = 'Social',
  Work = 'Work',
  Date = 'Date',
  Travel = 'Travel',
}

export enum Season {
  Spring = 'Spring',
  Summer = 'Summer',
  Autumn = 'Autumn',
  Winter = 'Winter',
  AllSeason = 'AllSeason',
}

export enum OutfitStatus {
  Active = 'active',
  Archived = 'archived',
  Favorite = 'favorite',
}
