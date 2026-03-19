import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CookieService } from 'ngx-cookie-service';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthRequest,
  AuthResponse,
  RegistrationRequest,
  RegistrationResponse,
} from '../../data/models/auth.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService{
  private readonly http = inject(HttpClient);
  private readonly cookieService = inject(CookieService);

  private readonly apiUrl = `${environment.baseUrl}/Auth`;

  // Signals for reactive state
  currentUser = signal<any>(null);
  isAuthenticated = signal<boolean>(false);

  constructor() {
    this.checkAuthStatus();
  }

  login(request: AuthRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/login`, request)
      .pipe(tap((response: AuthResponse) => this.handleAuthentication(response)));
  }

  register(request: RegistrationRequest): Observable<RegistrationResponse> {
    return this.http.post<RegistrationResponse>(`${this.apiUrl}/register`, request).pipe(
      tap((response: RegistrationResponse) =>
        this.handleAuthentication({
          id: response.userId,
          userName: response.userName,
          email: response.email,
          token: response.token,
          refreshToken: response.refreshToken,
        }),
      ),
    );
  }

  logout(): void {
    this.cookieService.delete('token', '/');
    this.cookieService.delete('refreshToken', '/');
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
  }

  refreshToken(): Observable<AuthResponse> {
    const token = this.cookieService.get('token');
    const refreshToken = this.cookieService.get('refreshToken');

    if (!token || !refreshToken) {
      this.logout();
      return of(null as any);
    }

    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, { Token: token, RefreshToken: refreshToken }).pipe(
      tap((response: AuthResponse) => this.handleAuthentication(response)),
      catchError(() => {
        this.logout();
        return of(null as any);
      }),
    );
  }

  private handleAuthentication(response: AuthResponse): void {
    // Store in secure cookies
    this.cookieService.set('token', response.token, 7, '/');
    if (response.refreshToken) {
      this.cookieService.set('refreshToken', response.refreshToken, 7, '/');
    }

    console.log('[AuthService] Token stored in cookies:', response.token.substring(0, 20) + '...');
    console.log('[AuthService] Refresh token stored:', response.refreshToken ? 'Yes' : 'No');

    this.currentUser.set({
      id: response.id,
      userName: response.userName,
      email: response.email,
    });
    this.isAuthenticated.set(true);
    console.log('[AuthService] isAuthenticated set to:', this.isAuthenticated());
  }

  // Handle OAuth social login callback
  handleSocialLogin(response: AuthResponse): void {
    this.handleAuthentication(response);
  }

  private checkAuthStatus(): void {
    const token = this.cookieService.get('token');
    if (token) {
      this.isAuthenticated.set(true);
      // Optional: Logic to fetch user profile using the token
    }
  }
}
