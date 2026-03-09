import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);

  loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  // Toggle password visibility
  togglePassword(): void {
    this.showPassword.update(value => !value);
  }

  // Email/Password Login
  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.loginForm.getRawValue()).subscribe({
      next: () => {
        Swal.fire({
          icon: 'success',
          title: 'Welcome Back!',
          text: 'You have successfully logged in.',
          timer: 2000,
          showConfirmButton: false,
          position: 'top-end',
          toast: true,
        });
        this.router.navigate(['/']);
      },
      error: (err: any) => {
        this.isLoading.set(false);
        this.errorMessage.set(
          err.error?.message || err.message || 'Login failed. Please check your credentials.',
        );
        Swal.fire({
          icon: 'error',
          title: 'Login Failed',
          text: this.errorMessage()!,
          confirmButtonColor: '#e63946',
        });
      },
    });
  }

  // Google Login
  loginWithGoogle(): void {
    this.isLoading.set(true);
    this.simulateSocialLogin('Google');
  }

  // Instagram Login
  loginWithInstagram(): void {
    this.isLoading.set(true);
    this.simulateSocialLogin('Instagram');
  }

  // Facebook Login
  loginWithFacebook(): void {
    this.isLoading.set(true);
    this.simulateSocialLogin('Facebook');
  }

  private simulateSocialLogin(provider: string): void {
    setTimeout(() => {
      this.isLoading.set(false);
      
      Swal.fire({
        icon: 'success',
        title: `Welcome!`,
        text: `You have successfully signed in with ${provider}.`,
        timer: 2000,
        showConfirmButton: false,
        position: 'top-end',
        toast: true,
      });

      localStorage.setItem('token', 'mock-social-token');
      localStorage.setItem('user', JSON.stringify({ 
        id: 'social-user', 
        email: `user@${provider.toLowerCase()}.com`,
        name: `${provider} User`
      }));

      this.router.navigate(['/']);
    }, 1500);
  }
}
