import { outfitPostsFeature } from './outfit-posts.reducer';

export const {
  selectOutfitPostsState,
  selectUserPosts,
  selectLoading: selectOutfitPostsLoading,
  selectUserPostsLoading,
  selectError: selectOutfitPostsError,
} = outfitPostsFeature;