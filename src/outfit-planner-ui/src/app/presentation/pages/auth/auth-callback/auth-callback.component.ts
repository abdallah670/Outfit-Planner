import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="auth-callback-container">
      <div class="loading-spinner">
        <div class="spinner"></div>
        <p>Processing login...</p>
      </div>
    </div>
  `,
  styles: [`
    .auth-callback-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }
    .loading-spinner {
      text-align: center;
      color: white;
    }
    .spinner {
      width: 50px;
      height: 50px;
      border: 4px solid rgba(255, 255, 255, 0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 20px;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `]
})
export class AuthCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  ngOnInit(): void {
    this.route.queryParams.subscribe((params: { [key: string]: any }) => {
      const token = params['token'];
      const refreshToken = params['refreshToken'];
      const error = params['error'];

      if (error) {
        console.error('OAuth error:', error);
        Swal.fire({
          icon: 'error',
          title: 'Login Failed',
          text: this.getErrorMessage(error),
          confirmButtonColor: '#e63946',
        });
        this.router.navigate(['/login'], { 
          queryParams: { error: 'social_login_failed' } 
        });
        return;
      }

      if (token) {
        // Handle successful OAuth login
        this.handleSuccessfulAuth(token, refreshToken);
      } else {
        this.router.navigate(['/login']);
      }
    });
  }

  private handleSuccessfulAuth(token: string, refreshToken?: string): void {
    // Create auth response object
    const authResponse = {
      id: '', // Will be extracted from token or fetched from API
      token: token,
      email: '', // Will be fetched from API
      userName: '', // Will be fetched from API
      refreshToken: refreshToken || ''
    };

    // Store the token using auth service
    this.authService.handleSocialLogin(authResponse);

    Swal.fire({
      icon: 'success',
      title: 'Welcome!',
      text: 'You have successfully logged in.',
      timer: 2000,
      showConfirmButton: false,
      position: 'top-end',
      toast: true,
    });

    this.router.navigate(['/']);
  }

  private getErrorMessage(error: string): string {
    switch (error) {
      case 'social_auth_failed':
        return 'Authentication with the social provider failed. Please try again.';
      case 'email_required':
        return 'Email access is required for login. Please grant permission.';
      case 'auth_error':
        return 'An error occurred during authentication. Please try again.';
      default:
        return 'Login failed. Please try again.';
    }
  }
}
