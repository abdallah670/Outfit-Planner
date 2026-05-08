import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatIconModule,
  ],
  template: `
    <div class="reset-password-container">
      <mat-card class="reset-card">
        <mat-card-header>
          <mat-card-title>Reset Password</mat-card-title>
          <mat-card-subtitle>Enter your new password</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          @if (!token || !email) {
            <div class="error-message">
              <mat-icon color="warn">error</mat-icon>
              <span>Invalid reset link. Please request a new password reset.</span>
            </div>
            <button mat-raised-button color="primary" routerLink="/forgot-password">
              Request New Reset Link
            </button>
          } @else if (isSuccess()) {
            <div class="success-message">
              <mat-icon color="primary">check_circle</mat-icon>
              <h2>Password Reset Successful!</h2>
              <p>Your password has been reset successfully.</p>
              <p>You can now log in with your new password.</p>
              <button mat-raised-button color="primary" routerLink="/login">
                Go to Login
              </button>
            </div>
          } @else {
            <form [formGroup]="resetForm" (ngSubmit)="onSubmit()">
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>New Password</mat-label>
                <input 
                  matInput 
                  [type]="hidePassword() ? 'password' : 'text'" 
                  formControlName="newPassword"
                  placeholder="Enter new password"
                >
                <button 
                  mat-icon-button 
                  matSuffix 
                  type="button"
                  (click)="hidePassword.set(!hidePassword())"
                >
                  <mat-icon>{{ hidePassword() ? 'visibility_off' : 'visibility' }}</mat-icon>
                </button>
                @if (resetForm.get('newPassword')?.hasError('required')) {
                  <mat-error>Password is required</mat-error>
                }
                @if (resetForm.get('newPassword')?.hasError('minlength')) {
                  <mat-error>Password must be at least 6 characters</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Confirm Password</mat-label>
                <input 
                  matInput 
                  [type]="hidePassword() ? 'password' : 'text'" 
                  formControlName="confirmPassword"
                  placeholder="Confirm new password"
                >
                @if (resetForm.get('confirmPassword')?.hasError('required')) {
                  <mat-error>Please confirm your password</mat-error>
                }
                @if (resetForm.hasError('passwordMismatch')) {
                  <mat-error>Passwords do not match</mat-error>
                }
              </mat-form-field>

              @if (errorMessage()) {
                <div class="error-message">
                  <mat-icon color="warn">error</mat-icon>
                  <span>{{ errorMessage() }}</span>
                </div>
              }

              <div class="button-row">
                <button 
                  mat-raised-button 
                  color="primary" 
                  type="submit" 
                  [disabled]="resetForm.invalid || isLoading()"
                  class="full-width"
                >
                  @if (isLoading()) {
                    <mat-spinner diameter="20"></mat-spinner>
                  } @else {
                    Reset Password
                  }
                </button>
              </div>
            </form>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: `
    .reset-password-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      padding: 20px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }
    .reset-card {
      max-width: 400px;
      width: 100%;
      padding: 20px;
    }
    .full-width {
      width: 100%;
      margin-bottom: 15px;
    }
    .button-row {
      margin-top: 20px;
    }
    .success-message {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 15px;
      padding: 30px 0;
      text-align: center;
    }
    .success-message mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
    }
    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #f44336;
      margin: 15px 0;
      font-size: 14px;
    }
    h2 {
      margin: 0;
      color: #333;
    }
    p {
      margin: 0;
      color: #666;
    }
  `
})
export class ResetPasswordComponent {
  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  token: string | null = null;
  email: string | null = null;
  
  isLoading = signal(false);
  isSuccess = signal(false);
  errorMessage = signal('');
  hidePassword = signal(true);

  resetForm = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: this.passwordMatchValidator });

  constructor() {
    this.token = this.route.snapshot.queryParamMap.get('token');
    this.email = this.route.snapshot.queryParamMap.get('email');
  }

  private passwordMatchValidator(form: any) {
    const password = form.get('newPassword')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.resetForm.invalid || !this.token || !this.email) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    const request = {
      email: this.email,
      token: this.token,
      newPassword: this.resetForm.value.newPassword!,
      confirmPassword: this.resetForm.value.confirmPassword!
    };
    
    this.authService.resetPassword(request).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.isSuccess.set(true);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || 'Password reset failed. Please try again.');
      }
    });
  }
}
