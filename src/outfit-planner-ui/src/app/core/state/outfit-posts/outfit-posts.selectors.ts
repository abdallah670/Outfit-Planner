import { outfitPostsFeature } from './outfit-posts.reducer';

export const {
  selectOutfitPostsState,
  selectLoading: selectOutfitPostsLoading,
  selectError: selectOutfitPostsError,
} = outfitPostsFeature;
