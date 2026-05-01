import { createFeatureSelector, createSelector } from '@ngrx/store';
import { UserState } from './user.reducer';

export const selectUserState = createFeatureSelector<UserState>('user');

export const selectUserProfile = createSelector(
  selectUserState,
  (state) => state.profile
);

export const selectProfilePictureUrl = createSelector(
  selectUserState,
  (state) => state.profile?.profilePictureUrl ?? null
);

export const selectUserLoading = createSelector(
  selectUserState,
  (state) => state.loading
);

export const selectUserUpdating = createSelector(
  selectUserState,
  (state) => state.updating
);

export const selectUploadingPicture = createSelector(
  selectUserState,
  (state) => state.uploadingPicture
);

export const selectChangingPassword = createSelector(
  selectUserState,
  (state) => state.changingPassword
);

export const selectUpdatingEmail = createSelector(
  selectUserState,
  (state) => state.updatingEmail
);



export const selectUserProfileLoading = createSelector(
  selectUserState,
  (state) => state.loading || state.updating || state.uploadingPicture || state.changingPassword
);

export const selectUserError = createSelector(
  selectUserState,
  (state) => state.error
);

export const selectUserStats = createSelector(
  selectUserProfile,
  (profile) => profile ? {
    wardrobeItemCount: profile.wardrobeItemCount,
    outfitCount: profile.outfitCount,
    totalWears: profile.totalWears,
    memberSince: new Date(profile.createdAt).toLocaleDateString()
  } : null
);

export const selectStyleProfile = createSelector(
  selectUserProfile,
  (profile) => profile?.styleProfile
);

export const selectUserPreferences = createSelector(
  selectUserProfile,
  (profile) => profile?.preferences
);

export const selectStyleRules = createSelector(
  selectUserState,
  (state) => state.styleRules
);

export const selectStyleRulesLoading = createSelector(
  selectUserState,
  (state) => state.styleRulesLoading
);

// Settings Selectors
export const selectAppPreferences = createSelector(
  selectUserState,
  (state) => state.appPreferences
);

export const selectNotificationSettings = createSelector(
  selectUserState,
  (state) => state.notificationSettings
);

export const selectSettingsLoading = createSelector(
  selectUserState,
  (state) => state.settingsLoading
);

// Connected Accounts Selectors
export const selectConnectedAccounts = createSelector(
  selectUserState,
  (state) => state.connectedAccounts
);

export const selectConnectedAccountsLoading = createSelector(
  selectUserState,
  (state) => state.connectedAccountsLoading
);

export const selectSelectedPublicProfile = createSelector(
  selectUserState,
  (state) => state.selectedPublicProfile
);

export const selectPublicProfileLoading = createSelector(
  selectUserState,
  (state) => state.publicProfileLoading
);
