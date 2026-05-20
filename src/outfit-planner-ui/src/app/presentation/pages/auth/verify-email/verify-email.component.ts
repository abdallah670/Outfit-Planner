import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../../core/services/auth.service';
import { VerifyEmailRequest } from '../../../../data/models/auth.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
  ],
  template: `
    <div class="auth-container">
      <!-- Left Side - Hero Image Section (Identical to Login/Register) -->
      <div class="hero-section">
        <div class="hero-overlay"></div>
        <div class="hero-content">
          <h1 class="hero-title">Outfit Planner</h1>
          <p class="hero-subtitle">Organize your wardrobe, plan your style, and look your best every day.</p>
        </div>
      </div>

      <!-- Right Side - Form/Action Section -->
      <div class="form-section">
        <div class="form-container">
          <!-- Logo (Identical to Login/Register) -->
          <div class="logo">
            <div class="logo-icon">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M20.38 3.46L16 2a4 4 0 01-8 0L3.62 3.46a2 2 0 00-1.34 2.23l.58 3.47a1 1 0 00.99.84H6v10c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V10h2.15a1 1 0 00.99-.84l.58-3.47a2 2 0 00-1.34-2.23z"/>
              </svg>
            </div>
            <span class="logo-text">Outfit Planner</span>
          </div>

          <!-- Dynamic States Based on Action -->
          @if (isLoading()) {
            <div class="state-container loading-state">
              <div class="spinner-container">
                <div class="loader pink-loader"></div>
              </div>
              <h2 class="welcome-title">Processing...</h2>
              <p class="welcome-subtitle">Please wait while we process your request.</p>
            </div>
          } @else if (isSuccess()) {
            <div class="state-container success-state">
              <div class="icon-container success">
                <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                  <polyline points="22 4 12 14.01 9 11.01"/>
                </svg>
              </div>
              <h2 class="welcome-title">Email Verified!</h2>
              <p class="welcome-subtitle">Your email address has been successfully verified.</p>
              <button class="submit-btn" routerLink="/login">
                Go to Login
              </button>
            </div>
          } @else if (tokenRequired()) {
            <div class="state-container">
              <h2 class="welcome-title">Verify your Email</h2>
              <p class="welcome-subtitle">We have sent a verification code to <strong class="highlight">{{ email() }}</strong>.</p>
              
              <div class="signup-form">
                <div class="form-group">
                  <label for="verification-code">Verification Code</label>
                  <div class="input-wrapper">
                    <svg class="input-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0110 0v4"/>
                    </svg>
                    <input type="text" id="verification-code" #codeInput placeholder="Enter code (e.g. 123456)" class="code-input" maxlength="20">
                  </div>
                </div>

                <button type="button" class="submit-btn" (click)="verifyManualCode(codeInput.value)">
                  Verify Code
                </button>

                <button type="button" class="resend-btn" (click)="resendEmail()">
                  Resend Code
                </button>

                @if (manualError()) {
                  <p class="error-msg">{{ manualError() }}</p>
                }
              </div>
            </div>
          } @else if (errorMessage()) {
            <div class="state-container error-state">
              <div class="icon-container error">
                <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
                </svg>
              </div>
              <h2 class="welcome-title">Verification Failed</h2>
              <p class="welcome-subtitle error-text">{{ errorMessage() }}</p>
              
              @if (email()) {
                <button type="button" class="submit-btn" (click)="resendEmail()">
                  Resend Verification Email
                </button>
              } @else {
                <div class="signup-form" style="width: 100%;">
                  <div class="form-group">
                    <label for="email-address">Email Address</label>
                    <div class="input-wrapper">
                      <svg class="input-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/>
                      </svg>
                      <input type="email" id="email-address" #emailInput placeholder="sarah@example.com">
                    </div>
                  </div>
                  <button type="button" class="submit-btn" (click)="requestVerificationForEmail(emailInput.value)">
                    Send Verification Code
                  </button>
                </div>
              }
            </div>
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

    /* Left Side - Hero Section (Identical copy) */
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
      background: linear-gradient(
        135deg,
        rgba(248, 87, 166, 0.2) 0%,
        rgba(233, 30, 140, 0.4) 100%
      );
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

    /* Right Side - Form/Action Section (Identical copy) */
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

    /* Logo (Identical copy) */
    .logo {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 2.5rem;
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

    /* Titles and Typography (Identical copy) */
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

    .highlight {
      color: var(--primary-pink);
      font-weight: 600;
    }

    /* Custom Verification States */
    .state-container {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      text-align: left;
      width: 100%;
    }

    .loading-state, .success-state, .error-state {
      align-items: center;
      text-align: center;
      width: 100%;
    }

    .signup-form {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
      width: 100%;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      width: 100%;
    }

    .form-group label {
      font-size: 0.95rem;
      font-weight: 500;
      color: var(--text-dark);
      text-align: left;
    }

    .input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
      width: 100%;
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

    /* Monospace verification code input field */
    .code-input {
      font-family: monospace;
      font-size: 1.2rem !important;
      letter-spacing: 3px;
      text-align: center;
      padding-left: 1rem !important;
    }

    /* Premium submit buttons (identical to login/register buttons) */
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

    .submit-btn:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(248, 87, 166, 0.4);
    }

    .submit-btn:active {
      transform: translateY(0);
    }

    .resend-btn {
      width: 100%;
      padding: 0.75rem;
      background-color: var(--bg-white);
      border: 1px solid var(--border-color);
      border-radius: 10px;
      color: var(--text-dark);
      font-size: 0.95rem;
      font-weight: 500;
      cursor: pointer;
      transition: border-color 0.2s, background-color 0.2s;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .resend-btn:hover {
      border-color: var(--primary-pink);
      background-color: var(--bg-light);
    }

    /* Loading Spinner */
    .spinner-container {
      margin-bottom: 1.5rem;
    }

    .pink-loader {
      width: 48px;
      height: 48px;
      border: 4px solid rgba(248, 87, 166, 0.1);
      border-top-color: var(--primary-pink);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to {
        transform: rotate(360deg);
      }
    }

    /* Success / Error Circular Icons */
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

    .error-msg {
      color: #f44336;
      font-size: 0.9rem;
      font-weight: 500;
      margin-top: 0.75rem;
      width: 100%;
      text-align: center;
    }

    .error-text {
      color: #f44336 !important;
    }

    /* Responsive Queries (Identical copy) */
    @media (max-width: 900px) {
      .hero-section {
        display: none;
      }
      
      .form-section {
        flex: none;
        width: 100%;
        padding: 1.5rem;
      }
    }

    @media (max-width: 480px) {
      .welcome-title {
        font-size: 1.875rem;
      }
    }
  `
})
export class VerifyEmailComponent {
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  isLoading = signal(true);
  isSuccess = signal(false);
  tokenRequired = signal(false);
  errorMessage = signal('');
  manualError = signal('');
  email = signal('');

  constructor() {
    this.verifyEmail();
  }

  private verifyEmail(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    const email = this.route.snapshot.queryParamMap.get('email');

    if (email) {
      this.email.set(email);
    }

    if (!token) {
      this.isLoading.set(false);
      if (email) {
        this.tokenRequired.set(true);
      } else {
        this.errorMessage.set('Invalid verification link. Please check your email or request a new verification code.');
      }
      return;
    }

    const request: VerifyEmailRequest = { token, email: email || '' };
    
    this.authService.verifyEmail(request).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.isSuccess.set(true);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || 'Verification failed. Please try again.');
      }
    });
  }

  verifyManualCode(code: string): void {
    if (!code || code.trim().length === 0) {
      this.manualError.set('Please enter the verification code.');
      return;
    }
    
    this.isLoading.set(true);
    this.manualError.set('');
    
    const request: VerifyEmailRequest = { token: code.trim(), email: this.email() };
    
    this.authService.verifyEmail(request).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.isSuccess.set(true);
        this.tokenRequired.set(false);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.manualError.set(error.error?.message || 'Verification failed. Please check the code and try again.');
      }
    });
  }

  requestVerificationForEmail(email: string): void {
    if (!email || !email.includes('@')) {
      this.errorMessage.set('Please enter a valid email address.');
      return;
    }
    
    this.email.set(email);
    this.isLoading.set(true);
    this.errorMessage.set('');
    
    this.authService.resendVerificationEmail({ email }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.tokenRequired.set(true);
        Swal.fire({
          icon: 'success',
          title: 'Verification Email Sent',
          text: 'A verification email has been sent to your address. Please check your inbox and follow the instructions.',
          confirmButtonText: 'OK'
        });
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || 'Failed to send verification email.');
      }
    });
  }

  resendEmail(): void {
    if (!this.email()) return;

    this.isLoading.set(true);
    this.errorMessage.set('');
    this.manualError.set('');

    this.authService.resendVerificationEmail({ email: this.email() }).subscribe({
      next: () => {
        this.isLoading.set(false);
        Swal.fire({
          icon: 'success',
          title: 'Verification Email Sent',
          text: 'A new verification email has been sent to your address. Please check your inbox and follow the instructions.',
          confirmButtonText: 'OK'
        });
      },
      error: (error) => {
        this.isLoading.set(false);
        if (this.tokenRequired()) {
          this.manualError.set(error.error?.message || 'Failed to resend verification email.');
        } else {
          this.errorMessage.set(error.error?.message || 'Failed to resend verification email.');
        }
      }
    });
  }
}
