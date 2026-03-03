export interface UserProfile {
  id: string;
  name: string;
  email: string;
  profilePictureUrl?: string;
  createdAt: string;
  lastLogin?: string;
  
  // Stats
  wardrobeItemCount: number;
  outfitCount: number;
  totalWears: number;
  
  // Style Profile
  styleProfile?: UserStyleProfile;
  
  // Preferences
  preferences?: UserPreferences;
}

export interface UserStyleProfile {
  style: StylePreference;
  preferredColors: string[];
  fitPreferences: string;
  comfortPriority: number;
  acceptsTrends: boolean;
}

export interface UserPreferences {
  shareOutfitsAnonymously: boolean;
  includeInTrendAnalysis: boolean;
  allowFriendRequests: boolean;
  defaultOutfitPrivacy: PrivacyLevel;
  showBodyMetrics: boolean;
  allowLocationTracking: boolean;
}

export enum StylePreference {
  Casual = 'Casual',
  Formal = 'Formal',
  Sporty = 'Sporty',
  Bohemian = 'Bohemian',
  Minimalist = 'Minimalist',
  Vintage = 'Vintage',
  Streetwear = 'Streetwear',
  Preppy = 'Preppy',
  Chic = 'Chic',
  Business = 'Business'
}

export enum PrivacyLevel {
  Private = 'Private',
  Friends = 'Friends',
  Public = 'Public'
}

export interface UpdateUserProfileRequest {
  name: string;
  styleProfile?: UserStyleProfile;
  preferences?: UserPreferences;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
