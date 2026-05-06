import { createSelector } from '@ngrx/store';
import { pollsFeature } from './polls.reducer';

export const {
  selectPollsState,
  selectPolls,
  selectUserPolls,
  selectSelectedPoll,
  selectRecentPoll,
  selectRecentPollComments,
  selectCommentsCursor,
  selectHasMoreComments,
  selectCommentsLoading,
  selectLoading: selectPollsLoading,
  selectUserPollsLoading,
  selectError: selectPollsError,
} = pollsFeature;

export const selectPollById = (pollId: string) =>
  createSelector(selectPolls, (polls) => polls.find(p => p.id === pollId));
