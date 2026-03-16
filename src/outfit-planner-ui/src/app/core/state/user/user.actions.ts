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

    // Clear Error
    'Clear Error': emptyProps(),
  },
});
