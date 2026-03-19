import { Component, Inject, OnDestroy, ChangeDetectorRef } from '@angular/core';
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
import { Subscription } from 'rxjs';
import { filter, take } from 'rxjs/operators';
import { UserPreferences, PrivacyLevel } from '../../../../../domain/entities/user-profile.entity';
import { UserActions } from '../../../../../core/state/user/user.actions';
import { selectUserError, selectUserUpdating } from '../../../../../core/state/user/user.selectors';

@Component({
  selector: 'app-edit-preferences-modal',
  templateUrl: './edit-preferences-modal.component.html',
  styleUrls: ['./edit-preferences-modal.component.scss'],
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
export class EditPreferencesModalComponent implements OnDestroy {
  preferencesForm: FormGroup;
  privacyLevels = Object.values(PrivacyLevel);
  isLoading = false;
  private updateSubscription?: Subscription;
  private errorSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditPreferencesModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { preferences: UserPreferences | undefined },
    private store: Store,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef,
  ) {
    const existingPrefs = data.preferences;

    this.preferencesForm = this.fb.group({
      includeInTrendAnalysis: [existingPrefs?.includeInTrendAnalysis ?? true],
      allowFriendRequests: [existingPrefs?.allowFriendRequests ?? true],
      shareOutfitsAnonymously: [existingPrefs?.shareOutfitsAnonymously ?? false],
      defaultOutfitPrivacy: [existingPrefs?.defaultOutfitPrivacy || PrivacyLevel.Private],
      showBodyMetrics: [existingPrefs?.showBodyMetrics ?? false],
      allowLocationTracking: [existingPrefs?.allowLocationTracking ?? false],
    });
  }

  onSubmit(): void {
    if (this.preferencesForm.valid) {
      this.isLoading = true;
      this.cdr.detectChanges(); // Trigger change detection

      const preferences: UserPreferences = {
        includeInTrendAnalysis: this.preferencesForm.get('includeInTrendAnalysis')?.value,
        allowFriendRequests: this.preferencesForm.get('allowFriendRequests')?.value,
        shareOutfitsAnonymously: this.preferencesForm.get('shareOutfitsAnonymously')?.value,
        defaultOutfitPrivacy: this.preferencesForm.get('defaultOutfitPrivacy')?.value,
        showBodyMetrics: this.preferencesForm.get('showBodyMetrics')?.value,
        allowLocationTracking: this.preferencesForm.get('allowLocationTracking')?.value,
      };

      // Dispatch the update action - don't include name when only updating preferences
      this.store.dispatch(
        UserActions.updateProfile({
          request: {
            preferences,
          },
        }),
      );

      // Subscribe to error state - show error if update fails
      this.errorSubscription = this.store.select(selectUserError).pipe(
        filter((error): error is string => error !== null && error !== undefined),
        take(1)
      ).subscribe((error: string) => {
        this.isLoading = false;
        this.cdr.detectChanges(); // Trigger change detection
        this.snackBar.open(`Update failed: ${error}`, 'Close', { duration: 5000 });
      });

      // Subscribe to updating state - when it goes from true to false, check result
      this.updateSubscription = this.store.select(selectUserUpdating).pipe(
        filter(updating => updating === false),
        take(1)
      ).subscribe(() => {
        this.store.select(selectUserError).pipe(take(1)).subscribe((error: string | null) => {
          this.isLoading = false;
          this.cdr.detectChanges(); // Trigger change detection
          if (error) {
            this.snackBar.open(`Update failed: ${error}`, 'Close', { duration: 5000 });
          } else {
            this.snackBar.open('Preferences updated successfully!', 'Close', { duration: 3000 });
            this.dialogRef.close(true);
          }
        });
      });

      // Fallback: close after 10 seconds if no response
      setTimeout(() => {
        if (this.isLoading) {
          this.isLoading = false;
          this.cdr.detectChanges(); // Trigger change detection
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
