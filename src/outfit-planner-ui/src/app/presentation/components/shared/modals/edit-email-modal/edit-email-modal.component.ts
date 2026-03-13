import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { UserActions } from '../../../../../core/state/user/user.actions';

@Component({
  selector: 'app-edit-email-modal',
  templateUrl: './edit-email-modal.component.html',
  styleUrls: ['./edit-email-modal.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatSnackBarModule,
  ],
})
export class EditEmailModalComponent {
  emailForm: FormGroup;
  currentEmail: string;
  isLoading = false;
  hideCurrentPassword = true;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditEmailModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { email: string },
    private store: Store,
    private snackBar: MatSnackBar,
  ) {
    this.currentEmail = data.email;

    this.emailForm = this.fb.group(
      {
        currentPassword: ['', Validators.required],
        newEmail: ['', [Validators.required, Validators.email]],
        confirmEmail: ['', [Validators.required, Validators.email]],
      },
      { validators: this.emailMatchValidator },
    );
  }

  emailMatchValidator(form: FormGroup) {
    const newEmail = form.get('newEmail')?.value;
    const confirmEmail = form.get('confirmEmail')?.value;

    if (newEmail && confirmEmail && newEmail !== confirmEmail) {
      return { emailMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.emailForm.valid) {
      this.isLoading = true;

      const newEmail = this.emailForm.get('newEmail')?.value;
      const currentPassword = this.emailForm.get('currentPassword')?.value;

      // Dispatch the update email action
      this.store.dispatch(
        UserActions.updateEmail({
          request: {
            newEmail: newEmail,
            confirmEmail: newEmail,
            currentPassword: currentPassword,
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
