import { createFeature, createReducer, on } from '@ngrx/store';
import { SocialActions } from './social.actions';
import { ValidationPoll } from '../../../domain/entities/validation-poll.entity';

export interface SocialState {
  polls: ValidationPoll[];
  selectedPoll: ValidationPoll | null;
  loading: boolean;
  error: string | null;
}

export const initialState: SocialState = {
  polls: [],
  selectedPoll: null,
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
      // Update the selected poll if it matches
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
  ),
});

export const {
  name,
  reducer,
  selectSocialState,
  selectPolls,
  selectSelectedPoll,
  selectLoading,
  selectError,
} = socialFeature;
