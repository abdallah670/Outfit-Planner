import { createActionGroup, props } from '@ngrx/store';

export const NotificationActions = createActionGroup({
  source: 'Notifications',
  events: {
    'Receive Notification': props<{ notification: any }>(),
  },
});