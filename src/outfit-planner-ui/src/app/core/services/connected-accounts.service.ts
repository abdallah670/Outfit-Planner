import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ConnectedAccount {
  provider: string;
  email: string;
  connectedAt: string;
}

export interface ConnectAccountRequest {
  provider: 'Google' | 'Facebook';
  returnUrl: string;
}

export interface ConnectAccountResponse {
  authorizationUrl: string;
}

@Injectable({
  providedIn: 'root',
})
export class ConnectedAccountsService {
  private readonly apiUrl = `${environment.baseUrl}/user/connected-accounts`;

  constructor(private http: HttpClient) {}

  getConnectedAccounts(): Observable<ConnectedAccount[]> {
    return this.http.get<ConnectedAccount[]>(this.apiUrl);
  }

  connectAccount(provider: 'Google' | 'Facebook'): Observable<ConnectAccountResponse> {
    const request: ConnectAccountRequest = {
      provider,
      returnUrl: `${window.location.origin}/settings`,
    };
    return this.http.post<ConnectAccountResponse>(`${this.apiUrl}/connect`, request);
  }

  disconnectAccount(provider: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/disconnect`, { provider });
  }
}
