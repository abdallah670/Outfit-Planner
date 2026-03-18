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
import {
  UserStyleProfile,
  StylePreference,
} from '../../../../../domain/entities/user-profile.entity';
import { UserActions } from '../../../../../core/state/user/user.actions';

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
export class EditStyleProfileModalComponent {
  styleForm: FormGroup;
  stylePreferences = Object.values(StylePreference);
  selectedColors: string[] = [];
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditStyleProfileModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { styleProfile: UserStyleProfile | undefined },
    private store: Store,
  ) {
    const existingStyle = data.styleProfile;
    this.selectedColors = existingStyle?.preferredColors || [];

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

  onSubmit(): void {
    if (this.styleForm.valid) {
      this.isLoading = true;

      const existingStyle = this.data.styleProfile;
      const styleProfile = {
        style: this.styleForm.get('style')?.value,
        fitPreferences: this.styleForm.get('fitPreferences')?.value,
        comfortPriority: this.styleForm.get('comfortPriority')?.value,
        acceptsTrends: this.styleForm.get('acceptsTrends')?.value,
        preferredColors: this.selectedColors,
        customRules: existingStyle?.customRules || [],
      };

      // Dispatch the update action
      this.store.dispatch(
        UserActions.updateProfile({
          request: {
            name: '', // Keep existing name
            styleProfile,
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
