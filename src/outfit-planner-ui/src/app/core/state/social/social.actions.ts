import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  ValidationPoll,
  CreatePollRequest,
  CastVoteRequest,
} from '../../../domain/entities/validation-poll.entity';
import { AddVoteCommentRequest, VoteComment } from '../../../domain/entities/social-engagement.entity';

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
    'Load Trending': props<{ page?: number; pageSize?: number }>(),
    'Load Trending Success': props<{ outfits: any[]; totalCount: number }>(),
    'Load Trending Failure': props<{ error: string }>(),

    // Engagement
    'React To Vote': props<{ voteId: string; reactionType: string }>(),
    'React To Vote Success': props<{ voteId: string; reactionType: string }>(),
    'React To Vote Failure': props<{ error: string }>(),

    'Add Vote Comment': props<{ request: AddVoteCommentRequest }>(),
    'Add Vote Comment Success': props<{ comment: VoteComment }>(),
    'Add Vote Comment Failure': props<{ error: string }>(),

    'Load Vote Comments': props<{ voteId: string; maxDepth?: number }>(),
    'Load Vote Comments Success': props<{ voteId: string; comments: VoteComment[] }>(),
    'Load Vote Comments Failure': props<{ error: string }>(),

    'Like Vote Comment': props<{ commentId: string }>(),
    'Like Vote Comment Success': props<{ commentId: string }>(),
    'Like Vote Comment Failure': props<{ error: string }>(),
  },
});
