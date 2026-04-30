
import { Outfit } from './outfit.entity';
import { Poll } from './poll.entity';
export enum PostType {
  OutfitPost = 0,
  PollPost = 1,
}

export enum Visibility {
  Public = 0,
  FriendsOnly = 1,
  Private = 2,
}

export interface FeedPost {
  id: string;
  userId: string;
  userName: string;
  userAvatarUrl: string;
  postType: PostType;
  outfitId?: string;
  outfit?: Outfit;
  pollId?: string;
  poll?: Poll;
  caption?: string;
  visibility: Visibility;
  likeCount: number;
  commentCount: number;
  userReaction?: string;
  createdAt: Date;
}

/**
 * Represents a comment on a feed post
 */
export interface PostComment {
  id: string;
  userId: string;
  userName: string;
  userAvatarUrl: string;
  content: string;
  createdAt: Date;
  isDeleted: boolean;
  parentCommentId?: string;
  replies?: PostComment[];
  likes?: string[]; // User IDs who liked the comment
}
/**
 * likes
 */
export interface PostLikes {
  id: string;
  userId: string;
  userName: string;
  userAvatarUrl: string;
  createdAt: Date;
}
export interface LikeRequest {
  postId: string;
  userId: string;
}
export interface UnlikeRequest {
  postId: string;
  userId: string;
}

