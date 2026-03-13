import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';
import { UserActions } from '../../../../../core/state/user/user.actions';

@Component({
  selector: 'app-change-password-modal',
  templateUrl: './change-password-modal.component.html',
  styleUrls: ['./change-password-modal.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule
  ]
})
export class ChangePasswordModalComponent {
  passwordForm: FormGroup;
  isLoading = false;
  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmPassword = true;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ChangePasswordModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: {},
    private store: Store
  ) {
    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    
    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      return { passwordMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.passwordForm.valid) {
      this.isLoading = true;
      
      const request = {
        currentPassword: this.passwordForm.get('currentPassword')?.value,
        newPassword: this.passwordForm.get('newPassword')?.value,
        confirmPassword: this.passwordForm.get('confirmPassword')?.value
      };

      // Dispatch the change password action
      this.store.dispatch(UserActions.changePassword({ request }));

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