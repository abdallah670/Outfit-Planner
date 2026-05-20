import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  template: `
    <div class="auth-container">
      <!-- Left Side - Hero Image Section -->
      <div class="hero-section">
        <div class="hero-overlay"></div>
        <div class="hero-content">
          <h1 class="hero-title">Outfit Planner</h1>
          <p class="hero-subtitle">Organize your wardrobe, plan your style, and look your best every day.</p>
        </div>
      </div>

      <!-- Right Side - Form Section -->
      <div class="form-section">
        <div class="form-container">
          <!-- Logo -->
          <div class="logo">
            <div class="logo-icon">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M20.38 3.46L16 2a4 4 0 01-8 0L3.62 3.46a2 2 0 00-1.34 2.23l.58 3.47a1 1 0 00.99.84H6v10c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V10h2.15a1 1 0 00.99-.84l.58-3.47a2 2 0 00-1.34-2.23z"/>
              </svg>
            </div>
            <span class="logo-text">Outfit Planner</span>
          </div>

          @if (!token || !email) {
            <!-- Invalid Link State -->
            <div class="state-container error-state">
              <div class="icon-container error">
                <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
                </svg>
              </div>
              <h2 class="welcome-title">Invalid Reset Link</h2>
              <p class="welcome-subtitle">This password reset link is invalid or has expired. Please request a new one.</p>
              <button class="submit-btn" routerLink="/forgot-password">Request New Reset Link</button>
            </div>
          } @else if (isSuccess()) {
            <!-- Success State -->
            <div class="state-container success-state">
              <div class="icon-container success">
                <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                  <polyline points="22 4 12 14.01 9 11.01"/>
                </svg>
              </div>
              <h2 class="welcome-title">Password Reset!</h2>
              <p class="welcome-subtitle">Your password has been reset successfully. You can now log in with your new password.</p>
              <button class="submit-btn" routerLink="/login">Go to Login</button>
            </div>
          } @else {
            <!-- Reset Password Form -->
            <h2 class="welcome-title">Reset Password</h2>
            <p class="welcome-subtitle">Enter your new password below.</p>

            <form [formGroup]="resetForm" (ngSubmit)="onSubmit()" class="signup-form">
              <!-- New Password -->
              <div class="form-group">
                <label for="newPassword">New Password</label>
                <div class="input-wrapper">
                  <svg class="input-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0110 0v4"/>
                  </svg>
                  <input [type]="hideNewPassword() ? 'password' : 'text'" id="newPassword" formControlName="newPassword" placeholder="Enter new password">
                  <button type="button" class="toggle-password" (click)="hideNewPassword.set(!hideNewPassword())">
                    @if (hideNewPassword()) {
                      <svg class="eye-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/>
                      </svg>
                    } @else {
                      <svg class="eye-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
                        <line x1="1" y1="1" x2="23" y2="23"/>
                      </svg>
                    }
                  </button>
                </div>
                @if (resetForm.get('newPassword')?.invalid && resetForm.get('newPassword')?.touched) {
                  <span class="field-error">
                    @if (resetForm.get('newPassword')?.hasError('required')) {
                      Password is required
                    } @else if (resetForm.get('newPassword')?.hasError('minlength')) {
                      Password must be at least 6 characters
                    }
                  </span>
                }
              </div>

              <!-- Confirm Password -->
              <div class="form-group">
                <label for="confirmPassword">Confirm Password</label>
                <div class="input-wrapper">
                  <svg class="input-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0110 0v4"/>
                  </svg>
                  <input [type]="hideConfirmPassword() ? 'password' : 'text'" id="confirmPassword" formControlName="confirmPassword" placeholder="Confirm new password">
                  <button type="button" class="toggle-password" (click)="hideConfirmPassword.set(!hideConfirmPassword())">
                    @if (hideConfirmPassword()) {
                      <svg class="eye-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/>
                      </svg>
                    } @else {
                      <svg class="eye-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
                        <line x1="1" y1="1" x2="23" y2="23"/>
                      </svg>
                    }
                  </button>
                </div>
                @if (resetForm.get('confirmPassword')?.invalid && resetForm.get('confirmPassword')?.touched) {
                  <span class="field-error">Please confirm your password</span>
                }
                @if (resetForm.hasError('passwordMismatch') && resetForm.get('confirmPassword')?.touched) {
                  <span class="field-error">Passwords do not match</span>
                }
              </div>

              @if (errorMessage()) {
                <p class="error-text">{{ errorMessage() }}</p>
              }

              <button type="submit" class="submit-btn" [disabled]="resetForm.invalid || isLoading()">
                @if (isLoading()) {
                  <span class="loader"></span>
                } @else {
                  Reset Password
                }
              </button>
            </form>
          }
        </div>
      </div>
    </div>
  `,
  styles: `
    :host {
      --primary-pink: #f857a6;
      --primary-pink-dark: #e91e8c;
      --text-dark: #1a1a2e;
      --text-gray: #6b7280;
      --text-light: #9ca3af;
      --border-color: #e5e7eb;
      --bg-white: #ffffff;
      --bg-light: #f9fafb;
    }

    .auth-container {
      display: flex;
      min-height: 100vh;
      width: 100vw;
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
    }

    .hero-section {
      flex: 1;
      background-image: url('/assets/login-hero.png');
      background-size: cover;
      background-position: center;
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .hero-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: linear-gradient(135deg, rgba(248, 87, 166, 0.2) 0%, rgba(233, 30, 140, 0.4) 100%);
    }

    .hero-content {
      position: relative;
      z-index: 1;
      text-align: center;
      padding: 2rem;
      color: white;
      max-width: 500px;
    }

    .hero-title {
      font-size: 3.5rem;
      font-weight: 700;
      margin-bottom: 1.5rem;
      text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .hero-subtitle {
      font-size: 1.25rem;
      line-height: 1.6;
      font-weight: 400;
      opacity: 0.95;
    }

    .form-section {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem;
      background-color: var(--bg-white);
    }

    .form-container {
      width: 100%;
      max-width: 480px;
    }

    .logo {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 2rem;
    }

    .logo-icon {
      width: 40px;
      height: 40px;
      background: linear-gradient(135deg, var(--primary-pink) 0%, var(--primary-pink-dark) 100%);
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
    }

    .logo-text {
      font-size: 1.25rem;
      font-weight: 600;
      color: var(--text-dark);
    }

    .welcome-title {
      font-size: 2.25rem;
      font-weight: 700;
      color: var(--text-dark);
      margin-bottom: 0.75rem;
    }

    .welcome-subtitle {
      font-size: 1rem;
      color: var(--text-gray);
      margin-bottom: 2rem;
      line-height: 1.5;
    }

    .signup-form {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .form-group label {
      font-size: 0.95rem;
      font-weight: 500;
      color: var(--text-dark);
    }

    .input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }

    .input-icon {
      position: absolute;
      left: 1rem;
      width: 20px;
      height: 20px;
      color: var(--text-light);
      pointer-events: none;
    }

    .input-wrapper input {
      width: 100%;
      padding: 0.875rem 2.75rem 0.875rem 2.75rem;
      border: 1px solid var(--border-color);
      border-radius: 10px;
      font-size: 0.95rem;
      color: var(--text-dark);
      background-color: var(--bg-white);
      transition: border-color 0.2s, box-shadow 0.2s;
    }

    .input-wrapper input:focus {
      outline: none;
      border-color: var(--primary-pink);
      box-shadow: 0 0 0 3px rgba(248, 87, 166, 0.1);
    }

    .input-wrapper input::placeholder {
      color: var(--text-light);
    }

    .toggle-password {
      position: absolute;
      right: 1rem;
      background: none;
      border: none;
      cursor: pointer;
      padding: 0;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .eye-icon {
      width: 20px;
      height: 20px;
      color: var(--text-light);
    }

    .field-error {
      color: #f44336;
      font-size: 0.8rem;
      font-weight: 500;
    }

    .error-text {
      color: #f44336;
      font-size: 0.9rem;
      font-weight: 500;
      margin: 0;
      text-align: center;
    }

    .submit-btn {
      width: 100%;
      padding: 1rem;
      background: linear-gradient(135deg, var(--primary-pink) 0%, var(--primary-pink-dark) 100%);
      color: white;
      border: none;
      border-radius: 10px;
      font-size: 1rem;
      font-weight: 600;
      cursor: pointer;
      transition: transform 0.2s, box-shadow 0.2s;
      margin-top: 0.5rem;
      display: flex;
      align-items: center;
      justify-content: center;
      text-decoration: none;
    }

    .submit-btn:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(248, 87, 166, 0.4);
    }

    .submit-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .loader {
      width: 20px;
      height: 20px;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .state-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      width: 100%;
    }

    .icon-container {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 1.5rem;
    }

    .icon-container.success {
      background-color: rgba(76, 175, 80, 0.1);
      color: #4caf50;
    }

    .icon-container.error {
      background-color: rgba(244, 67, 54, 0.1);
      color: #f44336;
    }

    @media (max-width: 900px) {
      .hero-section { display: none; }
      .form-section { flex: none; width: 100%; padding: 1.5rem; }
    }

    @media (max-width: 480px) {
      .welcome-title { font-size: 1.875rem; }
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
  hideNewPassword = signal(true);
  hideConfirmPassword = signal(true);

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