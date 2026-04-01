import { createSelector } from '@ngrx/store';
import { SocialState, socialFeature } from './social.reducer';
import { ValidationPoll } from '../../../domain/entities/validation-poll.entity';

// Feature selector is already defined in socialFeature
export const {
  selectSocialState,
  selectPolls: selectAllPolls,
  selectSelectedPoll,
  selectTrendingOutfits,
  selectCommentsByVote,
  selectLoading: selectSocialLoading,
  selectError: selectSocialError
} = socialFeature;

export const selectActivePolls = createSelector(
  selectAllPolls,
  (polls: ValidationPoll[]): ValidationPoll[] =>
    polls.filter((poll) => poll.status === 'active'),
);

export const selectExpiredPolls = createSelector(
  selectAllPolls,
  (polls: ValidationPoll[]): ValidationPoll[] =>
    polls.filter((poll) => poll.status === 'expired'),
);

export const selectPollById = (pollId: string) =>
  createSelector(selectAllPolls, (polls: ValidationPoll[]): ValidationPoll | undefined =>
    polls.find((poll) => poll.id === pollId),
  );
