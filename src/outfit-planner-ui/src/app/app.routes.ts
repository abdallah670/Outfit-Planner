import { Routes } from '@angular/router';
import { Login } from './presentation/pages/auth/login/login';
import { Register } from './presentation/pages/auth/register/register';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
];
