import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { User } from '../../../domain/entities/user.entity';
import {
  AuthRequest,
  AuthResponse,
  RegistrationRequest,
  RegistrationResponse,
} from '../../../data/models/auth.model';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    Login: props<{ request: AuthRequest }>(),
    'Login Success': props<{ response: AuthResponse }>(),
    'Login Failure': props<{ error: string }>(),

    Register: props<{ request: RegistrationRequest }>(),
    'Register Success': props<{ response: RegistrationResponse }>(),
    'Register Failure': props<{ error: string }>(),

    Logout: emptyProps(),

    'Refresh Token': emptyProps(),
    'Refresh Token Success': props<{ response: AuthResponse }>(),
    'Refresh Token Failure': props<{ error: string }>(),

    'Check Auth': emptyProps(),
  },
});
