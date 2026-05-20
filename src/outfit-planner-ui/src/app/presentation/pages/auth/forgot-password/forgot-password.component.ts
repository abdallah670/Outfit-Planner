import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-forgot-password',
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

          @if (isSuccess()) {
            <!-- Success State -->
            <div class="state-container success-state">
              <div class="icon-container success">
                <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                  <polyline points="22 4 12 14.01 9 11.01"/>
                </svg>
              </div>
              <h2 class="welcome-title">Check Your Email</h2>
              <p class="welcome-subtitle">If an account exists with this email, a password reset link has been sent. Please check your inbox and follow the instructions.</p>
              <button class="submit-btn" routerLink="/login">Back to Login</button>
            </div>
          } @else {
            <!-- Forgot Password Form -->
            <h2 class="welcome-title">Forgot Password?</h2>
            <p class="welcome-subtitle">Enter your email to receive a password reset link.</p>

            <form [formGroup]="forgotForm" (ngSubmit)="onSubmit()" class="signup-form">
              <div class="form-group">
                <label for="email">Email Address</label>
                <div class="input-wrapper">
                  <svg class="input-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/>
                  </svg>
                  <input type="email" id="email" formControlName="email" placeholder="your@email.com">
                </div>
                @if (forgotForm.get('email')?.invalid && forgotForm.get('email')?.touched) {
                  <span class="field-error">
                    @if (forgotForm.get('email')?.hasError('required')) {
                      Email is required
                    } @else if (forgotForm.get('email')?.hasError('email')) {
                      Please enter a valid email
                    }
                  </span>
                }
              </div>

              @if (errorMessage()) {
                <p class="error-text">{{ errorMessage() }}</p>
              }

              <button type="submit" class="submit-btn" [disabled]="forgotForm.invalid || isLoading()">
                @if (isLoading()) {
                  <span class="loader"></span>
                } @else {
                  Send Reset Link
                }
              </button>
            </form>

            <p class="sign-in-text">
              Remember your password? <a routerLink="/login" class="sign-in-link">Back to Login</a>
            </p>
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
      padding: 0.875rem 1rem 0.875rem 2.75rem;
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

    .sign-in-text {
      text-align: center;
      font-size: 0.95rem;
      color: var(--text-gray);
      margin-top: 1.5rem;
    }

    .sign-in-link {
      color: var(--primary-pink);
      text-decoration: none;
      font-weight: 600;
    }

    .sign-in-link:hover {
      color: var(--primary-pink-dark);
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

    @media (max-width: 900px) {
      .hero-section { display: none; }
      .form-section { flex: none; width: 100%; padding: 1.5rem; }
    }

    @media (max-width: 480px) {
      .welcome-title { font-size: 1.875rem; }
    }
  `
})
export class ForgotPasswordComponent {
  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

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
        // Log out current user if logged in (security: password reset invalidates session)
        if (this.authService.isAuthenticated()) {
          setTimeout(() => {
            this.authService.logout();
          }, 500);
        }
      },
      error: () => {
        this.isLoading.set(false);
        // Still show success to prevent email enumeration
        this.isSuccess.set(true);
        // Log out current user if logged in
        if (this.authService.isAuthenticated()) {
          setTimeout(() => {
            this.authService.logout();
          }, 500);
        }
      }
    });
  }
}