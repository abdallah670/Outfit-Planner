import { createFeature, createReducer, on } from '@ngrx/store';
import { SocialActions } from './social.actions';
import { ValidationPoll } from '../../../domain/entities/validation-poll.entity';
import { TrendingOutfit, OutfitComment } from '../../../domain/entities/social-engagement.entity';

export interface SocialState {
  polls: ValidationPoll[];
  selectedPoll: ValidationPoll | null;
  trendingOutfits: TrendingOutfit[];
  commentsByOutfit: { [outfitId: string]: OutfitComment[] };
  loading: boolean;
  error: string | null;
}

export const initialState: SocialState = {
  polls: [],
  selectedPoll: null,
  trendingOutfits: [],
  commentsByOutfit: {},
  loading: false,
  error: null,
};

export const socialFeature = createFeature({
  name: 'social',
  reducer: createReducer(
    initialState,

    // Load Polls
    on(SocialActions.loadPolls, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(SocialActions.loadPollsSuccess, (state, { polls }) => ({
      ...state,
      polls: polls || [],
      loading: false,
    })),
    on(SocialActions.loadPollsFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Load Poll By Id
    on(SocialActions.loadPollById, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(SocialActions.loadPollByIdSuccess, (state, { poll }) => ({
      ...state,
      selectedPoll: poll,
      loading: false,
    })),
    on(SocialActions.loadPollByIdFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Create Poll
    on(SocialActions.createPoll, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(SocialActions.createPollSuccess, (state) => ({
      ...state,
      loading: false,
    })),
    on(SocialActions.createPollFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Vote
    on(SocialActions.vote, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(SocialActions.voteSuccess, (state, { pollId }) => ({
      ...state,
      loading: false,
      // Update both the polls array and the selected poll
      polls: state.polls.map(p => 
        p.id === pollId ? { ...p, totalVotes: p.totalVotes + 1 } : p
      ),
      selectedPoll:
        state.selectedPoll?.id === pollId
          ? { ...state.selectedPoll, totalVotes: state.selectedPoll.totalVotes + 1 }
          : state.selectedPoll,
    })),
    on(SocialActions.voteFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),

    // Trending
    on(SocialActions.loadTrending, (state) => ({
      ...state,
      loading: true,
    })),
    on(SocialActions.loadTrendingSuccess, (state, { outfits }) => ({
      ...state,
      trendingOutfits: outfits,
      loading: false,
    })),
    on(SocialActions.loadTrendingFailure, (state, { error }) => ({
      ...state,
      error,
      loading: false,
    })),

    // Likes
    on(SocialActions.likeOutfitSuccess, (state, { outfitId }) => ({
      ...state,
      trendingOutfits: state.trendingOutfits.map(o => 
        o.id === outfitId ? { ...o, likes: o.likes + 1 } : o
      )
    })),
    on(SocialActions.unlikeOutfitSuccess, (state, { outfitId }) => ({
      ...state,
      trendingOutfits: state.trendingOutfits.map(o => 
        o.id === outfitId ? { ...o, likes: Math.max(0, o.likes - 1) } : o
      )
    })),

    // Comments
    on(SocialActions.loadCommentsSuccess, (state, { outfitId, comments }) => ({
      ...state,
      commentsByOutfit: {
        ...state.commentsByOutfit,
        [outfitId]: comments
      }
    })),
    on(SocialActions.addCommentSuccess, (state, { outfitId, comment }) => ({
      ...state,
      commentsByOutfit: {
        ...state.commentsByOutfit,
        [outfitId]: [comment, ...(state.commentsByOutfit[outfitId] || [])]
      },
      // Also update comment count in trending
      trendingOutfits: state.trendingOutfits.map(o => 
        o.id === outfitId ? { ...o, comments: o.comments + 1 } : o
      )
    })),
  ),
});

export const {
  name,
  reducer,
  selectSocialState,
  selectPolls,
  selectSelectedPoll,
  selectTrendingOutfits,
  selectCommentsByOutfit,
  selectLoading,
  selectError,
} = socialFeature;
