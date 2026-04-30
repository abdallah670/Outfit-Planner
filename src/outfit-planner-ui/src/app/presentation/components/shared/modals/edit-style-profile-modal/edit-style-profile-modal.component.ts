import { Component, Inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { take, filter } from 'rxjs/operators';
import { Subscription } from 'rxjs';
import {
  UserStyleProfile,
  StylePreference,
  StyleRule,
} from '../../../../../domain/entities/user-profile.entity';
import { UserActions } from '../../../../../core/state/user/user.actions';
import { selectUserProfile, selectUserError, selectUserUpdating } from '../../../../../core/state/user/user.selectors';

@Component({
  selector: 'app-edit-style-profile-modal',
  templateUrl: './edit-style-profile-modal.component.html',
  styleUrls: ['./edit-style-profile-modal.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatIconModule,
  ],
})
export class EditStyleProfileModalComponent implements OnDestroy {
  styleForm: FormGroup;
  stylePreferences = Object.values(StylePreference);
  selectedColors: string[] = [];
  customRules: StyleRule[] = [];
  isLoading = false;
  private updateSubscription?: Subscription;
  private errorSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditStyleProfileModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { styleProfile: UserStyleProfile | undefined },
    private store: Store,
    private snackBar: MatSnackBar,
  ) {
    const existingStyle = data.styleProfile;
    this.selectedColors = existingStyle?.preferredColors || [];
    this.customRules = existingStyle?.customRules ? Array.from(existingStyle.customRules) : [];

    this.styleForm = this.fb.group({
      style: [existingStyle?.style || ''],
      fitPreferences: [existingStyle?.fitPreferences || ''],
      comfortPriority: [existingStyle?.comfortPriority || 5],
      acceptsTrends: [existingStyle?.acceptsTrends || false],
    });
  }

  addColor(color: string): void {
    if (color && !this.selectedColors.includes(color)) {
      this.selectedColors = [...this.selectedColors, color];
    }
  }

  removeColor(index: number): void {
    this.selectedColors = this.selectedColors.filter((_, i) => i !== index);
  }

  addCustomRule(): void {
    // Add a new empty rule
    const newRule: StyleRule = {
      id: '', // Empty ID means new rule
      name: '',
      description: '',
      isActive: true,
      criteriaJson: '{}',
    };
    this.customRules = [...this.customRules, newRule];
  }

  removeCustomRule(index: number): void {
    this.customRules = this.customRules.filter((_, i) => i !== index);
  }

  updateCustomRule(index: number, field: keyof StyleRule, value: any): void {
    const rule = { ...this.customRules[index], [field]: value };
    this.customRules = [
      ...this.customRules.slice(0, index),
      rule,
      ...this.customRules.slice(index + 1)
    ];
  }

  onSubmit(): void {
    if (this.styleForm.valid) {
      this.isLoading = true;

      // Create style profile update WITHOUT customRules (they are managed via separate endpoints)
      const styleProfile = {
        style: this.styleForm.get('style')?.value || 'Casual',
        fitPreferences: this.styleForm.get('fitPreferences')?.value || 'Regular',
        comfortPriority: this.styleForm.get('comfortPriority')?.value || 5,
        acceptsTrends: this.styleForm.get('acceptsTrends')?.value || false,
        preferredColors: this.selectedColors,
        customRules: [], // Not sent via profile update - managed via separate endpoints
      };

      // Dispatch the update action - don't include name when only updating style profile
      this.store.dispatch(
        UserActions.updateProfile({
          request: {
            styleProfile,
          },
        }),
      );

      // Subscribe to error state - show error if update fails
      this.errorSubscription = this.store.select(selectUserError).pipe(
        filter((error): error is string => error !== null && error !== undefined),
        take(1)
      ).subscribe((error: string) => {
        this.isLoading = false;
        this.snackBar.open(`Update failed: ${error}`, 'Close', { duration: 5000 });
      });

      // Subscribe to updating state - when it goes from true to false, check if success or failure
      this.updateSubscription = this.store.select(selectUserUpdating).pipe(
        filter(updating => updating === false), // Wait for update to complete
        take(1)
      ).subscribe(() => {
        // Check if there was an error
        this.store.select(selectUserError).pipe(take(1)).subscribe((error: string | null) => {
          this.isLoading = false;
          if (error) {
            this.snackBar.open(`Update failed: ${error}`, 'Close', { duration: 5000 });
          } else {
            this.snackBar.open('Profile updated successfully!', 'Close', { duration: 3000 });
            this.dialogRef.close(true);
          }
        });
      });

      // Fallback: close after 10 seconds if no response
      setTimeout(() => {
        if (this.isLoading) {
          this.isLoading = false;
          this.snackBar.open('Update timed out. Please try again.', 'Close', { duration: 5000 });
        }
      }, 10000);
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  ngOnDestroy(): void {
    this.updateSubscription?.unsubscribe();
    this.errorSubscription?.unsubscribe();
  }
}
