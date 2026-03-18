import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AppPreferences {
  temperatureUnit: 'Celsius' | 'Fahrenheit';
  language: string;
  theme: 'Light' | 'Dark' | 'Auto';
  measurementUnit: 'Metric' | 'Imperial';
}

export interface UpdateAppPreferencesRequest {
  temperatureUnit: string;
  language: string;
  theme: string;
  measurementUnit: string;
}

@Injectable({
  providedIn: 'root',
})
export class AppPreferencesService {
  private readonly apiUrl = `${environment.baseUrl}/user/app-preferences`;

  constructor(private http: HttpClient) {}

  getPreferences(): Observable<AppPreferences> {
    return this.http.get<AppPreferences>(this.apiUrl);
  }

  updatePreferences(preferences: UpdateAppPreferencesRequest): Observable<void> {
    return this.http.put<void>(this.apiUrl, preferences);
  }
}
