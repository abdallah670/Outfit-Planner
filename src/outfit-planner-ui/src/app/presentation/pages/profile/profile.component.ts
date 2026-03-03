import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import {
  UserProfile,
  StylePreference,
  PrivacyLevel,
  UserStyleProfile,
  UserPreferences,
} from '../../../domain/entities/user-profile.entity';
import { UserActions } from '../../../core/state/user/user.actions';
import {
  selectUserProfile,
  selectUserLoading,
  selectUserUpdating,
  selectUploadingPicture,
  selectChangingPassword,
  selectUserError,
  selectUserStats,
  selectStyleProfile,
  selectUserPreferences,
} from '../../../core/state/user/user.selectors';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="profile-container">
      <!-- Header -->
      <div class="profile-header">
        <h1>My Profile</h1>
      </div>

      <!-- Loading State -->
      <div *ngIf="loading$ | async" class="glass-card loading-state">
        <div class="spinner-small"></div>
        <p>Loading profile...</p>
      </div>

      <!-- Error State -->
      <div *ngIf="error$ | async as error" class="glass-card error-message">
        <span>{{ error }}</span>
        <button class="btn-icon" (click)="clearError()" aria-label="Dismiss error">✕</button>
      </div>

      <ng-container *ngIf="profile$ | async as profile">
        <!-- Left Column: Profile Card & Stats -->
        <div class="left-column">
          <!-- Main Profile Card -->
          <div class="glass-card profile-card">
            <div class="profile-picture-container">
              <div class="profile-picture">
                <img
                  [src]="$any(profile).profilePictureUrl || 'assets/default-avatar.png'"
                  [alt]="$any(profile).name"
                  (error)="onImageError($event)"
                />
                <div class="upload-overlay" *ngIf="!(uploadingPicture$ | async)">
                  <input
                    type="file"
                    #fileInput
                    accept="image/*"
                    (change)="onFileSelected($event)"
                    hidden
                  />
                  <button class="upload-btn" (click)="fileInput.click()">
                    <svg
                      width="20"
                      height="20"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      stroke-width="2"
                      stroke-linecap="round"
                      stroke-linejoin="round"
                    >
                      <path
                        d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"
                      ></path>
                      <circle cx="12" cy="13" r="4"></circle>
                    </svg>
                  </button>
                </div>
                <div class="uploading-spinner" *ngIf="uploadingPicture$ | async">
                  <div class="spinner-small"></div>
                </div>
              </div>
            </div>

            <div class="profile-info">
              <h2 class="gradient-text">{{ $any(profile).name }}</h2>
              <p class="email">{{ $any(profile).email }}</p>
              <div class="member-badge">Member since {{ $any(stats$ | async)?.memberSince }}</div>
            </div>
          </div>

          <!-- Stats Grid inside Left Column -->
          <div class="stats-bento" *ngIf="stats$ | async as stats">
            <div class="glass-card stat-card">
              <span class="stat-value gradient-text-alt">{{ $any(stats).wardrobeItemCount }}</span>
              <span class="stat-label">Items</span>
            </div>
            <div class="glass-card stat-card">
              <span class="stat-value gradient-text-alt">{{ $any(stats).outfitCount }}</span>
              <span class="stat-label">Outfits</span>
            </div>
            <div class="glass-card stat-card">
              <span class="stat-value gradient-text-alt">{{ $any(stats).totalWears }}</span>
              <span class="stat-label">Wears</span>
            </div>
          </div>
        </div>

        <!-- Right Column: Forms and Settings -->
        <div class="right-column">
          <!-- Edit Profile Form -->
          <div class="glass-card form-section">
            <div class="section-header">
              <h3>Personal Info</h3>
            </div>
            <form (ngSubmit)="onUpdateProfile()">
              <div class="form-group custom-input full-width">
                <label for="name">Full Name</label>
                <input
                  type="text"
                  id="name"
                  [(ngModel)]="editName"
                  name="name"
                  required
                  placeholder="Your name"
                />
              </div>

              <div class="form-actions full-width">
                <button type="submit" class="btn-primary" [disabled]="updating$ | async">
                  {{ (updating$ | async) ? 'Saving...' : 'Save Changes' }}
                </button>
              </div>
            </form>
          </div>

          <!-- Style Profile -->
          <div class="glass-card form-section" *ngIf="styleProfile$ | async as style">
            <div class="section-header">
              <h3>Style Profile</h3>
            </div>
            <form (ngSubmit)="onUpdateStyleProfile()" class="grid-form">
              <div class="form-group custom-input">
                <label for="style">Core Style</label>
                <div class="select-wrapper">
                  <select id="style" [(ngModel)]="editStyle.style" name="style">
                    <option *ngFor="let s of stylePreferences" [value]="s">{{ s }}</option>
                  </select>
                </div>
              </div>

              <div class="form-group custom-input">
                <label for="fitPreferences">Fit Preference</label>
                <input
                  type="text"
                  id="fitPreferences"
                  [(ngModel)]="editStyle.fitPreferences"
                  name="fitPreferences"
                  placeholder="e.g., Loose, Fitted, Oversized"
                />
              </div>

              <div class="form-group range-group full-width">
                <div class="range-header">
                  <label for="comfortPriority">Comfort Priority</label>
                  <span class="range-value">{{ editStyle.comfortPriority }}%</span>
                </div>
                <input
                  type="range"
                  id="comfortPriority"
                  [(ngModel)]="editStyle.comfortPriority"
                  name="comfortPriority"
                  min="0"
                  max="100"
                  class="custom-range"
                />
              </div>

              <div class="form-group custom-toggle full-width">
                <label class="toggle-label">
                  <div class="toggle-info">
                    <span class="toggle-title">Open to Fashion Trends</span>
                    <span class="toggle-desc">Incorporate trendy items into suggestions</span>
                  </div>
                  <div class="toggle-switch">
                    <input
                      type="checkbox"
                      [(ngModel)]="editStyle.acceptsTrends"
                      name="acceptsTrends"
                    />
                    <span class="slider"></span>
                  </div>
                </label>
              </div>

              <div class="form-actions full-width">
                <button type="submit" class="btn-primary">Update Style</button>
              </div>
            </form>
          </div>

          <!-- Privacy Settings -->
          <div class="glass-card form-section" *ngIf="preferences$ | async as prefs">
            <div class="section-header">
              <h3>Privacy Settings</h3>
            </div>
            <form (ngSubmit)="onUpdatePreferences()" class="grid-form">
              <div class="form-group custom-toggle full-width">
                <label class="toggle-label">
                  <div class="toggle-info">
                    <span class="toggle-title">Anonymous Sharing</span>
                    <span class="toggle-desc">Share outfits without your profile link</span>
                  </div>
                  <div class="toggle-switch">
                    <input
                      type="checkbox"
                      [(ngModel)]="editPreferences.shareOutfitsAnonymously"
                      name="shareOutfitsAnonymously"
                    />
                    <span class="slider"></span>
                  </div>
                </label>
              </div>

              <div class="form-group custom-toggle full-width">
                <label class="toggle-label">
                  <div class="toggle-info">
                    <span class="toggle-title">Trend Analysis</span>
                    <span class="toggle-desc">Contribute data to global trend analysis</span>
                  </div>
                  <div class="toggle-switch">
                    <input
                      type="checkbox"
                      [(ngModel)]="editPreferences.includeInTrendAnalysis"
                      name="includeInTrendAnalysis"
                    />
                    <span class="slider"></span>
                  </div>
                </label>
              </div>

              <div class="form-group custom-input full-width">
                <label for="defaultPrivacy">Default Outfit Privacy</label>
                <div class="select-wrapper">
                  <select
                    id="defaultPrivacy"
                    [(ngModel)]="editPreferences.defaultOutfitPrivacy"
                    name="defaultPrivacy"
                  >
                    <option *ngFor="let p of privacyLevels" [value]="p">{{ p }}</option>
                  </select>
                </div>
              </div>

              <div class="form-actions full-width">
                <button type="submit" class="btn-primary">Update Privacy</button>
              </div>
            </form>
          </div>

          <!-- Security Settings -->
          <div class="glass-card form-section security-section">
            <div class="section-header">
              <h3>Security</h3>
            </div>
            <form (ngSubmit)="onChangePassword()" class="grid-form">
              <div class="form-group custom-input full-width">
                <label for="currentPassword">Current Password</label>
                <input
                  type="password"
                  id="currentPassword"
                  [(ngModel)]="passwordForm.currentPassword"
                  name="currentPassword"
                  required
                  placeholder="Enter current password"
                />
              </div>

              <div class="form-group custom-input">
                <label for="newPassword">New Password</label>
                <input
                  type="password"
                  id="newPassword"
                  [(ngModel)]="passwordForm.newPassword"
                  name="newPassword"
                  required
                  minlength="6"
                  placeholder="New password"
                />
              </div>

              <div class="form-group custom-input">
                <label for="confirmPassword">Confirm Password</label>
                <input
                  type="password"
                  id="confirmPassword"
                  [(ngModel)]="passwordForm.confirmPassword"
                  name="confirmPassword"
                  required
                  placeholder="Confirm new password"
                />
              </div>

              <div class="form-actions full-width">
                <button type="submit" class="btn-danger" [disabled]="changingPassword$ | async">
                  {{ (changingPassword$ | async) ? 'Changing...' : 'Change Password' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </ng-container>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        min-height: calc(100vh - 80px);
        position: relative;
      }

      .profile-header {
        margin-bottom: 30px;
        grid-column: 1 / -1;
      }

      .profile-header h1 {
        margin: 0;
        color: var(--text-main);
        font-size: 32px;
        font-weight: 700;
        letter-spacing: -1px;
      }

      .profile-container {
        max-width: 1100px;
        margin: 0 auto;
        padding: 40px 20px;
        display: grid;
        grid-template-columns: 320px 1fr;
        gap: 30px;
        align-items: start;
      }

      @media (max-width: 900px) {
        .profile-container {
          grid-template-columns: 1fr;
        }
      }

      /* Glassmorphism Classes */
      .glass-card {
        background: rgba(255, 255, 255, 0.6);
        border: 1px solid rgba(255, 255, 255, 0.8);
        border-radius: var(--radius-xl);
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.05);
        padding: 30px;
        transition:
          transform 0.3s ease,
          box-shadow 0.3s ease;
        position: relative;
        z-index: 1;
      }

      .glass-card:hover {
        transform: translateY(-2px);
        box-shadow: 0 12px 40px rgba(0, 0, 0, 0.08);
      }

      .gradient-text {
        background: linear-gradient(135deg, var(--accent-dark) 0%, var(--accent-pink) 100%);
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
      }

      .gradient-text-alt {
        background: linear-gradient(135deg, var(--accent-green) 0%, var(--accent-dark) 100%);
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
      }

      /* Left Column - Profile Card */
      .left-column {
        display: flex;
        flex-direction: column;
        gap: 20px;
      }

      .profile-card {
        text-align: center;
        padding: 40px 30px;
      }

      .profile-picture-container {
        display: flex;
        justify-content: center;
        margin-bottom: 24px;
      }

      .profile-picture {
        position: relative;
        width: 140px;
        height: 140px;
        border-radius: 50%;
        padding: 6px;
        background: linear-gradient(135deg, var(--accent-pink), var(--accent-green));
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
      }

      .profile-picture img {
        width: 100%;
        height: 100%;
        border-radius: 50%;
        object-fit: cover;
        border: 4px solid white;
        background: white;
      }

      .upload-overlay {
        position: absolute;
        bottom: 0px;
        right: 0px;
        z-index: 2;
      }

      .upload-btn {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        background: white;
        border: 1px solid rgba(0, 0, 0, 0.1);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        color: var(--accent-dark);
        transition:
          background 0.2s ease,
          color 0.2s ease;
      }

      .upload-btn:hover {
        background: var(--bg-primary);
        color: var(--accent-pink);
      }

      .uploading-spinner,
      .loading-state {
        position: absolute;
        inset: 6px;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.8);
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        backdrop-filter: blur(4px);
      }

      .loading-state {
        position: static;
        grid-column: 1 / -1;
        padding: 40px;
        gap: 16px;
        color: var(--text-muted);
      }

      .spinner-small {
        width: 30px;
        height: 30px;
        border: 3px solid rgba(0, 0, 0, 0.1);
        border-top-color: var(--accent-pink);
        border-radius: 50%;
        animation: spin 1s linear infinite;
      }

      @keyframes spin {
        to {
          transform: rotate(360deg);
        }
      }

      .profile-info h2 {
        font-size: 28px;
        font-weight: 700;
        margin: 0 0 8px 0;
        letter-spacing: -0.5px;
      }

      .profile-info .email {
        color: var(--text-muted);
        font-size: 15px;
        margin: 0 0 16px 0;
      }

      .member-badge {
        display: inline-block;
        padding: 6px 16px;
        background: rgba(0, 0, 0, 0.04);
        border-radius: 20px;
        font-size: 13px;
        color: var(--text-dim);
        font-weight: 500;
      }

      /* Stats Bento */
      .stats-bento {
        display: grid;
        grid-template-columns: 1fr;
        gap: 15px;
      }

      .stats-bento .stat-card {
        padding: 20px;
        display: flex;
        justify-content: space-between;
        align-items: center;
      }

      .stats-bento .stat-value {
        font-size: 28px;
        font-weight: 700;
        line-height: 1;
      }

      .stats-bento .stat-label {
        font-size: 14px;
        color: var(--text-muted);
        font-weight: 500;
      }

      /* Right Column */
      .right-column {
        display: flex;
        flex-direction: column;
        gap: 25px;
      }

      .section-header {
        margin-bottom: 24px;
        padding-bottom: 16px;
        border-bottom: 1px solid rgba(0, 0, 0, 0.06);
      }

      .section-header h3 {
        margin: 0;
        font-size: 20px;
        font-weight: 600;
        color: var(--text-main);
      }

      /* Forms */
      .grid-form {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 24px;
      }

      .form-section form {
        display: grid;
        grid-template-columns: 1fr;
        gap: 24px;
      }

      .form-section form.grid-form {
        grid-template-columns: 1fr 1fr;
      }

      .full-width {
        grid-column: 1 / -1;
      }

      @media (max-width: 600px) {
        .grid-form,
        .form-section form.grid-form {
          grid-template-columns: 1fr;
        }
      }

      .custom-input label,
      .range-header label {
        display: block;
        font-size: 14px;
        font-weight: 600;
        color: var(--text-muted);
        margin-bottom: 8px;
      }

      .custom-input input,
      .custom-input select {
        width: 100%;
        padding: 14px 16px;
        background: rgba(255, 255, 255, 0.5);
        border: 1px solid rgba(0, 0, 0, 0.1);
        border-radius: 12px;
        font-size: 15px;
        color: var(--text-main);
        transition: all 0.2s ease;
        font-family: inherit;
        box-sizing: border-box;
      }

      .custom-input input:focus,
      .custom-input select:focus {
        outline: none;
        border-color: var(--accent-pink);
        background: white;
        box-shadow: 0 0 0 3px rgba(248, 180, 196, 0.2);
      }

      .select-wrapper {
        position: relative;
      }

      .select-wrapper::after {
        content: '▼';
        font-size: 10px;
        color: var(--text-dim);
        position: absolute;
        right: 16px;
        top: 50%;
        transform: translateY(-50%);
        pointer-events: none;
      }

      /* Range Slider */
      .range-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 12px;
      }

      .range-value {
        font-weight: 700;
        color: var(--accent-pink);
      }

      .custom-range {
        width: 100%;
        height: 6px;
        background: rgba(0, 0, 0, 0.06);
        border-radius: 4px;
        outline: none;
        -webkit-appearance: none;
      }

      .custom-range::-webkit-slider-thumb {
        -webkit-appearance: none;
        appearance: none;
        width: 20px;
        height: 20px;
        border-radius: 50%;
        background: white;
        border: 2px solid var(--accent-pink);
        cursor: pointer;
        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
        transition: transform 0.1s ease;
      }

      .custom-range::-webkit-slider-thumb:hover {
        transform: scale(1.1);
      }

      /* Toggles */
      .custom-toggle {
        padding: 16px;
        background: rgba(255, 255, 255, 0.4);
        border-radius: 12px;
        border: 1px solid rgba(0, 0, 0, 0.05);
        box-sizing: border-box;
      }

      .toggle-label {
        display: flex;
        justify-content: space-between;
        align-items: center;
        cursor: pointer;
        width: 100%;
      }

      .toggle-info {
        display: flex;
        flex-direction: column;
        gap: 4px;
      }

      .toggle-title {
        font-weight: 600;
        font-size: 15px;
        color: var(--text-main);
      }

      .toggle-desc {
        font-size: 13px;
        color: var(--text-muted);
      }

      .toggle-switch {
        position: relative;
        width: 50px;
        height: 28px;
      }

      .toggle-switch input {
        opacity: 0;
        width: 0;
        height: 0;
      }

      .slider {
        position: absolute;
        cursor: pointer;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: rgba(0, 0, 0, 0.1);
        transition: 0.4s;
        border-radius: 34px;
      }

      .slider:before {
        position: absolute;
        content: '';
        height: 20px;
        width: 20px;
        left: 4px;
        bottom: 4px;
        background-color: white;
        transition: 0.4s;
        border-radius: 50%;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      }

      input:checked + .slider {
        background-color: var(--accent-green);
      }

      input:checked + .slider:before {
        transform: translateX(22px);
      }

      /* Buttons */
      .form-actions {
        display: flex;
        justify-content: flex-end;
        margin-top: 16px;
      }

      .btn-primary {
        background: var(--accent-pink);
        color: white;
        border: none;
        padding: 14px 28px;
        border-radius: 12px;
        font-size: 15px;
        font-weight: 600;
        cursor: pointer;
        transition: all 0.3s ease;
        box-shadow: 0 4px 12px rgba(248, 180, 196, 0.4);
      }

      .btn-primary:hover:not(:disabled) {
        transform: translateY(-2px);
        box-shadow: 0 6px 16px rgba(0, 0, 0, 0.15);
      }

      .btn-primary:disabled {
        background: #ccc;
        cursor: not-allowed;
        transform: none;
        box-shadow: none;
      }

      .btn-danger {
        background: #e17055;
        color: white;
        border: none;
        padding: 14px 28px;
        border-radius: 12px;
        font-size: 15px;
        font-weight: 600;
        cursor: pointer;
        transition: all 0.3s ease;
        box-shadow: 0 4px 12px rgba(225, 112, 85, 0.2);
      }

      .btn-danger:hover:not(:disabled) {
        transform: translateY(-2px);
        box-shadow: 0 6px 16px rgba(214, 48, 49, 0.3);
      }

      .btn-danger:disabled {
        background: #ccc;
        cursor: not-allowed;
        transform: none;
        box-shadow: none;
      }

      /* Alerts */
      .error-message {
        border-left: 4px solid #ff7675;
        color: #d63031;
        background: rgba(255, 118, 117, 0.1);
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 16px 24px;
        margin-bottom: 24px;
        grid-column: 1 / -1;
      }

      .btn-icon {
        background: transparent;
        border: none;
        color: inherit;
        font-size: 20px;
        cursor: pointer;
        padding: 4px;
      }
    `,
  ],
})
export class ProfileComponent implements OnInit {
  profile$: Observable<UserProfile | null>;
  loading$: Observable<boolean>;
  updating$: Observable<boolean>;
  uploadingPicture$: Observable<boolean>;
  changingPassword$: Observable<boolean>;
  error$: Observable<string | null>;
  stats$: Observable<{
    wardrobeItemCount: number;
    outfitCount: number;
    totalWears: number;
    memberSince: string;
  } | null>;
  styleProfile$: Observable<UserStyleProfile | undefined>;
  preferences$: Observable<UserPreferences | undefined>;

  // Form models
  editName = '';
  editStyle: UserStyleProfile = {
    style: StylePreference.Casual,
    preferredColors: [],
    fitPreferences: '',
    comfortPriority: 50,
    acceptsTrends: false,
  };
  editPreferences: UserPreferences = {
    shareOutfitsAnonymously: false,
    includeInTrendAnalysis: true,
    allowFriendRequests: true,
    defaultOutfitPrivacy: PrivacyLevel.Private,
    showBodyMetrics: false,
    allowLocationTracking: true,
  };
  passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  };

  stylePreferences = Object.values(StylePreference);
  privacyLevels = Object.values(PrivacyLevel);

  constructor(private store: Store) {
    this.profile$ = this.store.select(selectUserProfile);
    this.loading$ = this.store.select(selectUserLoading);
    this.updating$ = this.store.select(selectUserUpdating);
    this.uploadingPicture$ = this.store.select(selectUploadingPicture);
    this.changingPassword$ = this.store.select(selectChangingPassword);
    this.error$ = this.store.select(selectUserError);
    this.stats$ = this.store.select(selectUserStats);
    this.styleProfile$ = this.store.select(selectStyleProfile);
    this.preferences$ = this.store.select(selectUserPreferences);
  }

  ngOnInit() {
    this.store.dispatch(UserActions.loadProfile());

    // Subscribe to profile to initialize form values
    this.profile$.subscribe((profile: UserProfile | null) => {
      if (profile) {
        this.editName = profile.name;
        if (profile.styleProfile) {
          this.editStyle = { ...profile.styleProfile };
        }
        if (profile.preferences) {
          this.editPreferences = { ...profile.preferences };
        }
      }
    });
  }

  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    img.src = 'assets/default-avatar.png';
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.store.dispatch(UserActions.uploadProfilePicture({ file: input.files[0] }));
    }
  }

  onUpdateProfile() {
    this.store.dispatch(
      UserActions.updateProfile({
        request: { name: this.editName },
      }),
    );
  }

  onUpdateStyleProfile() {
    this.store.dispatch(
      UserActions.updateProfile({
        request: {
          name: this.editName,
          styleProfile: this.editStyle,
        },
      }),
    );
  }

  onUpdatePreferences() {
    this.store.dispatch(
      UserActions.updateProfile({
        request: {
          name: this.editName,
          preferences: this.editPreferences,
        },
      }),
    );
  }

  onChangePassword() {
    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      alert('Passwords do not match');
      return;
    }

    this.store.dispatch(
      UserActions.changePassword({
        request: {
          currentPassword: this.passwordForm.currentPassword,
          newPassword: this.passwordForm.newPassword,
          confirmPassword: this.passwordForm.confirmPassword,
        },
      }),
    );

    // Reset form
    this.passwordForm = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    };
  }

  clearError() {
    this.store.dispatch(UserActions.clearError());
  }
}
