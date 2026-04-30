import { createSelector } from '@ngrx/store';
import { followFeature } from './follow.reducer';

export const {
  selectFollowState,
  selectFollowers,
  selectFollowing,
  selectFollowStats,
  selectLoading: selectFollowLoading,
  selectError: selectFollowError,
} = followFeature;

export const selectFollowersForUser = (userId: string) =>
  createSelector(selectFollowers, (followers) => followers[userId] || []);

export const selectFollowingForUser = (userId: string) =>
  createSelector(selectFollowing, (following) => following[userId] || []);

export const selectFollowStatsForUser = (userId: string) =>
  createSelector(selectFollowStats, (stats) => stats[userId] || null);

export const selectIsFollowingUser = (userId: string) =>
  createSelector(selectFollowStats, (stats) => stats[userId]?.isFollowing || false);
