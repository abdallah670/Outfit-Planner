/**
 * Public user profile DTO (matches backend PublicUserProfileDto)
 */
export interface PublicUserProfile {
  id: string;
  name: string;
  username: string; // handle (lowercase to match [JsonPropertyName("username")] in backend)
  profilePictureUrl?: string;
  bio?: string;
  createdAt: Date;

  // Stats
  outfitCount: number;
  wardrobeItemCount: number;
  totalWears: number;
  followersCount: number;
  followingCount: number;
  isFollowing: boolean;
  isOwner: boolean;
  

  // Style profile (optional)
  styleProfile?: {
    style: number; // StylePreference enum value
    preferredColors: string[];
    fitPreferences?: string;
    comfortPriority: number;
    acceptsTrends: boolean;
  };
}
