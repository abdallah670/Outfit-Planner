import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { exhaustMap, map, catchError, of, switchMap } from 'rxjs';
import { UserActions } from './user.actions';
import { UserRepositoryImpl } from '../../../data/repositories/user.repository.impl';
import { UserProfile } from '../../../domain/entities/user-profile.entity';

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
