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
  voteId: string;
  createdAt: Date;
}

/**
 * Represents a reaction to a vote or comment
 */
export interface VoteReaction {
  userId: string;
  reactionType: 'Like' | 'Love' | 'Insightful' | string;
}

/**
 * Represents a comment on a vote
 */
export interface VoteComment {
  id: string;
  voteId: string;
  userId: string;
  userName: string;
  userAvatarUrl?: string;
  content: string;
  createdAt: Date;
  isDeleted: boolean;
  parentCommentId?: string;
  replies?: VoteComment[];
  likes?: string[]; // User IDs who liked the comment
}

/**
 * DTO for adding a comment to a vote
 */
export interface AddVoteCommentRequest {
  voteId: string;
  content: string;
  parentCommentId?: string;
}

/**
 * Represents engagement metrics for an outfit (connected via its auto-vote)
 */
export interface OutfitEngagement {
  outfitId: string;
  voteCount: number;
  commentCount: number;
  reactionCount: number;
  userHasVoted: boolean;
  userReaction?: string;
}
