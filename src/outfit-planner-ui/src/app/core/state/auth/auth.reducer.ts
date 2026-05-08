import { createReducer, on } from '@ngrx/store';
import { AuthActions } from './auth.actions';
import { User } from '../../../domain/entities/user-profile.entity';

export interface AuthState {
  user: Partial<User> | null;
  token: string | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
}

export const initialState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  loading: false,
  error: null,
};

export const authReducer = createReducer(
  initialState,

  // Login
  on(AuthActions.login, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(AuthActions.loginSuccess, (state, { response }) => ({
    ...state,
    user: {
      id: response.id,
      email: response.email,
      username: response.userName,
    },
    token: response.token ?? null,
    isAuthenticated: true,
    loading: false,
    error: null,
  })),
  on(AuthActions.loginFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Register
  on(AuthActions.register, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(AuthActions.registerSuccess, (state, { response }) => ({
    ...state,
    user: {
      id: response.userId,
      email: response.email,
      username: response.userName,
    },
    token: response.token ?? null,
    isAuthenticated: true,
    loading: false,
    error: null,
  })),
  on(AuthActions.registerFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Logout
  on(AuthActions.logout, () => initialState),

  // Token Refresh
  on(AuthActions.refreshToken, (state) => ({
    ...state,
    loading: true,
  })),
  on(AuthActions.refreshTokenSuccess, (state, { response }) => ({
    ...state,
    user: {
      id: response.id,
      email: response.email,
      username: response.userName,
    },
    token: response.token ?? null,
    isAuthenticated: true,
    loading: false,
  })),
  on(AuthActions.refreshTokenFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),
);