import { createSelector, createFeatureSelector } from '@ngrx/store';
import { SocialState } from './social.reducer';
import { ValidationPoll } from '../../../domain/entities/validation-poll.entity';

export const selectSocialState = createFeatureSelector<SocialState>('social');

export const selectAllPolls = createSelector(
  selectSocialState,
  (state: SocialState): ValidationPoll[] => state?.polls || [],
);

export const selectSelectedPoll = createSelector(
  selectSocialState,
  (state: SocialState): ValidationPoll | null => state?.selectedPoll || null,
);

export const selectSocialLoading = createSelector(
  selectSocialState,
  (state: SocialState): boolean => !!state?.loading,
);

export const selectSocialError = createSelector(
  selectSocialState,
  (state: SocialState): string | null => state?.error || null,
);

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

export const selectTrendingOutfits = createSelector(
  selectSocialState,
  (state: SocialState) => state?.trendingOutfits || [],
);

export const selectCommentsByOutfit = (outfitId: string) =>
  createSelector(
    selectSocialState,
    (state: SocialState) => state?.commentsByOutfit[outfitId] || [],
  );

export const selectPollById = (pollId: string) =>
  createSelector(selectAllPolls, (polls: ValidationPoll[]): ValidationPoll | undefined =>
    polls.find((poll) => poll.id === pollId),
  );
