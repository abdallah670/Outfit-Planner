import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { CommandResponse } from '../../../domain/entities/response.entity';
import { FeedPost } from '../../../domain/entities/feed.entity';

export const OutfitPostsActions = createActionGroup({
  source: 'outfit-posts',
  events: {
    'Create Outfit Post': props<{ outfitId: string; caption?: string; visibility: number }>(),
    'Create Outfit Post Success': props<{ post: FeedPost }>(),
    'Create Outfit Post Failure': props<{ error: string }>(),

    'Get Outfit Post': props<{ id: string }>(),
    'Get Outfit Post Success': props<{ post: FeedPost }>(),
    'Get Outfit Post Failure': props<{ error: string }>(),

    'Load User Outfit Posts': emptyProps(),
    'Load User Outfit Posts Success': props<{ posts: FeedPost[] }>(),
    'Load User Outfit Posts Failure': props<{ error: string }>(),

    'Update Outfit Post': props<{ id: string; caption?: string; visibility: number }>(),
    'Update Outfit Post Success': props<{ response: CommandResponse }>(),
    'Update Outfit Post Failure': props<{ error: string }>(),

    'Delete Outfit Post': props<{ id: string }>(),
    'Delete Outfit Post Success': props<{ id: string }>(),
    'Delete Outfit Post Failure': props<{ error: string }>(),
  },
});
