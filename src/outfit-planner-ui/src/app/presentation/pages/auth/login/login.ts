import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { environment } from '../../../../../environments/environment';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
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
    // Redirect to backend OAuth endpoint
    window.location.href = `${environment.baseUrl}/Auth/google`;
  }

  // Facebook Login
  loginWithFacebook(): void {
    this.isLoading.set(true);
    // Redirect to backend OAuth endpoint
    window.location.href = `${environment.baseUrl}/Auth/facebook`;
  }
}
