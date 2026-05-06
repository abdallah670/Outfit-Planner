import { Routes } from '@angular/router';
import { Login } from './presentation/pages/auth/login/login';
import { Register } from './presentation/pages/auth/register/register';
import { AuthCallbackComponent } from './presentation/pages/auth/auth-callback/auth-callback.component';
import { WardrobeDashboardComponent } from './presentation/pages/wardrobe-dashboard/wardrobe-dashboard.component';
import { AddClothingItemComponent } from './presentation/pages/add-clothing-item/add-clothing-item.component';
import { ClothingItemDetail } from './presentation/pages/clothing-item-detail/clothing-item-detail';
import { HomeComponent } from './presentation/pages/home/home.component';
import { ProfileComponent } from './presentation/pages/profile/profile.component';
import { OutfitsDashboardComponent } from './presentation/pages/outfits-dashboard/outfits-dashboard.component';
import { OutfitBuilderComponent } from './presentation/pages/outfit-builder/outfit-builder.component';
import { DailySuggestionComponent } from './presentation/pages/daily-suggestion/daily-suggestion.component';
import { OutfitDetailComponent } from './presentation/pages/outfit-detail/outfit-detail.component';
import { SettingsComponent } from './presentation/pages/settings/settings.component';
import { CalendarComponent } from './presentation/pages/calendar/calendar.component';
import { SocialComponent } from './presentation/pages/social/social.component';
import { CommunityFeedComponent } from './presentation/pages/community-feed/community-feed.component';
import { CreatePollComponent } from './presentation/pages/create-poll/create-poll.component';
import { PollDetailComponent } from './presentation/pages/social/poll-detail/poll-detail.component';
import { GlobalSearchComponent } from './presentation/pages/global-search/global-search.component';
import { NotificationsCenterComponent } from './presentation/pages/notifications-center/notifications-center.component';
import { TrendingOutfitsComponent } from './presentation/pages/social/trending-outfits/trending-outfits.component';
import { authGuard } from './core/guards/auth-guard';
import { PublicProfileComponent } from './presentation/pages/public-profile/public-profile.component';
import { MyPollsComponent } from './presentation/pages/social/my-polls/my-polls.component';
import { EditPollComponent } from './presentation/pages/social/edit-poll/edit-poll.component';
import { CreateOutfitPostComponent } from './presentation/pages/social/create-outfit-post/create-outfit-post.component';
import { MyOutfitPostsComponent } from './presentation/pages/social/my-outfit-posts/my-outfit-posts.component';

export const routes: Routes = [
  { path: '', component: HomeComponent, canActivate: [authGuard] },
  { path: 'home', redirectTo: '', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'auth/callback', component: AuthCallbackComponent },
  { path: 'profile', component: ProfileComponent, canActivate: [authGuard] },
  { path: 'wardrobe', component: WardrobeDashboardComponent, canActivate: [authGuard] },
  { path: 'wardrobe/new', component: AddClothingItemComponent, canActivate: [authGuard] },
  { path: 'wardrobe/edit/:id', component: AddClothingItemComponent, canActivate: [authGuard] },
  { path: 'wardrobe/:id', component: ClothingItemDetail, canActivate: [authGuard] },
  { path: 'outfits', component: OutfitsDashboardComponent, canActivate: [authGuard] },
  { path: 'outfits/build', component: OutfitBuilderComponent, canActivate: [authGuard] },
  { path: 'outfits/build/:id', component: OutfitBuilderComponent, canActivate: [authGuard] },
  { path: 'outfits/today', component: DailySuggestionComponent },
  { path: 'outfits/:id', component: OutfitDetailComponent, canActivate: [authGuard] },
  { path: 'calendar', component: CalendarComponent },
  { path: 'social', component: SocialComponent, canActivate: [authGuard] },
  { path: 'social/feed', component: CommunityFeedComponent, canActivate: [authGuard] },
  { path: 'social/create', component: CreatePollComponent, canActivate: [authGuard] },
  { path: 'social/trending', component: TrendingOutfitsComponent, canActivate: [authGuard] },
  { path: 'social/profile/:userId',component:PublicProfileComponent, canActivate: [authGuard] },
  { path: 'profile/:userId',component:PublicProfileComponent, canActivate: [authGuard] },
  { path: 'social/polls/:id', component: PollDetailComponent, canActivate: [authGuard] },
  { path: 'social/polls/:id/edit', component: EditPollComponent, canActivate: [authGuard] },
  { path: 'social/my-polls', component: MyPollsComponent, canActivate: [authGuard] },
  { path: 'social/create-post', component: CreateOutfitPostComponent, canActivate: [authGuard] },
  { path: 'social/my-posts', component: MyOutfitPostsComponent, canActivate: [authGuard] },
  { path: 'search', component: GlobalSearchComponent, canActivate: [authGuard] },
  { path: 'notifications', component: NotificationsCenterComponent },
  { path: 'settings', component: SettingsComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '' },
];
