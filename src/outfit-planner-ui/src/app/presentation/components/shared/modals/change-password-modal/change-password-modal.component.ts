import { Component, Inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Subscription } from 'rxjs';
import { filter, take } from 'rxjs/operators';
import { UserActions } from '../../../../../core/state/user/user.actions';
import { selectUserError, selectChangingPassword } from '../../../../../core/state/user/user.selectors';
import { AuthService } from '../../../../../core/services/auth.service';

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
export class ChangePasswordModalComponent implements OnDestroy {
  passwordForm: FormGroup;
  isLoading = false;
  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmPassword = true;
  private updateSubscription?: Subscription;
  private errorSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ChangePasswordModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: {},
    private store: Store,
    private snackBar: MatSnackBar,
    private authService: AuthService,
    private router: Router,
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

      // Subscribe to error state - show error if update fails
      this.errorSubscription = this.store.select(selectUserError).pipe(
        filter((error): error is string => error !== null && error !== undefined),
        take(1)
      ).subscribe((error: string) => {
        this.isLoading = false;
        this.snackBar.open(`Password change failed: ${error}`, 'Close', { duration: 5000 });
      });

      // Subscribe to changingPassword state - when it goes from true to false, check result
      this.updateSubscription = this.store.select(selectChangingPassword).pipe(
        filter(changing => changing === false), // Wait for change to complete
        take(1)
      ).subscribe(() => {
        // Check if there was an error
        this.store.select(selectUserError).pipe(take(1)).subscribe((error: string | null) => {
          this.isLoading = false;
          if (error) {
            this.snackBar.open(`Password change failed: ${error}`, 'Close', { duration: 5000 });
          } else {
            this.snackBar.open('Password changed successfully! You will be logged out for security.', 'Close', { duration: 3000 });
            this.dialogRef.close(true);
            // Log out the user for security after password change
            setTimeout(() => {
              this.authService.logout();
              this.router.navigate(['/login']);
            }, 1000);
          }
        });
      });

      // Fallback: close after 10 seconds if no response
      setTimeout(() => {
        if (this.isLoading) {
          this.isLoading = false;
          this.snackBar.open('Password change timed out. Please try again.', 'Close', { duration: 5000 });
        }
      }, 10000);
    }
  }

  goToForgotPassword(): void {
    this.dialogRef.close(false);
    this.router.navigate(['/forgot-password']);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  ngOnDestroy(): void {
    this.updateSubscription?.unsubscribe();
    this.errorSubscription?.unsubscribe();
  }
}