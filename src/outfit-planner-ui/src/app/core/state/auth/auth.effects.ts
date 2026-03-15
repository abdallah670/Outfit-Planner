import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { exhaustMap, map, catchError, tap, of } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { AuthActions } from './auth.actions';
import { UserActions } from '../user/user.actions';
import { AuthResponse, RegistrationResponse } from '../../../data/models/auth.model';

export const login$ = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) => {
    return actions$.pipe(
      ofType(AuthActions.login),
      exhaustMap(({ request }) =>
        authService.login(request).pipe(
          map((response: AuthResponse) => AuthActions.loginSuccess({ response })),
          catchError((error) =>
            of(AuthActions.loginFailure({ error: error?.message || 'Login failed' })),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const register$ = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) => {
    return actions$.pipe(
      ofType(AuthActions.register),
      exhaustMap(({ request }) =>
        authService.register(request).pipe(
          map((response: RegistrationResponse) => AuthActions.registerSuccess({ response })),
          catchError((error) =>
            of(AuthActions.registerFailure({ error: error?.message || 'Registration failed' })),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const logout$ = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService), router = inject(Router)) => {
    return actions$.pipe(
      ofType(AuthActions.logout),
      tap(() => {
        authService.logout();
        router.navigate(['/login']);
      }),
    );
  },
  { functional: true, dispatch: false },
);

export const refreshToken$ = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) => {
    return actions$.pipe(
      ofType(AuthActions.refreshToken),
      exhaustMap(() =>
        authService.refreshToken().pipe(
          map((response: AuthResponse) => AuthActions.refreshTokenSuccess({ response })),
          catchError((error) =>
            of(
              AuthActions.refreshTokenFailure({ error: error?.message || 'Token refresh failed' }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const loginSuccess$ = createEffect(
  (actions$ = inject(Actions), router = inject(Router)) => {
    return actions$.pipe(
      ofType(AuthActions.loginSuccess, AuthActions.registerSuccess),
      tap(() => router.navigate(['/'])),
    );
  },
  { functional: true, dispatch: false },
);

// Load user profile after successful login/register
export const loadProfileAfterAuth$ = createEffect(
  () => {
    return inject(Actions).pipe(
      ofType(AuthActions.loginSuccess, AuthActions.registerSuccess),
      tap(() => {
        inject(Store).dispatch(UserActions.loadProfile());
      }),
    );
  },
  { functional: true, dispatch: false },
);
