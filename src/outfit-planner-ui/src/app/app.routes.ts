import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { adminGuard } from './core/guards/admin-guard';

export const routes: Routes = [
  { 
    path: '', 
    loadComponent: () => import('./presentation/pages/home/home.component').then(m => m.HomeComponent), 
    canActivate: [authGuard] 
  },
  { path: 'home', redirectTo: '', pathMatch: 'full' },
  
  // Auth routes - lazy loaded
  { 
    path: 'login', 
    loadComponent: () => import('./presentation/pages/auth/login/login').then(m => m.Login) 
  },
  { 
    path: 'register', 
    loadComponent: () => import('./presentation/pages/auth/register/register').then(m => m.Register) 
  },
  { 
    path: 'auth/callback', 
    loadComponent: () => import('./presentation/pages/auth/auth-callback/auth-callback.component').then(m => m.AuthCallbackComponent) 
  },
  { 
    path: 'verify-email', 
    loadComponent: () => import('./presentation/pages/auth/verify-email/verify-email.component').then(m => m.VerifyEmailComponent) 
  },
  { 
    path: 'forgot-password', 
    loadComponent: () => import('./presentation/pages/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent) 
  },
  { 
    path: 'reset-password', 
    loadComponent: () => import('./presentation/pages/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent) 
  },
  
  // Profile routes - lazy loaded
  { 
    path: 'profile', 
    loadComponent: () => import('./presentation/pages/profile/profile/profile.component').then(m => m.ProfileComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'profile/stats', 
    loadComponent: () => import('./presentation/pages/profile/profile-stats/profile-stats.component').then(m => m.ProfileStatsComponent), 
    canActivate: [authGuard] 
  },
  
  // Wardrobe routes - lazy loaded
  { 
    path: 'wardrobe', 
    loadComponent: () => import('./presentation/pages/wardrobe/wardrobe-dashboard/wardrobe-dashboard.component').then(m => m.WardrobeDashboardComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'wardrobe/new', 
    loadComponent: () => import('./presentation/pages/wardrobe/add-clothing-item/add-clothing-item.component').then(m => m.AddClothingItemComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'wardrobe/edit/:id', 
    loadComponent: () => import('./presentation/pages/wardrobe/add-clothing-item/add-clothing-item.component').then(m => m.AddClothingItemComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'wardrobe/:id', 
    loadComponent: () => import('./presentation/pages/wardrobe/clothing-item-detail/clothing-item-detail').then(m => m.ClothingItemDetail), 
    canActivate: [authGuard] 
  },
  
  // Outfits routes - lazy loaded
  { 
    path: 'outfits', 
    loadComponent: () => import('./presentation/pages/outfits/outfits-dashboard/outfits-dashboard.component').then(m => m.OutfitsDashboardComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'outfits/build', 
    loadComponent: () => import('./presentation/pages/outfits/outfit-builder/outfit-builder.component').then(m => m.OutfitBuilderComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'outfits/build/:id', 
    loadComponent: () => import('./presentation/pages/outfits/outfit-builder/outfit-builder.component').then(m => m.OutfitBuilderComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'outfits/today', 
    loadComponent: () => import('./presentation/pages/daily-suggestion/daily-suggestion.component').then(m => m.DailySuggestionComponent) 
    , canActivate: [authGuard] 
  },
  { 
    path: 'outfits/:id', 
    loadComponent: () => import('./presentation/pages/outfits/outfit-detail/outfit-detail.component').then(m => m.OutfitDetailComponent), 
    canActivate: [authGuard] 
  },
  
  // Social routes - lazy loaded
  { 
    path: 'social', 
    loadComponent: () => import('./presentation/pages/social/community-feed/community-feed.component').then(m => m.CommunityFeedComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'social/feed', 
    loadComponent: () => import('./presentation/pages/social/community-feed/community-feed.component').then(m => m.CommunityFeedComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'social/create-poll', 
    loadComponent: () => import('./presentation/pages/social/create-poll/create-poll.component').then(m => m.CreatePollComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'social/profile/:userId', 
    loadComponent: () => import('./presentation/pages/profile/public-profile/public-profile.component').then(m => m.PublicProfileComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'profile/:userId', 
    loadComponent: () => import('./presentation/pages/profile/public-profile/public-profile.component').then(m => m.PublicProfileComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'social/polls/:id', 
    loadComponent: () => import('./presentation/pages/social/poll-detail/poll-detail.component').then(m => m.PollDetailComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'social/polls/:id/edit', 
    loadComponent: () => import('./presentation/pages/social/create-poll/create-poll.component').then(m => m.CreatePollComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'social/my-polls', 
    loadComponent: () => import('./presentation/pages/social/my-polls/my-polls.component').then(m => m.MyPollsComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'social/create-post', 
    loadComponent: () => import('./presentation/pages/social/create-outfit-post/create-outfit-post.component').then(m => m.CreateOutfitPostComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'social/posts/:id', 
    loadComponent: () => import('./presentation/pages/social/outfit-post-detail/outfit-post-detail.component').then(m => m.OutfitPostDetailComponent), 
    canActivate: [authGuard] 
  },
  { 
    path: 'social/posts/:id/edit', 
    loadComponent: () => import('./presentation/pages/social/create-outfit-post/create-outfit-post.component').then(m => m.CreateOutfitPostComponent), 
    canActivate: [authGuard] 
  },
  
  // AI routes - lazy loaded
  { 
    path: 'ai-assistant', 
    loadComponent: () => import('./presentation/pages/ai/ai-assistant/ai-assistant.component').then(m => m.AiAssistantComponent), 
    canActivate: [authGuard], 
    title: 'AI Fashion Assistant' 
  },
  { 
    path: 'inspiration', 
    loadComponent: () => import('./presentation/pages/ai/ai-assistant/ai-assistant.component').then(m => m.AiAssistantComponent), 
    canActivate: [authGuard], 
    title: 'AI Fashion Inspiration' 
  },
  { 
    path: 'ai', 
    loadComponent: () => import('./presentation/pages/ai/ai-assistant/ai-assistant.component').then(m => m.AiAssistantComponent), 
    canActivate: [authGuard], 
    title: 'AI Fashion Assistant' 
  },
  
  // Other routes
  { path: 'calendar', loadComponent: () => import('./presentation/pages/calendar/calendar.component').then(m => m.CalendarComponent) },
  { path: 'search', loadComponent: () => import('./presentation/pages/global-search/global-search.component').then(m => m.GlobalSearchComponent), canActivate: [authGuard] },
  { path: 'notifications', loadComponent: () => import('./presentation/pages/notifications-center/notifications-center.component').then(m => m.NotificationsCenterComponent) },
  { path: 'settings', loadComponent: () => import('./presentation/pages/settings/settings.component').then(m => m.SettingsComponent), canActivate: [authGuard] },
  
  // Admin routes - lazy loaded
  { 
    path: 'admin', 
    loadComponent: () => import('./presentation/layouts/admin-layout/admin-layout.component').then(m => m.AdminLayoutComponent)
  },
  { path: 'admin/dashboard', loadComponent: () => import('./presentation/pages/admin/dashboard/admin-dashboard.component').
    then(m => m.AdminDashboardComponent) , canActivate: [adminGuard] },
  { path: 'admin/users', loadComponent: () => import('./presentation/pages/admin/users/admin-users.component').then(m => m.AdminUsersComponent) , canActivate: [adminGuard]},
  { path: 'admin/reports', loadComponent: () => import('./presentation/pages/admin/reports/admin-reports.component').then(m => m.AdminReportsComponent), canActivate: [adminGuard] },
  { path: 'admin/settings', loadComponent: () => import('./presentation/pages/admin/settings/admin-settings.component').then(m => m.AdminSettingsComponent), canActivate: [adminGuard] },
  { path: 'admin/audit-logs', loadComponent: () => import('./presentation/pages/admin/audit-logs/admin-audit-logs.component').then(m => m.AdminAuditLogsComponent), canActivate: [adminGuard] },
  { path: 'admin/content/posts', loadComponent: () => import('./presentation/pages/admin/content/admin-posts.component').then(m => m.AdminPostsComponent) , canActivate: [adminGuard]},
  { path: 'admin/content/polls', loadComponent: () => import('./presentation/pages/admin/content/admin-polls.component').then(m => m.AdminPollsComponent) , canActivate: [adminGuard]},
  { path: 'admin/content/outfits', loadComponent: () => import('./presentation/pages/admin/content/admin-outfits.component').then(m => m.AdminOutfitsComponent) , canActivate: [adminGuard]},
  { path: 'admin/analytics', loadComponent: () => import('./presentation/pages/admin/analytics/admin-analytics.component').then(m => m.AdminAnalyticsComponent) , canActivate: [adminGuard]},
  { path: 'admin/system', loadComponent: () => import('./presentation/pages/admin/system/admin-system.component').then(m => m.AdminSystemComponent), canActivate: [adminGuard] },
  
  { path: '**', redirectTo: '' },
];
