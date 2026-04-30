import { createFeature, createReducer, on } from '@ngrx/store';
import { PollsActions } from './polls.actions';
import { Poll } from '../../../domain/entities/poll.entity';

export interface PollsState {
  polls: Poll[];
  selectedPoll: Poll | null;
  loading: boolean;
  error: string | null;
}

export const initialState: PollsState = {
  polls: [],
  selectedPoll: null,
  loading: false,
  error: null,
};

export const pollsFeature = createFeature({
  name: 'polls',
  reducer: createReducer(
    initialState,
    on(PollsActions.loadPolls, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(PollsActions.loadPollsSuccess, (state, { polls }) => ({
      ...state,
      polls,
      loading: false,
    })),
    on(PollsActions.loadPollsFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),
    on(PollsActions.loadPollByIdSuccess, (state, { poll }) => ({
      ...state,
      selectedPoll: poll,
    })),
    on(PollsActions.voteSuccess, (state, { pollId }) => ({
      ...state,
      // Optimistic update or reload could happen here
    })),
    on(PollsActions.deletePollSuccess, (state, { pollId }) => ({
      ...state,
      polls: state.polls.filter(p => p.id !== pollId)
    }))
  ),
});

export const {
  name,
  reducer,
  selectPollsState,
  selectPolls,
  selectSelectedPoll,
  selectLoading,
  selectError,
} = pollsFeature;
