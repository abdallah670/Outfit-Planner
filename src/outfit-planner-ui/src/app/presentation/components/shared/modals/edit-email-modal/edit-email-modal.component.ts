import { Component, Inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Subscription } from 'rxjs';
import { filter, take } from 'rxjs/operators';
import { UserActions } from '../../../../../core/state/user/user.actions';
import { selectUserError, selectUpdatingEmail } from '../../../../../core/state/user/user.selectors';

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
export class EditEmailModalComponent implements OnDestroy {
  emailForm: FormGroup;
  currentEmail: string;
  isLoading = false;
  hideCurrentPassword = true;
  private updateSubscription?: Subscription;
  private errorSubscription?: Subscription;

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

      // Subscribe to error state - show error if update fails
      this.errorSubscription = this.store.select(selectUserError).pipe(
        filter(error => error !== null && error !== undefined),
        take(1)
      ).subscribe((error: string | null) => {
        this.isLoading = false;
        this.snackBar.open(`Update failed: ${error}`, 'Close', { duration: 5000 });
      });

      // Subscribe to updatingEmail state - when it goes from true to false, check result
      this.updateSubscription = this.store.select(selectUpdatingEmail).pipe(
        filter(updating => updating === false), // Wait for update to complete
        take(1)
      ).subscribe(() => {
        // Check if there was an error
        this.store.select(selectUserError).pipe(take(1)).subscribe((error: string | null) => {
          this.isLoading = false;
          if (error) {
            this.snackBar.open(`Update failed: ${error}`, 'Close', { duration: 5000 });
          } else {
            this.snackBar.open('Email updated successfully!', 'Close', { duration: 3000 });
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
