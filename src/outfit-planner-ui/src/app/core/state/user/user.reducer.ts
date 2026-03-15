import { createReducer, on } from '@ngrx/store';
import { UserActions } from './user.actions';
import { UserProfile } from '../../../domain/entities/user-profile.entity';

export interface UserState {
  profile: UserProfile | null;
  loading: boolean;
  updating: boolean;
  uploadingPicture: boolean;
  changingPassword: boolean;
  updatingEmail: boolean;
  error: string | null;
}

export const initialState: UserState = {
  profile: null,
  loading: false,
  updating: false,
  uploadingPicture: false,
  changingPassword: false,
  updatingEmail: false,
  error: null,
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

  // Clear Error
  on(UserActions.clearError, (state) => ({
    ...state,
    error: null,
  })),
);
