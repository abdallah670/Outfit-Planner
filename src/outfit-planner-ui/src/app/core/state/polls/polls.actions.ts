import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Poll, CreatePollRequest, UpdatePollRequest, CastVoteRequest } from '../../../domain/entities/poll.entity';

export const PollsActions = createActionGroup({
  source: 'polls',
  events: {
    'Load Polls': emptyProps(),
    'Load Polls Success': props<{ polls: Poll[] }>(),
    'Load Polls Failure': props<{ error: string }>(),

    'Load Poll By Id': props<{ id: string }>(),
    'Load Poll By Id Success': props<{ poll: Poll }>(),
    'Load Poll By Id Failure': props<{ error: string }>(),

    'Load User Polls': emptyProps(),
    'Load User Polls Success': props<{ polls: Poll[] }>(),
    'Load User Polls Failure': props<{ error: string }>(),

    'Create Poll': props<{ request: CreatePollRequest }>(),
    'Create Poll Success': props<{ pollId: string }>(),
    'Create Poll Failure': props<{ error: string }>(),

    'Update Poll': props<{ pollId: string; request: UpdatePollRequest }>(),
    'Update Poll Success': props<{ poll: Poll }>(),
    'Update Poll Failure': props<{ error: string }>(),

    'Vote': props<{ pollId: string; request: CastVoteRequest }>(),
    'Vote Success': props<{ pollId: string }>(),
    'Vote Failure': props<{ error: string }>(),

    'Close Poll': props<{ pollId: string }>(),
    'Close Poll Success': props<{ pollId: string }>(),
    'Close Poll Failure': props<{ error: string }>(),

    'Delete Poll': props<{ pollId: string }>(),
    'Delete Poll Success': props<{ pollId: string }>(),
    'Delete Poll Failure': props<{ error: string }>(),

    'Load Recent Poll': props<{ cursor?: string; pageSize?: number; commentsPageSize?: number }>(),
    'Load Recent Poll Success': props<{ poll: Poll; comments: any[]; commentsCursor?: string | null; hasMoreComments?: boolean }>(),
    'Load Recent Poll Failure': props<{ error: string }>(),
    'Load More Poll Comments': props<{ cursor?: string; pageSize?: number }>(),
    'Load More Poll Comments Success': props<{ comments: any[]; commentsCursor?: string | null; hasMoreComments?: boolean }>(),
    'Load More Poll Comments Failure': props<{ error: string }>(),
  },
});
