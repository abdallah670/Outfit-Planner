
import { Outfit } from './outfit.entity';
import { Poll, PollOption } from './poll.entity';
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
  Private = 0,
  Followers = 1,
  Public = 2,
}


export interface TaggedUser {
  userId: string;
  userName: string;
  profilePictureUrl?: string;
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
  taggedUsers?: TaggedUser[];
  visibility: Visibility;
  likesCount: number;
  commentsCount: number;
  userReaction?: string;
  createdAt: Date;
  isFollowing?:boolean;
  isOwner?:boolean;
  hasVoted?:boolean;
  isLiked:boolean;
}
export interface FeedPostWithComments extends FeedPost {
  comments: PostComment[];
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
  totalReplies?: number;
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

