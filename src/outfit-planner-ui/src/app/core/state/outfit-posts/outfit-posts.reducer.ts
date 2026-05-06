import { createFeature, createReducer, on } from '@ngrx/store';
import { OutfitPostsActions } from './outfit-posts.actions';
import { FeedPost } from '../../../domain/entities/feed.entity';

export interface OutfitPostsState {
  userPosts: FeedPost[];
  loading: boolean;
  userPostsLoading: boolean;
  error: string | null;
}

export const initialState: OutfitPostsState = {
  userPosts: [],
  loading: false,
  userPostsLoading: false,
  error: null,
};

export const outfitPostsFeature = createFeature({
  name: 'outfitPosts',
  reducer: createReducer(
    initialState,

    // Create Outfit Post
    on(OutfitPostsActions.createOutfitPost, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OutfitPostsActions.createOutfitPostSuccess, (state) => ({
      ...state,
      loading: false,
    })),
    on(OutfitPostsActions.createOutfitPostFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Get Outfit Post
    on(OutfitPostsActions.getOutfitPost, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OutfitPostsActions.getOutfitPostSuccess, (state) => ({
      ...state,
      loading: false,
    })),
    on(OutfitPostsActions.getOutfitPostFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Load User Outfit Posts
    on(OutfitPostsActions.loadUserOutfitPosts, (state) => ({
      ...state,
      userPostsLoading: true,
      error: null,
    })),
    on(OutfitPostsActions.loadUserOutfitPostsSuccess, (state, { posts }) => ({
      ...state,
      userPosts: posts,
      userPostsLoading: false,
    })),
    on(OutfitPostsActions.loadUserOutfitPostsFailure, (state, { error }) => ({
      ...state,
      userPostsLoading: false,
      error,
    })),

    // Update Outfit Post
    on(OutfitPostsActions.updateOutfitPost, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OutfitPostsActions.updateOutfitPostSuccess, (state) => ({
      ...state,
      loading: false,
    })),
    on(OutfitPostsActions.updateOutfitPostFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Delete Outfit Post
    on(OutfitPostsActions.deleteOutfitPost, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(OutfitPostsActions.deleteOutfitPostSuccess, (state) => ({
      ...state,
      loading: false,
    })),
    on(OutfitPostsActions.deleteOutfitPostFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    }))
  ),
});

export const {
  name,
  reducer,
  selectOutfitPostsState,
  selectUserPosts,
  selectLoading,
  selectUserPostsLoading,
  selectError,
} = outfitPostsFeature;
