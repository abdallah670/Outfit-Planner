import { createReducer, on } from '@ngrx/store';
import { NotificationActions } from './notifications.actions';

export interface NotificationsState {
  unreadCount: number;
  latestNotification: any | null;
}

export const initialNotificationsState: NotificationsState = {
  unreadCount: 0,
  latestNotification: null,
};

export const notificationsReducer = createReducer(
  initialNotificationsState,

  on(NotificationActions.receiveNotification, (state, { notification }) => ({
    ...state,
    unreadCount: state.unreadCount + 1,
    latestNotification: notification,
  })),
);