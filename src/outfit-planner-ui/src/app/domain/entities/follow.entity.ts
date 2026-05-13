
/**
 * Follow entities
 */

export interface Follower {
  id: string;
  userId: string;
  userName: string;
  userAvatarUrl: string;
  createdAt: Date;
  isFollowing: boolean;
}

export interface Following {
  id: string;
  userId: string;
  userName: string;
  userAvatarUrl: string;
  createdAt: Date;
  isFollowing: boolean;
}
export interface FollowStats {
  followersCount: number;
  followingCount: number;
  isFollowing: boolean;
}
export interface FollowRequest {
  followerId: string;
  followedId: string;
}
export interface UnfollowRequest {
  followerId: string;
  followedId: string;
}
export type IsFollowing = boolean;


