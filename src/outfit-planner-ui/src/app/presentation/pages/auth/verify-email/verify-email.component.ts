import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../../core/services/auth.service';
import { VerifyEmailRequest } from '../../../../data/models/auth.model';

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
    <div class="verify-email-container">
      <mat-card class="verify-card">
        <mat-card-header>
          <mat-card-title>Email Verification</mat-card-title>
        </mat-card-header>
        
        <mat-card-content>
          @if (isLoading()) {
            <div class="loading-state">
              <mat-spinner diameter="50"></mat-spinner>
              <p>Verifying your email...</p>
            </div>
          } @else if (isSuccess()) {
            <div class="success-state">
              <mat-icon class="success-icon">check_circle</mat-icon>
              <h2>Email Verified!</h2>
              <p>Your email has been successfully verified.</p>
              <p>You can now log in to your account.</p>
              <button mat-raised-button color="primary" routerLink="/login">
                Go to Login
              </button>
            </div>
          } @else if (errorMessage()) {
            <div class="error-state">
              <mat-icon class="error-icon">error</mat-icon>
              <h2>Verification Failed</h2>
              <p>{{ errorMessage() }}</p>
              <button mat-raised-button color="primary" (click)="resendEmail()">
                Resend Verification Email
              </button>
            </div>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: `
    .verify-email-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      padding: 20px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }
    .verify-card {
      max-width: 500px;
      width: 100%;
      padding: 30px;
      text-align: center;
    }
    .loading-state, .success-state, .error-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 20px;
      padding: 30px 0;
    }
    .success-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: #4caf50;
    }
    .error-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: #f44336;
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
export class VerifyEmailComponent {
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  isLoading = signal(true);
  isSuccess = signal(false);
  errorMessage = signal('');
  email = signal('');

  constructor() {
    this.verifyEmail();
  }

  private verifyEmail(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    const email = this.route.snapshot.queryParamMap.get('email');

    if (!token || !email) {
      this.isLoading.set(false);
      this.errorMessage.set('Invalid verification link. Please check your email and try again.');
      return;
    }

    this.email.set(email);

    const request: VerifyEmailRequest = { token, email };
    
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

  resendEmail(): void {
    if (!this.email()) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.resendVerificationEmail({ email: this.email() }).subscribe({
      next: () => {
        this.isLoading.set(false);
        alert('Verification email has been resent. Please check your inbox.');
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || 'Failed to resend verification email.');
      }
    });
  }
}
