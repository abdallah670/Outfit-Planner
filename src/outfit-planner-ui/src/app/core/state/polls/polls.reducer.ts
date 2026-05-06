import { createFeature, createReducer, on } from '@ngrx/store';
import { PollsActions } from './polls.actions';
import { Poll } from '../../../domain/entities/poll.entity';

export interface PollsState {
  polls: Poll[];
  userPolls: Poll[];
  selectedPoll: Poll | null;
  recentPoll: Poll | null;
  recentPollComments: any[];
  commentsCursor: string | null;
  hasMoreComments: boolean;
  commentsLoading: boolean;
  loading: boolean;
  userPollsLoading: boolean;
  error: string | null;
}

export const initialState: PollsState = {
  polls: [],
  userPolls: [],
  selectedPoll: null,
  recentPoll: null,
  recentPollComments: [],
  commentsCursor: null,
  hasMoreComments: false,
  commentsLoading: false,
  loading: false,
  userPollsLoading: false,
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
    on(PollsActions.loadUserPolls, (state) => ({
      ...state,
      userPollsLoading: true,
      error: null,
    })),
    on(PollsActions.loadUserPollsSuccess, (state, { polls }) => ({
      ...state,
      userPolls: polls,
      userPollsLoading: false,
    })),
    on(PollsActions.loadUserPollsFailure, (state, { error }) => ({
      ...state,
      userPollsLoading: false,
      error,
    })),
    on(PollsActions.updatePollSuccess, (state, { poll }) => ({
      ...state,
      polls: state.polls.map(p => p.id === poll.id ? poll : p),
      userPolls: state.userPolls.map(p => p.id === poll.id ? poll : p),
      selectedPoll: state.selectedPoll?.id === poll.id ? poll : state.selectedPoll,
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
    })),
    on(PollsActions.loadRecentPoll, (state) => ({
      ...state,
      loading: true,
      error: null
    })),
    on(PollsActions.loadRecentPollSuccess, (state, { poll, comments, commentsCursor, hasMoreComments }) => ({
      ...state,
      recentPoll: poll,
      recentPollComments: comments,
      commentsCursor: commentsCursor ?? null,
      hasMoreComments: hasMoreComments ?? false,
      loading: false
    })),
    on(PollsActions.loadRecentPollFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error
    })),
    on(PollsActions.loadMorePollComments, (state) => ({
      ...state,
      commentsLoading: true,
    })),
    on(PollsActions.loadMorePollCommentsSuccess, (state, { comments, commentsCursor, hasMoreComments }) => ({
      ...state,
      recentPollComments: [...state.recentPollComments, ...comments],
      commentsCursor: commentsCursor ?? null,
      hasMoreComments: hasMoreComments ?? false,
      commentsLoading: false,
    })),
    on(PollsActions.loadMorePollCommentsFailure, (state, { error }) => ({
      ...state,
      commentsLoading: false,
      error
    }))
  ),
});

export const {
  name,
  reducer,
  selectPollsState,
  selectPolls,
  selectSelectedPoll,
  selectRecentPoll,
  selectRecentPollComments,
  selectCommentsCursor,
  selectHasMoreComments,
  selectCommentsLoading,
  selectLoading,
  selectError,
  selectUserPolls: selectUserPollsState,
  selectUserPollsLoading,
} = pollsFeature;
