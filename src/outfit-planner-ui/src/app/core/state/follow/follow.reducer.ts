import { createFeature, createReducer, on } from '@ngrx/store';
import { FollowActions } from './follow.actions';
import { Follower, Following, FollowStats } from '../../../domain/entities/follow.entity';

export interface FollowState {
  followers: Record<string, Follower[]>; // Map of userId to their followers
  following: Record<string, Following[]>; // Map of userId to people they follow
  followStats: Record<string, FollowStats>;
  loading: boolean;
  error: string | null;
}

export const initialState: FollowState = {
  followers: {},
  following: {},
  followStats: {},
  loading: false,
  error: null,
};

export const followFeature = createFeature({
  name: 'follow',
  reducer: createReducer(
    initialState,
    
    // Follow / Unfollow
    on(FollowActions.followUser, FollowActions.unfollowUser, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(FollowActions.followUserSuccess, (state, { userId }) => {
      // Optimistic update logic could go here, or we invalidate stats
      return {
        ...state,
        loading: false,
      };
    }),
    on(FollowActions.unfollowUserSuccess, (state, { userId }) => {
      // Optimistic update logic
      return {
        ...state,
        loading: false,
      };
    }),
    on(FollowActions.followUserFailure, FollowActions.unfollowUserFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Followers
    on(FollowActions.loadFollowers, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(FollowActions.loadFollowersSuccess, (state, { userId, result, append }) => {
      const currentFollowers = state.followers[userId] || [];
      const newFollowers = append ? [...currentFollowers, ...result.items] : result.items;
      return {
        ...state,
        followers: {
          ...state.followers,
          [userId]: newFollowers
        },
        loading: false,
      };
    }),
    on(FollowActions.loadFollowersFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Following
    on(FollowActions.loadFollowing, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(FollowActions.loadFollowingSuccess, (state, { userId, result, append }) => {
      const currentFollowing = state.following[userId] || [];
      const newFollowing = append ? [...currentFollowing, ...result.items] : result.items;
      return {
        ...state,
        following: {
          ...state.following,
          [userId]: newFollowing
        },
        loading: false,
      };
    }),
    on(FollowActions.loadFollowingFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Stats
    on(FollowActions.loadFollowStats, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(FollowActions.loadFollowStatsSuccess, (state, { userId, stats }) => ({
      ...state,
      followStats: {
        ...state.followStats,
        [userId]: stats
      },
      loading: false,
    })),
    on(FollowActions.loadFollowStatsFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),
    
    // Check Status
    on(FollowActions.checkFollowStatusSuccess, (state, { userId, isFollowing }) => {
      const currentStats = state.followStats[userId] || { followersCount: 0, followingCount: 0, isFollowing: false };
      return {
        ...state,
        followStats: {
          ...state.followStats,
          [userId]: {
            ...currentStats,
            isFollowing
          }
        }
      };
    })
  ),
});

export const {
  name,
  reducer,
  selectFollowState,
  selectFollowers,
  selectFollowing,
  selectFollowStats,
  selectLoading,
  selectError,
} = followFeature;
