import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  isDevMode,
  importProvidersFrom,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { CookieService } from 'ngx-cookie-service';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { MatSnackBarModule } from '@angular/material/snack-bar';

import { routes } from './app.routes';
import { tokenInterceptor } from './core/interceptors/token-interceptor';
import { authReducer } from './core/state/auth/auth.reducer';
import * as authEffects from './core/state/auth/auth.effects';
import {
  wardrobeFeature,
  reducer as wardrobeReducer,
} from './core/state/wardrobe/wardrobe.reducer';
import { WardrobeEffects } from './core/state/wardrobe/wardrobe.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([tokenInterceptor])),
    provideAnimationsAsync(),
    importProvidersFrom(MatSnackBarModule),
    CookieService,
    provideStore({
      auth: authReducer,
      wardrobe: wardrobeReducer,
    }),
    provideEffects(authEffects, WardrobeEffects),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: !isDevMode(),
      autoPause: true,
      trace: false,
      traceLimit: 75,
    }),
  ],
};
