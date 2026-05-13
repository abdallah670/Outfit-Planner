
import { Outfit } from './outfit.entity';
import { Poll } from './poll.entity';
/**
 * IMPORTANT: These values must match the C# backend PostType enum:
 * - Poll = 0 (C# first item default)
 * - Outfit = 1 (C# second item default)
 */
export enum PostType {
  Poll = 0,
  Outfit = 1,
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
  tags?: string[];
  visibility: Visibility;
  likesCount: number;
  commentsCount: number;
  userReaction?: string;
  createdAt: Date;
  isfollowing?:boolean;
  isowner?:boolean;
  hasvoted?:boolean;
  uservote:string;
  isliked:boolean;
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

