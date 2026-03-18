import { createReducer, on } from '@ngrx/store';
import { UserActions } from './user.actions';
import { UserProfile, StyleRule } from '../../../domain/entities/user-profile.entity';
import { AppPreferences } from '../../services/app-preferences.service';
import { NotificationSettings } from '../../services/notification-settings.service';

export interface UserState {
  profile: UserProfile | null;
  loading: boolean;
  updating: boolean;
  uploadingPicture: boolean;
  changingPassword: boolean;
  updatingEmail: boolean;
  error: string | null;
  styleRules: StyleRule[];
  styleRulesLoading: boolean;
  appPreferences: AppPreferences | null;
  notificationSettings: NotificationSettings | null;
  settingsLoading: boolean;
}

export const initialState: UserState = {
  profile: null,
  loading: false,
  updating: false,
  uploadingPicture: false,
  changingPassword: false,
  updatingEmail: false,
  error: null,
  styleRules: [],
  styleRulesLoading: false,
  appPreferences: null,
  notificationSettings: null,
  settingsLoading: false,
};

export const userReducer = createReducer(
  initialState,

  // Load Profile
  on(UserActions.loadProfile, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(UserActions.loadProfileSuccess, (state, { profile }) => ({
    ...state,
    profile,
    loading: false,
    error: null,
  })),
  on(UserActions.loadProfileFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(UserActions.loadProfilePicture, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(UserActions.loadProfilePictureSuccess, (state, { profilePictureUrl }) => ({
    ...state,
    profile: state.profile ? { ...state.profile, profilePictureUrl } : null,
    loading: false,
    error: null,
  })),
  on(UserActions.loadProfilePictureFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Update Profile
  on(UserActions.updateProfile, (state) => ({
    ...state,
    updating: true,
    error: null,
  })),
  on(UserActions.updateProfileSuccess, (state, { profile }) => ({
    ...state,
    profile,
    updating: false,
    error: null,
  })),
  on(UserActions.updateProfileFailure, (state, { error }) => ({
    ...state,
    updating: false,
    error,
  })),

  // Upload Profile Picture
  on(UserActions.uploadProfilePicture, (state) => ({
    ...state,
    uploadingPicture: true,
    error: null,
  })),
  on(UserActions.uploadProfilePictureSuccess, (state, { profilePictureUrl }) => ({
    ...state,
    profile: state.profile ? { ...state.profile, profilePictureUrl } : null,
    uploadingPicture: false,
    error: null,
  })),
  on(UserActions.uploadProfilePictureFailure, (state, { error }) => ({
    ...state,
    uploadingPicture: false,
    error,
  })),

  // Change Password
  on(UserActions.changePassword, (state) => ({
    ...state,
    changingPassword: true,
    error: null,
  })),
  on(UserActions.changePasswordSuccess, (state) => ({
    ...state,
    changingPassword: false,
    error: null,
  })),
  on(UserActions.changePasswordFailure, (state, { error }) => ({
    ...state,
    changingPassword: false,
    error,
  })),

  // Update Email
  on(UserActions.updateEmail, (state) => ({
    ...state,
    updatingEmail: true,
    error: null,
  })),
  on(UserActions.updateEmailSuccess, (state, { email }) => ({
    ...state,
    profile: state.profile ? { ...state.profile, email } : null,
    updatingEmail: false,
    error: null,
  })),
  on(UserActions.updateEmailFailure, (state, { error }) => ({
    ...state,
    updatingEmail: false,
    error,
  })),

  // Style Rules
  on(UserActions.loadStyleRules, (state) => ({
    ...state,
    styleRulesLoading: true,
  })),
  on(UserActions.loadStyleRulesSuccess, (state, { rules }) => ({
    ...state,
    styleRules: rules,
    styleRulesLoading: false,
  })),
  on(UserActions.loadStyleRulesFailure, (state, { error }) => ({
    ...state,
    styleRulesLoading: false,
    error,
  })),
  on(UserActions.createStyleRuleSuccess, (state, { rule }) => ({
    ...state,
    styleRules: [...state.styleRules, rule],
  })),
  on(UserActions.updateStyleRuleSuccess, (state, { rule }) => ({
    ...state,
    styleRules: state.styleRules.map((r) => (r.id === rule.id ? rule : r)),
  })),
  on(UserActions.deleteStyleRuleSuccess, (state, { id }) => ({
    ...state,
    styleRules: state.styleRules.filter((r) => r.id !== id),
  })),

  // App Preferences
  on(UserActions.loadAppPreferences, (state) => ({
    ...state,
    settingsLoading: true,
  })),
  on(UserActions.loadAppPreferencesSuccess, (state, { preferences }) => ({
    ...state,
    appPreferences: preferences,
    settingsLoading: false,
  })),
  on(UserActions.loadAppPreferencesFailure, (state, { error }) => ({
    ...state,
    settingsLoading: false,
    error,
  })),
  on(UserActions.updateAppPreferences, (state) => ({
    ...state,
    settingsLoading: true,
  })),
  on(UserActions.updateAppPreferencesSuccess, (state, { preferences }) => ({
    ...state,
    appPreferences: preferences,
    settingsLoading: false,
  })),
  on(UserActions.updateAppPreferencesFailure, (state, { error }) => ({
    ...state,
    settingsLoading: false,
    error,
  })),

  // Notification Settings
  on(UserActions.loadNotificationSettings, (state) => ({
    ...state,
    settingsLoading: true,
  })),
  on(UserActions.loadNotificationSettingsSuccess, (state, { settings }) => ({
    ...state,
    notificationSettings: settings,
    settingsLoading: false,
  })),
  on(UserActions.loadNotificationSettingsFailure, (state, { error }) => ({
    ...state,
    settingsLoading: false,
    error,
  })),
  on(UserActions.updateNotificationSettingsSuccess, (state, { settings }) => ({
    ...state,
    notificationSettings: settings,
  })),

  // Clear Error
  on(UserActions.clearError, (state) => ({
    ...state,
    error: null,
  })),
);
