import { createFeature, createReducer, on } from '@ngrx/store';
import { FeedActions } from './feed.actions';
import { OutfitPostsActions } from '../outfit-posts/outfit-posts.actions';
import { FeedPost, PostComment } from '../../../domain/entities/feed.entity';

export interface FeedState {
  posts: FeedPost[];
  nextCursor: string | null;
  hasMore: boolean;
  selectedPost: FeedPost | null;
  commentsByPost: { [postId: string]: { items: PostComment[]; nextCursor: string | null; hasMore: boolean } };
  loading: boolean;
  error: string | null;
}

export const initialState: FeedState = {
  posts: [],
  nextCursor: null,
  hasMore: false,
  selectedPost: null,
  commentsByPost: {},
  loading: false,
  error: null,
};

export const feedFeature = createFeature({
  name: 'feed',
  reducer: createReducer(
    initialState,

    on(FeedActions.loadPosts, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(FeedActions.loadPostsSuccess, (state, { result, append }) => ({
      ...state,
      posts: append ? [...state.posts, ...result.items] : result.items,
      nextCursor: result.nextCursor,
      hasMore: result.hasMore,
      loading: false,
    })),
    on(FeedActions.loadPostsFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    on(FeedActions.loadPostByIdSuccess, (state, { post }) => ({
      ...state,
      selectedPost: post,
    })),

    on(FeedActions.deletePostSuccess, (state, { id }) => ({
      ...state,
      posts: state.posts.filter((p) => p.id !== id),
    })),

    on(FeedActions.addReactionSuccess, (state, { postId }) => ({
      ...state,
      posts: state.posts.map((p) =>
        p.id === postId ? { ...p, likesCount: p.likesCount + 1, userReaction: 'Heart' } : p
      ),
    })),

    on(FeedActions.removeReactionSuccess, (state, { postId }) => ({
      ...state,
      posts: state.posts.map((p) =>
        p.id === postId ? { ...p, likesCount: Math.max(0, p.likesCount - 1), userReaction: undefined } : p
      ),
    })),

    on(FeedActions.loadCommentsSuccess, (state, { postId, result, append }) => ({
      ...state,
      commentsByPost: {
        ...state.commentsByPost,
        [postId]: {
          items: append ? [...(state.commentsByPost[postId]?.items || []), ...result.items] : result.items,
          nextCursor: result.nextCursor,
          hasMore: result.hasMore,
        },
      },
    })),

    on(FeedActions.addCommentSuccess, (state, { postId, comment }) => ({
      ...state,
      commentsByPost: {
        ...state.commentsByPost,
        [postId]: {
          ...state.commentsByPost[postId],
          items: [comment, ...(state.commentsByPost[postId]?.items || [])],
        },
      },
      posts: state.posts.map((p) =>
        p.id === postId ? { ...p, commentsCount: p.commentsCount + 1 } : p
      ),
    })),

    on(FeedActions.deleteCommentSuccess, (state, { commentId, postId }) => ({
      ...state,
      commentsByPost: {
        ...state.commentsByPost,
        [postId]: {
          ...state.commentsByPost[postId],
          items: (state.commentsByPost[postId]?.items || []).filter((c) => c.id !== commentId),
        },
      },
      posts: state.posts.map((p) =>
        p.id === postId ? { ...p, commentsCount: Math.max(0, p.commentsCount - 1) } : p
      ),
    })),
    on(OutfitPostsActions.createOutfitPostSuccess, (state, { post }) => ({
      ...state,
      posts: [post, ...state.posts],
    })),


  ),
});

export const {
  name,
  reducer,
  selectFeedState,
  selectPosts,
  selectNextCursor,
  selectHasMore,
  selectSelectedPost,
  selectCommentsByPost,
  selectLoading,
  selectError,
} = feedFeature;
