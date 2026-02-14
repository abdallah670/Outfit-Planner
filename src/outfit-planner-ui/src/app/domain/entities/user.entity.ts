export interface User {
  id: string;
  email: string;
  username: string;
  name: string;
  styleProfile: UserStyleProfile;
  preferences: UserPreferences;
  createdAt: Date;
  lastLogin: Date;
}

export interface UserStyleProfile {
  id: string;
  userId: string;
  style: StylePreference;
  preferredColors: string[];
  fitPreferences: string;
  comfortPriority: number;
  acceptsTrends: boolean;
}

export interface UserPreferences {
  id: string;
  userId: string;
  shareOutfitsAnonymously: boolean;
  includeInTrendAnalysis: boolean;
  allowFriendRequests: boolean;
  defaultOutfitPrivacy: PrivacyLevel;
  allowLocationTracking: boolean;
}

export enum StylePreference {
  Minimalist = 'minimalist',
  Classic = 'classic',
  Bohemian = 'bohemian',
  Streetwear = 'streetwear',
  Professional = 'professional',
  Athleisure = 'athleisure',
  Eclectic = 'eclectic',
  Vintage = 'vintage'
}

export enum PrivacyLevel {
  Private = 'private',
  Friends = 'friends',
  Community = 'community',
  Public = 'public'
}
