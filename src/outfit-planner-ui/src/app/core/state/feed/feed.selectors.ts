import { createSelector } from '@ngrx/store';
import { feedFeature } from './feed.reducer';
import { FeedPost } from '../../../domain/entities/feed.entity';

export const {
  selectFeedState,
  selectPosts,
  selectNextCursor,
  selectHasMore,
  selectSelectedPost,
  selectCommentsByPost,
  selectLoading: selectFeedLoading,
  selectError: selectFeedError,
} = feedFeature;

export const selectPostById = (postId: string) =>
  createSelector(selectPosts, (posts: FeedPost[]): FeedPost | undefined =>
    posts.find(post => post.id === postId)
  );
export const selectPostByIdloading = (postId: string) =>
  createSelector(selectFeedLoading, (loading: boolean) => loading);

export const selectPostComments = (postId: string) =>
  createSelector(selectCommentsByPost, (commentsByPost: any) => commentsByPost[postId]?.items || []);
