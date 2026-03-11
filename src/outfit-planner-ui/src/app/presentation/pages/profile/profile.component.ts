import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatConfirmDialogComponent } from '../../components/shared/mat-confirm-dialog/mat-confirm-dialog.component';
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
  selectUserError,
  selectStyleProfile,
  selectUserPreferences,
} from '../../../core/state/user/user.selectors';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatIconModule,
    MatSnackBarModule,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly authService = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);

  profile$!: Observable<UserProfile | null>;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;
  styleProfile$: Observable<UserStyleProfile | undefined>;
  preferences$: Observable<UserPreferences | undefined>;

  // Enums for templates
  stylePreferences = Object.values(StylePreference);
  privacyLevels = Object.values(PrivacyLevel);

  constructor() {
    this.profile$ = this.store.select(selectUserProfile);
    this.loading$ = this.store.select(selectUserLoading);
    this.error$ = this.store.select(selectUserError);
    this.styleProfile$ = this.store.select(selectStyleProfile);
    this.preferences$ = this.store.select(selectUserPreferences);
  }

  ngOnInit() {
    this.store.dispatch(UserActions.loadProfile());
  }

  getMemberSince(createdAt: string | undefined): string {
    if (!createdAt) return 'Unknown';
    const date = new Date(createdAt);
    return date.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  }

  getAverageWears(profile: UserProfile): number {
    if (!profile.wardrobeItemCount || !profile.totalWears) return 0;
    return Math.round(profile.totalWears / profile.wardrobeItemCount);
  }

  getWardrobeValue(profile: UserProfile): string {
    // Mock calculation - in production this would come from backend
    const estimatedValue = (profile.wardrobeItemCount || 0) * 50;
    return (estimatedValue / 1000).toFixed(1);
  }

  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    img.src = 'assets/default-avatar.png';
  }

  // Modal handlers - These would open dialog components
  // For now, we'll show a snackbar indicating the feature
  openEditProfileModal(): void {
    this.snackBar.open('Profile editing will be available in a modal', 'Close', {
      duration: 3000,
    });
  }

  openEditStyleModal(): void {
    this.snackBar.open('Style profile editing will be available in a modal', 'Close', {
      duration: 3000,
    });
  }

  openEditEmailModal(): void {
    this.snackBar.open('Email editing will be available in a modal', 'Close', {
      duration: 3000,
    });
  }

  openChangePasswordModal(): void {
    this.snackBar.open('Password change will be available in a modal', 'Close', {
      duration: 3000,
    });
  }

  openEditPreferencesModal(): void {
    this.snackBar.open('Preferences editing will be available in a modal', 'Close', {
      duration: 3000,
    });
  }

  exportData(): void {
    this.snackBar.open('Data export feature coming soon', 'Close', {
      duration: 3000,
    });
  }

  onLogout(): void {
    const dialogRef = this.dialog.open(MatConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Log Out',
        message: 'Are you sure you want to log out?',
        confirmText: 'Log Out',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this.authService.logout();
        // Navigate to login page
        window.location.href = '/login';
      }
    });
  }

  confirmDeleteAccount(): void {
    const dialogRef = this.dialog.open(MatConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Account',
        message:
          'Are you sure you want to delete your account? This action cannot be undone and all your data will be permanently removed.',
        confirmText: 'Delete Account',
        cancelText: 'Cancel',
        isDanger: true,
      },
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        // TODO: Add delete account API call and action when backend supports it
        // For now, show a message
        this.snackBar.open('Account deletion is not yet implemented', 'Close', {
          duration: 5000,
        });
      }
    });
  }

  clearError() {
    this.store.dispatch(UserActions.clearError());
  }
}
