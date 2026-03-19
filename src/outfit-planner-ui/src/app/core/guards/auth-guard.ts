import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { inject } from '@angular/core';
import Swal from 'sweetalert2';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Access the signal value correctly by calling it
  const isAuth = authService.isAuthenticated();

  if (!isAuth) {
    Swal.fire({
      icon: 'warning',
      title: 'Not Authorized',
      text: 'Please login first to access this page.',
      confirmButtonColor: '#3085d6',
    });
    router.navigateByUrl('/login');
    return false;
  }
  return true;
};
