import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
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
    <div class="forgot-password-container">
      <mat-card class="forgot-card">
        <mat-card-header>
          <mat-card-title>Forgot Password</mat-card-title>
          <mat-card-subtitle>Enter your email to receive a password reset link</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          @if (isSuccess()) {
            <div class="success-message">
              <mat-icon color="primary">check_circle</mat-icon>
              <p>If an account exists with this email, a password reset link has been sent.</p>
              <p>Please check your inbox and follow the instructions.</p>
              <button mat-raised-button color="primary" routerLink="/login">
                Back to Login
              </button>
            </div>
          } @else {
            <form [formGroup]="forgotForm" (ngSubmit)="onSubmit()">
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Email</mat-label>
                <input matInput formControlName="email" type="email" placeholder="your@email.com">
                @if (forgotForm.get('email')?.hasError('required')) {
                  <mat-error>Email is required</mat-error>
                }
                @if (forgotForm.get('email')?.hasError('email')) {
                  <mat-error>Please enter a valid email</mat-error>
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
                  [disabled]="forgotForm.invalid || isLoading()"
                  class="full-width"
                >
                  @if (isLoading()) {
                    <mat-spinner diameter="20"></mat-spinner>
                  } @else {
                    Send Reset Link
                  }
                </button>
              </div>

              <div class="links">
                <a routerLink="/login">Back to Login</a>
              </div>
            </form>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: `
    .forgot-password-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      padding: 20px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }
    .forgot-card {
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
      margin: 10px 0;
      font-size: 14px;
    }
    .links {
      text-align: center;
      margin-top: 20px;
    }
    .links a {
      color: #667eea;
      text-decoration: none;
    }
    .links a:hover {
      text-decoration: underline;
    }
  `
})
export class ForgotPasswordComponent {
  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  isLoading = signal(false);
  isSuccess = signal(false);
  errorMessage = signal('');

  forgotForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  onSubmit(): void {
    if (this.forgotForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    const email = this.forgotForm.value.email!;
    
    this.authService.forgotPassword({ email }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.isSuccess.set(true);
      },
      error: (error) => {
        this.isLoading.set(false);
        // Still show success to prevent email enumeration
        this.isSuccess.set(true);
      }
    });
  }
}
