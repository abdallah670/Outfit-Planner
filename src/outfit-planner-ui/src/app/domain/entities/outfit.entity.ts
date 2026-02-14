export interface Outfit {
  id: string;
  userId: string;
  name: string;
  items: OutfitItem[];
  occasion: OccasionType;
  suitableWeather: WeatherCondition;
  suitableSeason: Season;
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
  role: 'primary' | 'secondary' | 'accent';
  layeringOrder: number;
  isEssential: boolean;
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
  Casual = 'casual',
  BusinessCasual = 'business_casual',
  Formal = 'formal',
  Athletic = 'athletic',
  Social = 'social',
  Work = 'work',
  Date = 'date',
  Travel = 'travel'
}

export enum Season {
  Spring = 'spring',
  Summer = 'summer',
  Autumn = 'autumn',
  Winter = 'winter'
}

export enum OutfitStatus {
  Active = 'active',
  Archived = 'archived',
  Favorite = 'favorite'
}
