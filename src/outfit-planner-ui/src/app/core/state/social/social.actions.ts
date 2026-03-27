import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  ValidationPoll,
  CreatePollRequest,
  CastVoteRequest,
} from '../../../domain/entities/validation-poll.entity';

export const SocialActions = createActionGroup({
  source: 'social',
  events: {
    // Load Polls
    'Load Polls': emptyProps(),
    'Load Polls Success': props<{ polls: ValidationPoll[] }>(),
    'Load Polls Failure': props<{ error: string }>(),

    // Load Poll By Id
    'Load Poll By Id': props<{ id: string }>(),
    'Load Poll By Id Success': props<{ poll: ValidationPoll }>(),
    'Load Poll By Id Failure': props<{ error: string }>(),

    // Create Poll
    'Create Poll': props<{ request: CreatePollRequest }>(),
    'Create Poll Success': props<{ pollId: string }>(),
    'Create Poll Failure': props<{ error: string }>(),

    // Vote
    'Vote': props<{ pollId: string; request: CastVoteRequest }>(),
    'Vote Success': props<{ pollId: string }>(),
    'Vote Failure': props<{ error: string }>(),

    // Trending
    'Load Trending': emptyProps(),
    'Load Trending Success': props<{ outfits: any[] }>(),
    'Load Trending Failure': props<{ error: string }>(),

    // Engagement
    'Like Outfit': props<{ outfitId: string }>(),
    'Like Outfit Success': props<{ outfitId: string }>(),
    'Like Outfit Failure': props<{ error: string }>(),

    'Unlike Outfit': props<{ outfitId: string }>(),
    'Unlike Outfit Success': props<{ outfitId: string }>(),
    'Unlike Outfit Failure': props<{ error: string }>(),

    'Add Comment': props<{ outfitId: string; content: string }>(),
    'Add Comment Success': props<{ outfitId: string; comment: any }>(),
    'Add Comment Failure': props<{ error: string }>(),

    'Load Comments': props<{ outfitId: string }>(),
    'Load Comments Success': props<{ outfitId: string; comments: any[] }>(),
    'Load Comments Failure': props<{ error: string }>(),
  },
});
