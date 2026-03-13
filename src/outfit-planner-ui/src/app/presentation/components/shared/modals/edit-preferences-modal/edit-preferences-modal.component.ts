import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';
import { UserPreferences, PrivacyLevel } from '../../../../../domain/entities/user-profile.entity';
import { UserActions } from '../../../../../core/state/user/user.actions';

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
export class EditPreferencesModalComponent {
  preferencesForm: FormGroup;
  privacyLevels = Object.values(PrivacyLevel);
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditPreferencesModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { preferences: UserPreferences | undefined },
    private store: Store,
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

      const preferences: UserPreferences = {
        includeInTrendAnalysis: this.preferencesForm.get('includeInTrendAnalysis')?.value,
        allowFriendRequests: this.preferencesForm.get('allowFriendRequests')?.value,
        shareOutfitsAnonymously: this.preferencesForm.get('shareOutfitsAnonymously')?.value,
        defaultOutfitPrivacy: this.preferencesForm.get('defaultOutfitPrivacy')?.value,
        showBodyMetrics: this.preferencesForm.get('showBodyMetrics')?.value,
        allowLocationTracking: this.preferencesForm.get('allowLocationTracking')?.value,
      };

      // Dispatch the update action
      this.store.dispatch(
        UserActions.updateProfile({
          request: {
            name: '', // Keep existing name
            preferences,
          },
        }),
      );

      // Simulate API call completion
      setTimeout(() => {
        this.isLoading = false;
        this.dialogRef.close(true);
      }, 1000);
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
