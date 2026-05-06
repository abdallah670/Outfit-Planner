import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { FeedPost } from '../../../domain/entities/feed.entity';
import { PostComment } from '../../../domain/entities/feed.entity';
import { CommandResponse, CursorPagedResult } from '../../../domain/entities/response.entity';

export const FeedActions = createActionGroup({
  source: 'feed',
  events: {
    'Load Posts': props<{ cursor?: string; pageSize?: number; visibility?: string; sortBy?: string; postType?: string }>(),
    'Load Posts Success': props<{ result: CursorPagedResult<FeedPost>; append: boolean }>(),
    'Load Posts Failure': props<{ error: string }>(),

    'Load Post By Id': props<{ id: string }>(),
    'Load Post By Id Success': props<{ post: FeedPost }>(),
    'Load Post By Id Failure': props<{ error: string }>(),

    'Delete Post': props<{ id: string }>(),
    'Delete Post Success': props<{ id: string }>(),
    'Delete Post Failure': props<{ error: string }>(),

    'Add Reaction': props<{ postId: string }>(),
    'Add Reaction Success': props<{ postId: string }>(),
    'Add Reaction Failure': props<{ error: string }>(),

    'Remove Reaction': props<{ postId: string }>(),
    'Remove Reaction Success': props<{ postId: string }>(),
    'Remove Reaction Failure': props<{ error: string }>(),

    'Load Comments': props<{ postId: string; cursor?: string; pageSize?: number }>(),
    'Load Comments Success': props<{ postId: string; result: CursorPagedResult<PostComment>; append: boolean }>(),
    'Load Comments Failure': props<{ error: string }>(),

    'Add Comment': props<{ postId: string; content: string; parentCommentId?: string }>(),
    'Add Comment Success': props<{ postId: string; comment: any }>(),
    'Add Comment Failure': props<{ error: string }>(),

    'Delete Comment': props<{ commentId: string; postId: string }>(),
    'Delete Comment Success': props<{ commentId: string; postId: string }>(),
    'Delete Comment Failure': props<{ error: string }>(),
  },
});
