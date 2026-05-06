import { createActionGroup, props } from '@ngrx/store';
import { Follower, Following, FollowStats } from '../../../domain/entities/follow.entity';
import { CursorPagedResult } from '../../../domain/entities/response.entity';

export const FollowActions = createActionGroup({
  source: 'follow',
  events: {
    'Follow User': props<{ userId: string }>(),
    'Follow User Success': props<{ userId: string }>(),
    'Follow User Failure': props<{ error: string }>(),

    'Unfollow User': props<{ userId: string }>(),
    'Unfollow User Success': props<{ userId: string }>(),
    'Unfollow User Failure': props<{ error: string }>(),

    'Check Follow Status': props<{ userId: string }>(),
    'Check Follow Status Success': props<{ userId: string; isFollowing: boolean }>(),
    'Check Follow Status Failure': props<{ error: string }>(),

    'Load Followers': props<{ userId: string; cursor?: string; pageSize?: number }>(),
    'Load Followers Success': props<{ userId: string; result: CursorPagedResult<Follower>; append: boolean }>(),
    'Load Followers Failure': props<{ error: string }>(),

    'Load Following': props<{ userId: string; cursor?: string; pageSize?: number }>(),
    'Load Following Success': props<{ userId: string; result: CursorPagedResult<Following>; append: boolean }>(),
    'Load Following Failure': props<{ error: string }>(),

    'Load Follow Stats': props<{ userId: string }>(),
    'Load Follow Stats Success': props<{ userId: string; stats: FollowStats }>(),
    'Load Follow Stats Failure': props<{ error: string }>(),
  },
});
