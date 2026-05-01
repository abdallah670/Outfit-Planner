import { createFeature, createReducer, on } from '@ngrx/store';
import { PollsActions } from './polls.actions';
import { Poll } from '../../../domain/entities/poll.entity';

export interface PollsState {
  polls: Poll[];
  selectedPoll: Poll | null;
  recentPoll: Poll | null;
  recentPollComments: any[];
  commentsCursor: string | null;
  hasMoreComments: boolean;
  commentsLoading: boolean;
  loading: boolean;
  error: string | null;
}

export const initialState: PollsState = {
  polls: [],
  selectedPoll: null,
  recentPoll: null,
  recentPollComments: [],
  commentsCursor: null,
  hasMoreComments: false,
  commentsLoading: false,
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
} = pollsFeature;
