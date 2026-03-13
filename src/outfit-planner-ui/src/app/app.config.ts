import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  isDevMode,
  importProvidersFrom,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';
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
import { socialRepositoryProvider } from './data/repositories/social.repository.impl';
import { wearEventRepositoryProvider } from './data/repositories/wear-event.repository.impl';
import { socialFeature, reducer as socialReducer } from './core/state/social/social.reducer';
import {
  calendarFeature,
  reducer as calendarReducer,
} from './core/state/calendar/calendar.reducer';
import { CalendarEffects } from './core/state/calendar/calendar.effects';
import { SocialEffects } from './core/state/social/social.effects';
import { searchReducer } from './core/state/search/search.reducer';
import * as searchEffects from './core/state/search/search.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([tokenInterceptor]), withFetch()),
    provideAnimationsAsync(),
    importProvidersFrom(MatSnackBarModule),
    CookieService,
    provideStore({
      auth: authReducer,
      wardrobe: wardrobeReducer,
      weather: weatherReducer,
      user: userReducer,
      outfit: outfitReducer,
      social: socialReducer,
      calendar: calendarReducer,
      search: searchReducer,
    }),
    provideEffects(
      authEffects,
      WardrobeEffects,
      WeatherEffects,
      userEffects,
      OutfitEffects,
      SocialEffects,
      CalendarEffects,
      searchEffects,
    ),
    weatherRepositoryProvider,
    outfitRepositoryProvider,
    socialRepositoryProvider,
    wearEventRepositoryProvider,
    provideStoreDevtools({
      maxAge: 25,
      logOnly: !isDevMode(),
      autoPause: true,
      trace: false,
      traceLimit: 75,
    }),
  ],
};
