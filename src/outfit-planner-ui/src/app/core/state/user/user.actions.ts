import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  UserProfile,
  UpdateUserProfileRequest,
  ChangePasswordRequest,
  UpdateEmailRequest,
  StyleRule,
  CreateStyleRuleRequest,
  UpdateStyleRuleRequest,
} from '../../../domain/entities/user-profile.entity';
import { AppPreferences, UpdateAppPreferencesRequest } from '../../services/app-preferences.service';
import { NotificationSettings, UpdateNotificationSettingsRequest } from '../../services/notification-settings.service';
import { ConnectedAccount } from '../../services/connected-accounts.service';
import { Follower, Following, FollowStats, IsFollowing } from '../../../domain/entities/follow.entity';
import { PublicUserProfile } from '../../../domain/entities/public-user-profile.entity';

export const UserActions = createActionGroup({
  source: 'User',
  events: {
    // Load Profile
    'Load Profile': emptyProps(),
    'Load Profile Success': props<{ profile: UserProfile }>(),
    'Load Profile Failure': props<{ error: string }>(),

    'Load Profile Picture': emptyProps(),
    'Load Profile Picture Success': props<{ profilePictureUrl: string }>(),
    'Load Profile Picture Failure': props<{ error: string }>(),

    // Update Profile
    'Update Profile': props<{ request: UpdateUserProfileRequest }>(),
    'Update Profile Success': props<{ profile: UserProfile }>(),
    'Update Profile Failure': props<{ error: string }>(),

    // Upload Profile Picture
    'Upload Profile Picture': props<{ file: File }>(),
    'Upload Profile Picture Success': props<{ profilePictureUrl: string }>(),
    'Upload Profile Picture Failure': props<{ error: string }>(),

    // Change Password
    'Change Password': props<{ request: ChangePasswordRequest }>(),
    'Change Password Success': emptyProps(),
    'Change Password Failure': props<{ error: string }>(),

    // Update Email
    'Update Email': props<{ request: UpdateEmailRequest }>(),
    'Update Email Success': props<{ email: string }>(),
    'Update Email Failure': props<{ error: string }>(),

    // Style Rules
    'Load Style Rules': emptyProps(),
    'Load Style Rules Success': props<{ rules: StyleRule[] }>(),
    'Load Style Rules Failure': props<{ error: string }>(),

    'Create Style Rule': props<{ rule: CreateStyleRuleRequest }>(),
    'Create Style Rule Success': props<{ rule: StyleRule }>(),
    'Create Style Rule Failure': props<{ error: string }>(),

    'Update Style Rule': props<{ id: string; rule: UpdateStyleRuleRequest }>(),
    'Update Style Rule Success': props<{ rule: StyleRule }>(),
    'Update Style Rule Failure': props<{ error: string }>(),

    'Delete Style Rule': props<{ id: string }>(),
    'Delete Style Rule Success': props<{ id: string }>(),
    'Delete Style Rule Failure': props<{ error: string }>(),

    // App Preferences
    'Load App Preferences': emptyProps(),
    'Load App Preferences Success': props<{ preferences: AppPreferences }>(),
    'Load App Preferences Failure': props<{ error: string }>(),
    'Update App Preferences': props<{ request: UpdateAppPreferencesRequest }>(),
    'Update App Preferences Success': props<{ preferences: AppPreferences }>(),
    'Update App Preferences Failure': props<{ error: string }>(),

    // Notification Settings
    'Load Notification Settings': emptyProps(),
    'Load Notification Settings Success': props<{ settings: NotificationSettings }>(),
    'Load Notification Settings Failure': props<{ error: string }>(),
    'Update Notification Settings': props<{ request: UpdateNotificationSettingsRequest }>(),
    'Update Notification Settings Success': props<{ settings: NotificationSettings }>(),
    'Update Notification Settings Failure': props<{ error: string }>(),

    // Clear Error
    'Clear Error': emptyProps(),

    // Connected Accounts
    'Load Connected Accounts': emptyProps(),
    'Load Connected Accounts Success': props<{ accounts: ConnectedAccount[] }>(),
    'Load Connected Accounts Failure': props<{ error: string }>(),
    'Connect Account': props<{ provider: 'Google' | 'Facebook' }>(),
    'Connect Account Success': props<{ accounts: ConnectedAccount[] }>(),
    'Connect Account Failure': props<{ error: string }>(),
    'Disconnect Account': props<{ provider: string }>(),
    'Disconnect Account Success': props<{ accounts: ConnectedAccount[] }>(),
    'Disconnect Account Failure': props<{ error: string }>(),

    // Export User Data
    'Export User Data': emptyProps(),
    'Export User Data Success': props<{ blob: Blob; filename: string }>(),
    'Export User Data Failure': props<{ error: string }>(),

    // Delete Account
    'Delete Account': emptyProps(),
    'Delete Account Success': emptyProps(),
    'Delete Account Failure': props<{ error: string }>(),

    //Load other user profile
    'Load User Profile': props<{ userId: string }>(),
    'Load User Profile Success': props<{ user: PublicUserProfile }>(),
    'Load User Profile Failure': props<{ error: string }>(),
    
  },
});
