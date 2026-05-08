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
import { feedRepositoryProvider } from './data/repositories/feed.repository.impl';
import { pollsRepositoryProvider } from './data/repositories/polls.repository.impl';
import { trendingRepositoryProvider } from './data/repositories/trending.repository.impl';
import { followRepositoryProvider } from './data/repositories/follow.repository.impl';
import { wearEventRepositoryProvider } from './data/repositories/wear-event.repository.impl';
import { userRepositoryProvider } from './data/repositories/user.repository.impl';
import { feedFeature, reducer as feedReducer } from './core/state/feed/feed.reducer';
import { FeedEffects } from './core/state/feed/feed.effects';
import { pollsFeature, reducer as pollsReducer } from './core/state/polls/polls.reducer';
import { PollsEffects } from './core/state/polls/polls.effects';
import { trendingFeature, reducer as trendingReducer } from './core/state/trending/trending.reducer';
import { TrendingEffects } from './core/state/trending/trending.effects';
import { followFeature, reducer as followReducer } from './core/state/follow/follow.reducer';
import { FollowEffects } from './core/state/follow/follow.effects';
import { outfitPostsFeature, reducer as outfitPostsReducer } from './core/state/outfit-posts/outfit-posts.reducer';
import { OutfitPostsEffects } from './core/state/outfit-posts/outfit-posts.effects';
import { outfitPostsRepositoryProvider } from './data/repositories/outfit-posts.repository.impl';
import { searchReducer } from './core/state/search/search.reducer';
import * as searchEffects from './core/state/search/search.effects';
import { calendarFeature } from './core/state/calendar/calendar.reducer';
import { CalendarEffects } from './core/state/calendar/calendar.effects';
import { adminReducer } from './core/state/admin/admin.reducer';
import { AdminEffects } from './core/state/admin/admin.effects';
import { adminRepositoryProvider } from './data/repositories/admin.repository.impl';

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
       feed: feedReducer,
       polls: pollsReducer,
       trending: trendingReducer,
       follow: followReducer,
       outfitPosts: outfitPostsReducer,
       admin: adminReducer,
       calendar: calendarFeature.reducer,
       search: searchReducer,
     }),
     provideEffects(
       authEffects,
       WardrobeEffects,
       WeatherEffects,
       userEffects,
       OutfitEffects,
       FeedEffects,
       PollsEffects,
       TrendingEffects,
       FollowEffects,
       OutfitPostsEffects,
       CalendarEffects,
       searchEffects,
       AdminEffects,
     ),
      weatherRepositoryProvider,
      outfitRepositoryProvider,
      feedRepositoryProvider,
      pollsRepositoryProvider,
      trendingRepositoryProvider,
      followRepositoryProvider,
      outfitPostsRepositoryProvider,
      wearEventRepositoryProvider,
     userRepositoryProvider,
     adminRepositoryProvider,
    provideStoreDevtools({
      maxAge: 25,
      logOnly: !isDevMode(),
      autoPause: true,
      trace: false,
      traceLimit: 75,
    }),
  ],
};
