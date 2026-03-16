import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserProfile } from '../../../../../domain/entities/user-profile.entity';
import { UserActions } from '../../../../../core/state/user/user.actions';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-edit-profile-picture-modal',
  templateUrl: './edit-profile-picture-modal.component.html',
  styleUrls: ['./edit-profile-picture-modal.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
})
export class EditProfilePictureModalComponent {
  selectedFile: File | null = null;
  previewUrl: string | null = null;
  isLoading = false;

  constructor(
    private dialogRef: MatDialogRef<EditProfilePictureModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { profile: UserProfile },
    private store: Store,
  ) {
    // Set initial preview to current profile picture
    this.previewUrl = data.profile.profilePictureUrl || null;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.selectedFile = input.files[0];
      
      // Create preview
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.previewUrl = e.target.result;
      };
      reader.readAsDataURL(this.selectedFile);
    }
  }

  triggerFileInput(): void {
    const fileInput = document.getElementById('profilePictureInput') as HTMLInputElement;
    if (fileInput) {
      fileInput.click();
    }
  }

  onSave(): void {
    if (this.selectedFile) {
      this.isLoading = true;
      
      // Create FormData for file upload
      const formData = new FormData();
      formData.append('file', this.selectedFile);
      
      // Dispatch action to upload profile picture
      this.store.dispatch(UserActions.uploadProfilePicture({ file: this.selectedFile }));
      
      // Close dialog after a short delay
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
