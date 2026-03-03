import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  UserProfile,
  UpdateUserProfileRequest,
  ChangePasswordRequest,
} from '../../domain/entities/user-profile.entity';
import { environment } from '../../../environments/environment';

interface UploadResponse {
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root',
})
export class UserDataSource {
  private readonly apiUrl = `${environment.baseUrl}/user`;
  private readonly baseUrlPrefix = environment.baseUrl.replace(/\/api$/i, '');

  constructor(private http: HttpClient) {}

  private getAbsoluteUrl(url: string | undefined): string | undefined {
    if (!url) return url;
    if (url.startsWith('http')) return url;
    return `${this.baseUrlPrefix}/${url.replace(/^\//, '')}`;
  }

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`).pipe(
      map((profile: UserProfile) => {
        if (profile.profilePictureUrl) {
          profile.profilePictureUrl = this.getAbsoluteUrl(profile.profilePictureUrl);
        }
        return profile;
      }),
    );
  }

  updateProfile(request: UpdateUserProfileRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/profile`, request);
  }

  uploadProfilePicture(file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<UploadResponse>(`${this.apiUrl}/profile-picture`, formData).pipe(
      map((response: UploadResponse) => {
        // Extract the image URL from the message (format: "success|url")
        const parts = response.message.split('|');
        const rawUrl = parts.length > 1 ? parts[1] : '';
        return this.getAbsoluteUrl(rawUrl) || '';
      }),
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/change-password`, request);
  }
}
