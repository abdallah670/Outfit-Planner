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
  occasion: string;
  trendingScore: number;
  createdAt: Date;
}

/**
 * Represents engagement metrics for an outfit
 */
export interface OutfitEngagement {
  outfitId: string;
  likeCount: number;
  commentCount: number;
  userHasLiked: boolean;
}

/**
 * Represents a comment on an outfit
 */
export interface OutfitComment {
  id: string;
  outfitId: string;
  userId: string;
  userName: string;
  userAvatarUrl: string;
  content: string;
  createdAt: Date;
  isDeleted: boolean;
  replies?: OutfitComment[];
}

/**
 * DTO for adding a comment
 */
export interface AddCommentRequest {
  outfitId: string;
  content: string;
}
