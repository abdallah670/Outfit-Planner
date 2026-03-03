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
import { weatherReducer } from './core/state/weather/weather.reducer';
import { WeatherEffects } from './core/state/weather/weather.effects';
import { userReducer } from './core/state/user/user.reducer';
import * as userEffects from './core/state/user/user.effects';
import { reducer as outfitReducer } from './core/state/outfit/outfit.reducer';
import { OutfitEffects } from './core/state/outfit/outfit.effects';
import { weatherRepositoryProvider } from './data/repositories/weather.repository.impl';
import { outfitRepositoryProvider } from './data/repositories/outfit.repository.impl';

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
      weather: weatherReducer,
      user: userReducer,
      outfit: outfitReducer,
    }),
    provideEffects(authEffects, WardrobeEffects, WeatherEffects, userEffects, OutfitEffects),
    weatherRepositoryProvider,
    outfitRepositoryProvider,
    provideStoreDevtools({
      maxAge: 25,
      logOnly: !isDevMode(),
      autoPause: true,
      trace: false,
      traceLimit: 75,
    }),
  ],
};
