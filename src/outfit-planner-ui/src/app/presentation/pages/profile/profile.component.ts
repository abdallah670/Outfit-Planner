import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Observable, take } from 'rxjs';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { UserDataSource } from '../../../data/datasources/user.datasource';
import { MatConfirmDialogComponent } from '../../components/shared/mat-confirm-dialog/mat-confirm-dialog.component';
import { EditProfileModalComponent } from '../../components/shared/modals/edit-profile-modal/edit-profile-modal.component';
import { EditProfilePictureModalComponent } from '../../components/shared/modals/edit-profile-picture-modal/edit-profile-picture-modal.component';
import { EditStyleProfileModalComponent } from '../../components/shared/modals/edit-style-profile-modal/edit-style-profile-modal.component';
import { EditPreferencesModalComponent } from '../../components/shared/modals/edit-preferences-modal/edit-preferences-modal.component';
import { ChangePasswordModalComponent } from '../../components/shared/modals/change-password-modal/change-password-modal.component';
import { EditEmailModalComponent } from '../../components/shared/modals/edit-email-modal/edit-email-modal.component';
import { EditStyleRulesModalComponent } from '../../components/shared/modals/edit-style-rules-modal/edit-style-rules-modal.component';
import {
  UserProfile,
  StylePreference,
  PrivacyLevel,
  UserStyleProfile,
  UserPreferences,
  StyleRule,
} from '../../../domain/entities/user-profile.entity';
import { UserActions } from '../../../core/state/user/user.actions';
import {
  selectUserProfile,
  selectUserLoading,
  selectUserError,
  selectStyleProfile,
  selectUserPreferences,
  selectStyleRules,
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
  private readonly userDataSource = inject(UserDataSource);

  profile$!: Observable<UserProfile | null>;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;
  styleProfile$: Observable<UserStyleProfile | undefined>;
  preferences$: Observable<UserPreferences | undefined>;
  styleRules$!: Observable<StyleRule[]>;

  // Enums for templates
  stylePreferences = Object.values(StylePreference);
  privacyLevels = Object.values(PrivacyLevel);

  constructor() {
    this.profile$ = this.store.select(selectUserProfile);
    this.loading$ = this.store.select(selectUserLoading);
    this.error$ = this.store.select(selectUserError);
    this.styleProfile$ = this.store.select(selectStyleProfile);
    this.preferences$ = this.store.select(selectUserPreferences);
    this.styleRules$ = this.store.select(selectStyleRules);
  }

  ngOnInit() {
    this.store.dispatch(UserActions.loadProfile());
    this.store.dispatch(UserActions.loadStyleRules());
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
    // Prevent infinite loop if default avatar also fails
    if (!img.src.includes('default-avatar.png')) {
      img.src = 'assets/default-avatar.png';
    }
  }

  openEditProfileModal(): void {
    this.profile$.pipe(take(1)).subscribe((profile: UserProfile | null) => {
      const dialogRef = this.dialog.open(EditProfileModalComponent, {
        width: '400px',
        data: {
          profile: profile,
        },
      });

      dialogRef.afterClosed().subscribe((result: boolean) => {
        if (result) {
          this.store.dispatch(UserActions.loadProfile());
          this.snackBar.open('Profile updated successfully', 'Close', {
            duration: 3000,
          });
        }
      });
    });
  }

  openEditProfilePictureModal(): void {
    this.profile$.pipe(take(1)).subscribe((profile: UserProfile | null) => {
      const dialogRef = this.dialog.open(EditProfilePictureModalComponent, {
        width: '400px',
        data: {
          profile: profile,
        },
      });

      dialogRef.afterClosed().subscribe((result: boolean) => {
        if (result) {
          this.store.dispatch(UserActions.loadProfile());
          this.snackBar.open('Profile picture updated successfully', 'Close', {
            duration: 3000,
          });
        }
      });
    });
  }

  openEditStyleModal(): void {
    this.profile$.pipe(take(1)).subscribe((profile: UserProfile | null) => {
      const dialogRef = this.dialog.open(EditStyleProfileModalComponent, {
        width: '400px',
        data: {
          styleProfile: profile?.styleProfile,
        },
      });

      dialogRef.afterClosed().subscribe((result: boolean) => {
        if (result) {
          this.store.dispatch(UserActions.loadProfile());
          this.snackBar.open('Style profile updated successfully', 'Close', {
            duration: 3000,
          });
        }
      });
    });
  }

  openStyleRulesModal(): void {
    // Load style rules before opening modal
    this.store.dispatch(UserActions.loadStyleRules());
    
    this.styleRules$.pipe(take(1)).subscribe((rules: StyleRule[]) => {
      const dialogRef = this.dialog.open(EditStyleRulesModalComponent, {
        width: '500px',
        data: {
          rules: rules,
        },
      });

      dialogRef.afterClosed().subscribe((result: boolean) => {
        if (result) {
          this.snackBar.open('Style rules updated successfully', 'Close', {
            duration: 3000,
          });
        }
      });
    });
  }

  // Inline Style Rules CRUD
  addNewRule(): void {
    // Open modal to create a new style rule
    const dialogRef = this.dialog.open(EditStyleRulesModalComponent, {
      width: '500px',
      data: {
        rule: null, // null means create mode
        isNew: true,
      },
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this.snackBar.open('New rule added successfully', 'Close', { duration: 3000 });
      }
    });
  }

  editRule(rule: StyleRule): void {
    // Open modal to edit existing style rule
    const dialogRef = this.dialog.open(EditStyleRulesModalComponent, {
      width: '500px',
      data: {
        rule: rule,
        isNew: false,
      },
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this.snackBar.open('Rule updated successfully', 'Close', { duration: 3000 });
      }
    });
  }

  deleteRule(ruleId: string): void {
    // Show confirmation dialog using MatDialog
    const dialogRef = this.dialog.open(MatConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Rule',
        message: 'Are you sure you want to delete this style rule?',
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.store.dispatch(UserActions.deleteStyleRule({ id: ruleId }));
        this.snackBar.open('Rule deleted successfully', 'Close', { duration: 3000 });
      }
    });
  }

  // Helper to convert style enum to display string
  getStyleDisplayName(styleValue: number | string | undefined): string {
    if (styleValue === undefined || styleValue === null) {
      return 'Not Set';
    }
    
    // Handle both number and string enum values
    const styleNames: { [key: number]: string } = {
      0: 'Minimalist',
      1: 'Classic',
      2: 'Bohemian',
      3: 'Streetwear',
      4: 'Professional',
      5: 'Athleisure',
      6: 'Eclectic',
      7: 'Vintage',
    };
    
    // If it's already a string, return it directly
    if (typeof styleValue === 'string') {
      return styleValue;
    }
    
    return styleNames[styleValue] || 'Not Set';
  }

  toggleRuleActive(rule: StyleRule): void {
    this.store.dispatch(
      UserActions.updateStyleRule({
        id: rule.id,
        rule: {
          name: rule.name,
          description: rule.description,
          isActive: !rule.isActive,
          criteriaJson: rule.criteriaJson,
        },
      })
    );
  }

  openEditEmailModal(): void {
    this.profile$.pipe(take(1)).subscribe((profile: UserProfile | null) => {
      const dialogRef = this.dialog.open(EditEmailModalComponent, {
        width: '400px',
        data: {
          email: profile?.email,
        },
      });

      dialogRef.afterClosed().subscribe((result: boolean) => {
        if (result) {
          this.store.dispatch(UserActions.loadProfile());
          this.snackBar.open('Email updated successfully', 'Close', {
            duration: 3000,
          });
        }
      });
    });
  }

  openChangePasswordModal(): void {
    const dialogRef = this.dialog.open(ChangePasswordModalComponent, {
      width: '400px',
      data: {},
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this.snackBar.open('Password changed successfully', 'Close', {
          duration: 3000,
        });
      }
    });
  }

  openEditPreferencesModal(): void {
    this.profile$.pipe(take(1)).subscribe((profile: UserProfile | null) => {
      const dialogRef = this.dialog.open(EditPreferencesModalComponent, {
        width: '400px',
        data: {
          preferences: profile?.preferences,
        },
      });

      dialogRef.afterClosed().subscribe((result: boolean) => {
        if (result) {
          this.store.dispatch(UserActions.loadProfile());
          this.snackBar.open('Preferences updated successfully', 'Close', {
            duration: 3000,
          });
        }
      });
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

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.snackBar.open('Please select an image file', 'Close', {
          duration: 3000,
        });
        return;
      }

      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        this.snackBar.open('Image size must be less than 5MB', 'Close', {
          duration: 3000,
        });
        return;
      }

      // Upload the file
      this.userDataSource.uploadProfilePicture(file).subscribe({
        next: (imageUrl: string) => {
          this.snackBar.open('Profile picture updated successfully', 'Close', {
            duration: 3000,
          });
          // Reload profile to get updated image URL
          this.store.dispatch(UserActions.loadProfile());
        },
        error: (error: Error) => {
          console.error('Upload error:', error);
          this.snackBar.open('Failed to upload profile picture', 'Close', {
            duration: 3000,
          });
        },
      });
    }
  }
}
