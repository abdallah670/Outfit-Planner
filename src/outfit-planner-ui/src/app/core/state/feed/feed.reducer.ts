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
        p.id === postId ? { ...p, userReaction: 'Heart' } : p
      ),
      selectedPost: state.selectedPost?.id === postId ? { ...state.selectedPost, userReaction: 'Heart' } : state.selectedPost,
    })),

    on(FeedActions.removeReactionSuccess, (state, { postId }) => ({
      ...state,
      posts: state.posts.map((p) =>
        p.id === postId ? { ...p, userReaction: undefined } : p
      ),
      selectedPost: state.selectedPost?.id === postId ? { ...state.selectedPost, userReaction: undefined } : state.selectedPost,
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
    })),
    on(OutfitPostsActions.createOutfitPostSuccess, (state, { post }) => ({
      ...state,
      posts: [post, ...state.posts],
    })),

    // Real-time SignalR handlers
    on(FeedActions.realtimePostReceived, (state, { post }) => ({
      ...state,
      posts: state.posts.some(p => p.id === post.id) ? state.posts : [post, ...state.posts],
    })),

    on(FeedActions.realtimeCommentUpdate, (state, { postId, count }) => ({
      ...state,
      posts: state.posts.map((p) =>
        p.id === postId ? { ...p, commentsCount: count } : p
      ),
    })),

    on(FeedActions.realtimeReactionUpdate, (state, { postId, count }) => ({
      ...state,
      posts: state.posts.map((p) =>
        p.id === postId ? { ...p, likesCount: count } : p
      ),
    })),

    on(FeedActions.realtimePollVoteUpdate, (state, { postId, totalVotes, optionVotes }) => {
      const updatePost = (p: FeedPost) => {
        if (p.id !== postId || !p.poll) return p;
        return {
          ...p,
          poll: {
            ...p.poll,
            totalVotes,
            options: p.poll.options.map(o => ({
              ...o,
              voteCount: optionVotes[o.id] ?? o.voteCount,
            })),
          },
        };
      };
      return {
        ...state,
        posts: state.posts.map(updatePost),
        selectedPost: state.selectedPost ? updatePost(state.selectedPost) : null,
      };
    }),

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
