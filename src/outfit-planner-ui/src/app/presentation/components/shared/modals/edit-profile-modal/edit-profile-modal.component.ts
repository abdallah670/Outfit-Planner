import { Component, Inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { UserProfile } from '../../../../../domain/entities/user-profile.entity';
import { UserActions } from '../../../../../core/state/user/user.actions';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-edit-profile-modal',
  templateUrl: './edit-profile-modal.component.html',
  styleUrls: ['./edit-profile-modal.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
  ],
})
export class EditProfileModalComponent {
  profileForm: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditProfileModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { profile: UserProfile },
    private store: Store,
    private cdRef: ChangeDetectorRef,
  ) {
    this.profileForm = this.fb.group({
      name: [data.profile.name, [Validators.required, Validators.maxLength(100)]],
    });
  }

  onSubmit(): void {
    if (this.profileForm.valid) {
      this.isLoading = true;
      this.cdRef.detectChanges();
      
      const request = {
        name: this.profileForm.get('name')?.value,
      };

      // Dispatch the update profile action
      this.store.dispatch(UserActions.updateProfile({ request }));

      // Close dialog after a short delay to allow action to be dispatched
      setTimeout(() => {
        this.isLoading = false;
        this.dialogRef.close(true);
      }, 500);
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
