export interface Outfit {
  id: string;
  userId: string;
  name: string;
  imageUrl?: string; // URL to the generated outfit preview image
  items: OutfitItem[];
  occasion: OccasionType;
  suitableWeather: WeatherCondition;
  season: Season;
  createdAt: Date;
  lastWorn: Date;
  timesWorn: number;
  status: OutfitStatus;
  feedback: OutfitFeedback[];
  commentsCount?: number;
  likesCount? : number;
  rank?:number;
  feedPostId?:string;
  postType?:string;
  score?:number;
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

/**
 * Clothing Item entity for wardrobe
 */
export interface ClothingItem {
  id: string;
  name: string;
  type: ClothingType;
  category?: string;
  color?: string;
  imageUrl?: string;
  userId?: string;
  createdAt?: Date;
}



/**
 * Represents a trending outfit for the social feed
 */
export interface TrendingOutfit {
  id: string;
  userId: string;
  userName: string;
  userAvatar: string;
  imageUrl: string;
  likes: number;
  comments: number;
  trendingScore: number;
  createdAt: Date;
  isfollowing?:boolean;
  isowner?:boolean;
  isliked:boolean;

}
