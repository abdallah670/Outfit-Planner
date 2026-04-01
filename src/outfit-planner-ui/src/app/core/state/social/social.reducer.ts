import { createFeature, createReducer, on } from '@ngrx/store';
import { SocialActions } from './social.actions';
import { ValidationPoll } from '../../../domain/entities/validation-poll.entity';
import { TrendingOutfit, VoteComment } from '../../../domain/entities/social-engagement.entity';

export interface SocialState {
  polls: ValidationPoll[];
  selectedPoll: ValidationPoll | null;
  trendingOutfits: TrendingOutfit[];
  commentsByVote: { [voteId: string]: VoteComment[] };
  loading: boolean;
  error: string | null;
}

export const initialState: SocialState = {
  polls: [],
  selectedPoll: null,
  trendingOutfits: [],
  commentsByVote: {},
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

    // Likes / Reactions
    on(SocialActions.reactToVoteSuccess, (state, { voteId, reactionType }) => ({
      ...state,
      // Update reaction in trending list if applicable
      trendingOutfits: state.trendingOutfits.map(o => 
        // Note: For now we don't know the voteId -> outfitId mapping easily here 
        // without fetching trending again or having it in state.
        // Assuming the UI will handle showing the immediate reaction locally.
        o
      )
    })),

    // Comments
    on(SocialActions.loadVoteCommentsSuccess, (state, { voteId, comments }) => ({
      ...state,
      commentsByVote: {
        ...state.commentsByVote,
        [voteId]: comments
      }
    })),
    on(SocialActions.addVoteCommentSuccess, (state, { comment }) => ({
      ...state,
      commentsByVote: {
        ...state.commentsByVote,
        [comment.voteId]: [comment, ...(state.commentsByVote[comment.voteId] || [])]
      }
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
  selectCommentsByVote,
  selectLoading,
  selectError,
} = socialFeature;
