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
  },
});
