import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface NotificationSettings {
  dailyOutfitSuggestion: boolean;
  weeklyStyleReport: boolean;
  weatherAlerts: boolean;
  newFeatures: boolean;
  socialNotifications: boolean;
  pushNotificationsEnabled: boolean;
}

export interface UpdateNotificationSettingsRequest {
  dailyOutfitSuggestion: boolean;
  weeklyStyleReport: boolean;
  weatherAlerts: boolean;
  newFeatures: boolean;
  socialNotifications: boolean;
  pushNotificationsEnabled: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationSettingsService {
  private readonly apiUrl = `${environment.baseUrl}/user/notification-settings`;

  constructor(private http: HttpClient) {}

  getSettings(): Observable<NotificationSettings> {
    return this.http.get<NotificationSettings>(this.apiUrl);
  }

  updateSettings(settings: UpdateNotificationSettingsRequest): Observable<void> {
    return this.http.put<void>(this.apiUrl, settings);
  }
}
