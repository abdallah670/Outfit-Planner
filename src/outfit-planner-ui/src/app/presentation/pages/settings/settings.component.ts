import { Component, OnInit, OnDestroy, Inject, Renderer2, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DOCUMENT } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject, fromEvent } from 'rxjs';
import { takeUntil, throttleTime, filter } from 'rxjs/operators';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { UserActions } from '../../../core/state/user/user.actions';
import {
  selectAppPreferences,
  selectNotificationSettings,
  selectSettingsLoading,
  selectConnectedAccounts,
  selectConnectedAccountsLoading,
} from '../../../core/state/user/user.selectors';
import { AppPreferences } from '../../../core/services/app-preferences.service';
import { NotificationSettings } from '../../../core/services/notification-settings.service';
import { ConnectedAccount } from '../../../core/services/connected-accounts.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, MatSnackBarModule, MatButtonModule],
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsComponent implements OnInit, OnDestroy {
  // Selectors
  appPreferences$!: Observable<AppPreferences | null>;
  notificationSettings$!: Observable<NotificationSettings | null>;
  settingsLoading$!: Observable<boolean>;
  connectedAccounts$!: Observable<ConnectedAccount[]>;
  connectedAccountsLoading$!: Observable<boolean>;

  // Local state for forms
  appPreferences: AppPreferences = {
    temperatureUnit: 'Celsius',
    language: 'en',
    theme: 'Auto',
    measurementUnit: 'Metric',
  };

  notificationSettings: NotificationSettings = {
    dailyOutfitSuggestion: true,
    weeklyStyleReport: false,
    weatherAlerts: true,
    newFeatures: true,
    socialNotifications: true,
    pushNotificationsEnabled: true,
  };

  // Original values for dirty checking
  private originalAppPreferences: AppPreferences = { ...this.appPreferences };
  private originalNotificationSettings: NotificationSettings = { ...this.notificationSettings };

  // Dirty state tracking
  isAppPreferencesDirty = false;
  isNotificationSettingsDirty = false;

  // Connected accounts - must be initialized as empty array
  connectedAccounts: ConnectedAccount[] = [];
  
  // Cached connection status for template (prevents ExpressionChangedAfterItHasBeenCheckedError)
  isGoogleConnected = false;
  isFacebookConnected = false;
  googleAccount: ConnectedAccount | undefined;
  facebookAccount: ConnectedAccount | undefined;
  
  isConnectingGoogle = false;
  isConnectingFacebook = false;
  isDisconnectingGoogle = false;
  isDisconnectingFacebook = false;

  // Sidebar active section tracking
  activeSection = 'preferences';
  private sectionIds = ['preferences', 'notifications', 'connected', 'privacy'];

  private destroy$ = new Subject<void>();

  constructor(
    private store: Store,
    private renderer: Renderer2,
    private snackBar: MatSnackBar,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    @Inject(DOCUMENT) private document: Document
  ) {}

  ngOnInit(): void {
    // Initialize selectors
    this.appPreferences$ = this.store.select(selectAppPreferences);
    this.notificationSettings$ = this.store.select(selectNotificationSettings);
    this.settingsLoading$ = this.store.select(selectSettingsLoading);
    this.connectedAccounts$ = this.store.select(selectConnectedAccounts);
    this.connectedAccountsLoading$ = this.store.select(selectConnectedAccountsLoading);

    // Load settings from API
    this.store.dispatch(UserActions.loadAppPreferences());
    this.store.dispatch(UserActions.loadNotificationSettings());
    this.store.dispatch(UserActions.loadConnectedAccounts());

    // Subscribe to store values
    this.appPreferences$.pipe(takeUntil(this.destroy$)).subscribe((prefs: AppPreferences | null) => {
      if (prefs) {
        this.appPreferences = { ...prefs };
        this.originalAppPreferences = { ...prefs };
        this.checkAppPreferencesDirty();
      }
    });

    this.notificationSettings$
      .pipe(takeUntil(this.destroy$))
      .subscribe((settings: NotificationSettings | null) => {
        if (settings) {
          this.notificationSettings = { ...settings };
          this.originalNotificationSettings = { ...settings };
          this.checkNotificationSettingsDirty();
        }
      });

    this.connectedAccounts$.pipe(takeUntil(this.destroy$)).subscribe((accounts: ConnectedAccount[]) => {
      this.connectedAccounts = accounts || [];
      this.updateCachedAccountStatus();
      console.log('[Settings] Connected accounts loaded:', this.connectedAccounts);
    });

    // Setup scroll spy
    this.setupScrollSpy();

    // Handle OAuth callback query parameters
    this.route.queryParams.subscribe((params: { [key: string]: string }) => {
      if (params['connected'] === 'true') {
        const provider = params['provider'] || 'account';
        this.snackBar.open(`${provider} account connected successfully!`, 'Close', { duration: 5000 });
        // Reload connected accounts to reflect the changes
        this.store.dispatch(UserActions.loadConnectedAccounts());
        // Clear the query params by navigating (optional, for cleaner URL)
        this.cleanQueryParams();
      } else if (params['error']) {
        const errorMsg = params['error'] === 'link_failed' 
          ? 'Failed to connect account. Please try again.' 
          : params['error'];
        this.snackBar.open(errorMsg, 'Close', { duration: 5000 });
        this.cleanQueryParams();
      }
    });

    // Handle URL fragment for direct section navigation
    this.route.fragment.subscribe((fragment: string | null) => {
      if (fragment) {
        // Use setTimeout to ensure DOM is ready
        setTimeout(() => {
          this.scrollToSection(fragment);
        }, 100);
      }
    });
  }

  private cleanQueryParams(): void {
    // Remove query params without triggering navigation
    window.history.replaceState({}, '', window.location.pathname);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Smooth scroll to section
  onNavClick(event: Event, sectionId: string): void {
    event.preventDefault();
    this.scrollToSection(sectionId);
    this.activeSection = sectionId;
  }

  // Scroll to a specific section by ID
  private scrollToSection(sectionId: string): void {
    const element = this.document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      this.activeSection = sectionId;
    }
  }

  // Setup scroll spy to update active section
  private setupScrollSpy(): void {
    fromEvent(window, 'scroll')
      .pipe(throttleTime(100), takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateActiveSection();
      });
  }

  // Update active section based on scroll position
  private updateActiveSection(): void {
    const scrollPosition = window.scrollY + 100; // Offset for header

    for (const sectionId of this.sectionIds) {
      const element = this.document.getElementById(sectionId);
      if (element) {
        const offsetTop = element.offsetTop;
        const offsetHeight = element.offsetHeight;

        if (scrollPosition >= offsetTop && scrollPosition < offsetTop + offsetHeight) {
          this.activeSection = sectionId;
          break;
        }
      }
    }
  }

  // Check if app preferences have changed
  private checkAppPreferencesDirty(): void {
    this.isAppPreferencesDirty =
      JSON.stringify(this.appPreferences) !== JSON.stringify(this.originalAppPreferences);
    this.cdr.markForCheck();
  }

  // Check if notification settings have changed
  private checkNotificationSettingsDirty(): void {
    this.isNotificationSettingsDirty =
      JSON.stringify(this.notificationSettings) !== JSON.stringify(this.originalNotificationSettings);
    this.cdr.markForCheck();
  }

  // App Preferences Handlers
  onTemperatureUnitChange(unit: 'Celsius' | 'Fahrenheit'): void {
    this.appPreferences.temperatureUnit = unit;
    this.checkAppPreferencesDirty();
  }

  onLanguageChange(language: string): void {
    this.appPreferences.language = language;
    this.checkAppPreferencesDirty();
  }

  onThemeChange(theme: 'Light' | 'Dark' | 'Auto'): void {
    this.appPreferences.theme = theme;
    this.checkAppPreferencesDirty();
  }

  onMeasurementUnitChange(unit: 'Metric' | 'Imperial'): void {
    this.appPreferences.measurementUnit = unit;
    this.checkAppPreferencesDirty();
  }

  // Save App Preferences
  saveAppPreferences(): void {
    if (!this.isAppPreferencesDirty) return;

    // Save current values before dispatching
    const savedPrefs = { ...this.appPreferences };

    this.store.dispatch(
      UserActions.updateAppPreferences({
        request: savedPrefs,
      })
    );

    // Immediately update original values to match what we just saved
    // This clears the dirty state right away for better UX
    this.originalAppPreferences = savedPrefs;
    this.isAppPreferencesDirty = false;
    this.snackBar.open('Preferences saved successfully!', 'Close', { duration: 3000 });
    this.cdr.markForCheck();
  }

  // Notification Settings Handlers
  onDailyOutfitSuggestionToggle(enabled: boolean): void {
    this.notificationSettings.dailyOutfitSuggestion = enabled;
    this.checkNotificationSettingsDirty();
  }

  onWeeklyStyleReportToggle(enabled: boolean): void {
    this.notificationSettings.weeklyStyleReport = enabled;
    this.checkNotificationSettingsDirty();
  }

  onWeatherAlertsToggle(enabled: boolean): void {
    this.notificationSettings.weatherAlerts = enabled;
    this.checkNotificationSettingsDirty();
  }

  onNewFeaturesToggle(enabled: boolean): void {
    this.notificationSettings.newFeatures = enabled;
    this.checkNotificationSettingsDirty();
  }

  onSocialNotificationsToggle(enabled: boolean): void {
    this.notificationSettings.socialNotifications = enabled;
    this.checkNotificationSettingsDirty();
  }

  onPushNotificationsToggle(enabled: boolean): void {
    this.notificationSettings.pushNotificationsEnabled = enabled;
    this.checkNotificationSettingsDirty();
  }

  // Save Notification Settings
  saveNotificationSettings(): void {
    if (!this.isNotificationSettingsDirty) {
      this.snackBar.open('No changes to save', 'Close', { duration: 2000 });
      return;
    }

    // Save current values before dispatching
    const savedSettings = { ...this.notificationSettings };

    this.store.dispatch(
      UserActions.updateNotificationSettings({
        request: savedSettings,
      })
    );

    // Immediately update original values to match what we just saved
    // This clears the dirty state right away for better UX
    this.originalNotificationSettings = savedSettings;
    this.isNotificationSettingsDirty = false;
    this.snackBar.open('Notification settings saved successfully!', 'Close', { duration: 3000 });
    this.cdr.markForCheck();
  }

  // Update cached account status (called when accounts change)
  private updateCachedAccountStatus(): void {
    this.isGoogleConnected = this.connectedAccounts.some(
      (account) => account.provider?.toLowerCase() === 'google'
    );
    this.isFacebookConnected = this.connectedAccounts.some(
      (account) => account.provider?.toLowerCase() === 'facebook'
    );
    this.googleAccount = this.connectedAccounts.find(
      (account) => account.provider?.toLowerCase() === 'google'
    );
    this.facebookAccount = this.connectedAccounts.find(
      (account) => account.provider?.toLowerCase() === 'facebook'
    );
  }

  // Connected Accounts - Case insensitive provider matching
  // NOTE: These methods are kept for backwards compatibility but template should use cached properties
  isAccountConnected(provider: string): boolean {
    return this.connectedAccounts.some((account) => 
      account.provider?.toLowerCase() === provider.toLowerCase()
    );
  }

  getConnectedAccount(provider: string): ConnectedAccount | undefined {
    return this.connectedAccounts.find((account) => 
      account.provider?.toLowerCase() === provider.toLowerCase()
    );
  }

  connectGoogle(): void {
    this.isConnectingGoogle = true;
    this.store.dispatch(UserActions.connectAccount({ provider: 'Google' }));
    setTimeout(() => {
      this.isConnectingGoogle = false;
    }, 2000);
  }

  connectFacebook(): void {
    this.isConnectingFacebook = true;
    this.store.dispatch(UserActions.connectAccount({ provider: 'Facebook' }));
    setTimeout(() => {
      this.isConnectingFacebook = false;
    }, 2000);
  }

  disconnectGoogle(): void {
    this.isDisconnectingGoogle = true;
    this.store.dispatch(UserActions.disconnectAccount({ provider: 'Google' }));
    setTimeout(() => {
      this.isDisconnectingGoogle = false;
      this.snackBar.open('Google account disconnected', 'Close', { duration: 3000 });
    }, 1500);
  }

  disconnectFacebook(): void {
    this.isDisconnectingFacebook = true;
    this.store.dispatch(UserActions.disconnectAccount({ provider: 'Facebook' }));
    setTimeout(() => {
      this.isDisconnectingFacebook = false;
      this.snackBar.open('Facebook account disconnected', 'Close', { duration: 3000 });
    }, 1500);
  }

  // Account Management
  onExportData(): void {
    // Show loading message
    this.snackBar.open('Preparing your data export...', 'Close', { duration: 2000 });

    // Use Angular HttpClient to properly go through the interceptor
    this.store.dispatch(UserActions.exportUserData());
  }

  onDeleteAccount(): void {
    // Show confirmation dialog
    const confirmed = confirm(
      'WARNING: This action cannot be undone!\n\n' +
      'This will permanently delete:\n' +
      '- Your account and profile\n' +
      '- All your clothing items\n' +
      '- All your outfits\n' +
      '- All your wear history\n' +
      '- All your preferences and settings\n\n' +
      'Are you absolutely sure you want to delete your account?'
    );

    if (!confirmed) {
      return;
    }

    // Double confirm
    const doubleConfirmed = confirm(
      'FINAL WARNING: All your data will be permanently lost!\n\n' +
      'Type "DELETE" in your mind and click OK to proceed.'
    );

    if (!doubleConfirmed) {
      return;
    }

    this.snackBar.open('Deleting your account...', 'Close', { duration: 2000 });

    // Use Angular HttpClient through the store to properly go through the interceptor
    this.store.dispatch(UserActions.deleteAccount());
  }
}
