import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
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
    MatIconModule,
  ],
})
export class EditProfileModalComponent {
  profileForm: FormGroup;
  selectedFilePreview: string | null = null;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditProfileModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { profile: UserProfile },
    private store: Store,
  ) {
    this.profileForm = this.fb.group({
      name: [data.profile.name, [Validators.required, Validators.maxLength(100)]],
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      const reader = new FileReader();

      reader.onload = (e: any) => {
        this.selectedFilePreview = e.target.result;
      };

      reader.readAsDataURL(file);

      // TODO: Handle file upload to backend
      // For now, we'll just show preview
      // In a real implementation, you would upload the file and update the profile
    }
  }

  onSubmit(): void {
    if (this.profileForm.valid) {
      this.isLoading = true;
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
