import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { exhaustMap, map, catchError, of, switchMap, tap, distinctUntilChanged } from 'rxjs';
import { UserActions } from './user.actions';
import { UserRepositoryImpl } from '../../../data/repositories/user.repository.impl';
import { UserProfile, StyleRule } from '../../../domain/entities/user-profile.entity';
import { StyleRuleService } from '../../services/style-rule.service';
import { AppPreferencesService, AppPreferences } from '../../services/app-preferences.service';
import { NotificationSettingsService, NotificationSettings } from '../../services/notification-settings.service';
import { ConnectedAccountsService, ConnectedAccount, ConnectAccountResponse } from '../../services/connected-accounts.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PublicUserProfile } from '../../../domain/entities/public-user-profile.entity';

export const loadProfile$ = createEffect(
  (actions$ = inject(Actions), userRepository = inject(UserRepositoryImpl)) => {
    return actions$.pipe(
      ofType(UserActions.loadProfile),
      exhaustMap(() =>
        userRepository.getProfile().pipe(
          map((profile: UserProfile) => UserActions.loadProfileSuccess({ profile })),
          catchError((error) =>
            of(
              UserActions.loadProfileFailure({ error: error?.message || 'Failed to load profile' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);
export const loadProfilePicture$ = createEffect(
  (actions$ = inject(Actions), userRepository = inject(UserRepositoryImpl)) => {
    return actions$.pipe(
      ofType(UserActions.loadProfilePicture),
      exhaustMap(() =>
        userRepository.getProfilePicture().pipe(
          map((profilePictureUrl: string) => UserActions.loadProfilePictureSuccess({ profilePictureUrl })),
          catchError((error) =>
            of(
              UserActions.loadProfilePictureFailure({ error: error?.message || 'Failed to load profile picture' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const updateProfile$ = createEffect(
  (actions$ = inject(Actions), userRepository = inject(UserRepositoryImpl)) => {
    return actions$.pipe(
      ofType(UserActions.updateProfile),
      exhaustMap(({ request }) =>
        userRepository.updateProfile(request).pipe(
          switchMap(() =>
            userRepository.getProfile().pipe(
              map((profile: UserProfile) => UserActions.updateProfileSuccess({ profile })),
              catchError((error) =>
                of(
                  UserActions.updateProfileFailure({
                    error: error?.message || 'Failed to update profile',
                  }),
                ),
              ),
            ),
          ),
          catchError((error) =>
            of(
              UserActions.updateProfileFailure({
                error: error?.message || 'Failed to update profile',
              }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const uploadProfilePicture$ = createEffect(
  (actions$ = inject(Actions), userRepository = inject(UserRepositoryImpl)) => {
    return actions$.pipe(
      ofType(UserActions.uploadProfilePicture),
      exhaustMap(({ file }) =>
        userRepository.uploadProfilePicture(file).pipe(
          map((profilePictureUrl: string) =>
            UserActions.uploadProfilePictureSuccess({ profilePictureUrl }),
          ),
          catchError((error) =>
            of(
              UserActions.uploadProfilePictureFailure({
                error: error?.message || 'Failed to upload profile picture',
              }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const changePassword$ = createEffect(
  (actions$ = inject(Actions), userRepository = inject(UserRepositoryImpl)) => {
    return actions$.pipe(
      ofType(UserActions.changePassword),
      exhaustMap(({ request }) =>
        userRepository.changePassword(request).pipe(
          map(() => UserActions.changePasswordSuccess()),
          catchError((error) =>
            of(
              UserActions.changePasswordFailure({
                error: error?.message || 'Failed to change password',
              }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const updateEmail$ = createEffect(
  (actions$ = inject(Actions), userRepository = inject(UserRepositoryImpl)) => {
    return actions$.pipe(
      ofType(UserActions.updateEmail),
      exhaustMap(({ request }) =>
        userRepository.updateEmail(request).pipe(
          map((response: { success: boolean; message: string }) => {
            if (response.success) {
              return UserActions.updateEmailSuccess({ email: request.newEmail });
            }
            return UserActions.updateEmailFailure({
              error: response.message || 'Failed to update email',
            });
          }),
          catchError((error) =>
            of(
              UserActions.updateEmailFailure({ error: error?.message || 'Failed to update email' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

// Style Rules Effects
export const loadStyleRules$ = createEffect(
  (actions$ = inject(Actions), styleRuleService = inject(StyleRuleService)) => {
    return actions$.pipe(
      ofType(UserActions.loadStyleRules),
      exhaustMap(() =>
        styleRuleService.getStyleRules().pipe(
          map((rules: StyleRule[]) => UserActions.loadStyleRulesSuccess({ rules })),
          catchError((error) =>
            of(
              UserActions.loadStyleRulesFailure({ error: error?.message || 'Failed to load style rules' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const createStyleRule$ = createEffect(
  (actions$ = inject(Actions), styleRuleService = inject(StyleRuleService)) => {
    return actions$.pipe(
      ofType(UserActions.createStyleRule),
      exhaustMap(({ rule }) =>
        styleRuleService.createStyleRule(rule).pipe(
          map((createdRule: StyleRule) => UserActions.createStyleRuleSuccess({ rule: createdRule })),
          catchError((error) =>
            of(
              UserActions.createStyleRuleFailure({ error: error?.message || 'Failed to create style rule' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const updateStyleRule$ = createEffect(
  (actions$ = inject(Actions), styleRuleService = inject(StyleRuleService)) => {
    return actions$.pipe(
      ofType(UserActions.updateStyleRule),
      exhaustMap(({ id, rule }) =>
        styleRuleService.updateStyleRule(id, rule).pipe(
          map(() => UserActions.updateStyleRuleSuccess({ rule: { ...rule, id } as StyleRule })),
          catchError((error) =>
            of(
              UserActions.updateStyleRuleFailure({ error: error?.message || 'Failed to update style rule' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const deleteStyleRule$ = createEffect(
  (actions$ = inject(Actions), styleRuleService = inject(StyleRuleService)) => {
    return actions$.pipe(
      ofType(UserActions.deleteStyleRule),
      exhaustMap(({ id }) =>
        styleRuleService.deleteStyleRule(id).pipe(
          map(() => UserActions.deleteStyleRuleSuccess({ id })),
          catchError((error) =>
            of(
              UserActions.deleteStyleRuleFailure({ error: error?.message || 'Failed to delete style rule' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

// App Preferences Effects
export const loadAppPreferences$ = createEffect(
  (actions$ = inject(Actions), appPreferencesService = inject(AppPreferencesService)) => {
    return actions$.pipe(
      ofType(UserActions.loadAppPreferences),
      distinctUntilChanged(),
      exhaustMap(() =>
        appPreferencesService.getPreferences().pipe(
          map((preferences: AppPreferences) => UserActions.loadAppPreferencesSuccess({ preferences })),
          catchError((error) =>
            of(
              UserActions.loadAppPreferencesFailure({ error: error?.message || 'Failed to load app preferences' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const updateAppPreferences$ = createEffect(
  (actions$ = inject(Actions), appPreferencesService = inject(AppPreferencesService)) => {
    return actions$.pipe(
      ofType(UserActions.updateAppPreferences),
      exhaustMap(({ request }) =>
        appPreferencesService.updatePreferences(request).pipe(
          switchMap(() =>
            appPreferencesService.getPreferences().pipe(
              map((preferences: AppPreferences) => UserActions.updateAppPreferencesSuccess({ preferences })),
              catchError((error) =>
                of(
                  UserActions.updateAppPreferencesFailure({ error: error?.message || 'Failed to update app preferences' }),
                ),
              ),
            ),
          ),
          catchError((error) =>
            of(
              UserActions.updateAppPreferencesFailure({ error: error?.message || 'Failed to update app preferences' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

// Notification Settings Effects
export const loadNotificationSettings$ = createEffect(
  (actions$ = inject(Actions), notificationSettingsService = inject(NotificationSettingsService)) => {
    return actions$.pipe(
      ofType(UserActions.loadNotificationSettings),
      exhaustMap(() =>
        notificationSettingsService.getSettings().pipe(
          map((settings: NotificationSettings) => UserActions.loadNotificationSettingsSuccess({ settings })),
          catchError((error) =>
            of(
              UserActions.loadNotificationSettingsFailure({ error: error?.message || 'Failed to load notification settings' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const updateNotificationSettings$ = createEffect(
  (actions$ = inject(Actions), notificationSettingsService = inject(NotificationSettingsService)) => {
    return actions$.pipe(
      ofType(UserActions.updateNotificationSettings),
      exhaustMap(({ request }) =>
        notificationSettingsService.updateSettings(request).pipe(
          switchMap(() =>
            notificationSettingsService.getSettings().pipe(
              map((settings: NotificationSettings) => UserActions.updateNotificationSettingsSuccess({ settings })),
              catchError((error) =>
                of(
                  UserActions.updateNotificationSettingsFailure({ error: error?.message || 'Failed to update notification settings' }),
                ),
              ),
            ),
          ),
          catchError((error) =>
            of(
              UserActions.updateNotificationSettingsFailure({ error: error?.message || 'Failed to update notification settings' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

// Connected Accounts Effects
export const loadConnectedAccounts$ = createEffect(
  (actions$ = inject(Actions), connectedAccountsService = inject(ConnectedAccountsService)) => {
    return actions$.pipe(
      ofType(UserActions.loadConnectedAccounts),
      distinctUntilChanged(),
      exhaustMap(() =>
        connectedAccountsService.getConnectedAccounts().pipe(
          map((accounts: ConnectedAccount[]) => UserActions.loadConnectedAccountsSuccess({ accounts })),
          catchError((error) =>
            of(
              UserActions.loadConnectedAccountsFailure({ error: error?.message || 'Failed to load connected accounts' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const connectAccount$ = createEffect(
  (actions$ = inject(Actions), connectedAccountsService = inject(ConnectedAccountsService)) => {
    return actions$.pipe(
      ofType(UserActions.connectAccount),
      exhaustMap(({ provider }) =>
        connectedAccountsService.connectAccount(provider).pipe(
          tap((response: ConnectAccountResponse) => {
            // Redirect to OAuth provider
            if (response.authorizationUrl) {
              window.location.href = response.authorizationUrl;
            }
          }),
          map(() => UserActions.loadConnectedAccounts()),
          catchError((error) =>
            of(
              UserActions.connectAccountFailure({ error: error?.message || 'Failed to connect account' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const disconnectAccount$ = createEffect(
  (actions$ = inject(Actions), connectedAccountsService = inject(ConnectedAccountsService)) => {
    return actions$.pipe(
      ofType(UserActions.disconnectAccount),
      exhaustMap(({ provider }) =>
        connectedAccountsService.disconnectAccount(provider).pipe(
          switchMap(() =>
            connectedAccountsService.getConnectedAccounts().pipe(
              map((accounts: ConnectedAccount[]) => UserActions.disconnectAccountSuccess({ accounts })),
              catchError((error) =>
                of(
                  UserActions.disconnectAccountFailure({ error: error?.message || 'Failed to disconnect account' }),
                ),
              ),
            ),
          ),
          catchError((error) =>
            of(
              UserActions.disconnectAccountFailure({ error: error?.message || 'Failed to disconnect account' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

// Export User Data Effect
export const exportUserData$ = createEffect(
  (actions$ = inject(Actions), http = inject(HttpClient)) => {
    return actions$.pipe(
      ofType(UserActions.exportUserData),
      exhaustMap(() =>
        http.get(`${environment.baseUrl}/user/export-data`, {
          responseType: 'blob',
          withCredentials: true,
        }).pipe(
          map((blob: Blob) => {
            // Create download link
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = `outfit-planner-data-${new Date().toISOString().split('T')[0].replace(/-/g, '')}.csv`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);
            return UserActions.exportUserDataSuccess({ blob, filename: link.download });
          }),
          catchError((error) =>
            of(
              UserActions.exportUserDataFailure({ error: error?.message || 'Failed to export data' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

// Delete Account Effect
export const deleteAccount$ = createEffect(
  (actions$ = inject(Actions), http = inject(HttpClient)) => {
    return actions$.pipe(
      ofType(UserActions.deleteAccount),
      exhaustMap(() =>
        http.delete(`${environment.baseUrl}/user/account`, {
          withCredentials: true,
        }).pipe(
          map(() => {
            // Redirect to home page after successful deletion
            window.location.href = '/';
            return UserActions.deleteAccountSuccess();
          }),
          catchError((error) =>
            of(
              UserActions.deleteAccountFailure({ error: error?.message || 'Failed to delete account' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const loadUserProfile$ = createEffect(
  (actions$ = inject(Actions), userRepository = inject(UserRepositoryImpl)) => {
    return actions$.pipe(
      ofType(UserActions.loadUserProfile),
      exhaustMap(({ userId }) =>
        userRepository.getPublicProfile(userId).pipe(
          map((publicUser: PublicUserProfile) => {
            // Map PublicUserProfile to UserProfile
            const userProfile: UserProfile = {
              id: publicUser.id,
              name: publicUser.name,
              email: '',
              username: publicUser.userName || '',
              profilePictureUrl: publicUser.profilePictureUrl,
              bio: publicUser.bio,
              createdAt: new Date(publicUser.createdAt).toISOString(),
              wardrobeItemCount: publicUser.wardrobeItemCount,
              outfitCount: publicUser.outfitCount,
              totalWears: publicUser.totalWears,
              styleProfile: publicUser.styleProfile ? {
                style: publicUser.styleProfile.style as any,
                preferredColors: publicUser.styleProfile.preferredColors,
                fitPreferences: publicUser.styleProfile.fitPreferences || '',
                comfortPriority: publicUser.styleProfile.comfortPriority,
                acceptsTrends: publicUser.styleProfile.acceptsTrends,
                customRules: [],
              } : undefined,
            };
            return UserActions.loadUserProfileSuccess({ user: userProfile });
          }),
          catchError((error) =>
            of(
              UserActions.loadUserProfileFailure({
                error: error?.message || 'Failed to load user profile',
              }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);
