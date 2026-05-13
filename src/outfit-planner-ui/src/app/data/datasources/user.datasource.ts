import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  UserProfile,
  UpdateUserProfileRequest,
  ChangePasswordRequest,
  UpdateEmailRequest,
} from '../../domain/entities/user-profile.entity';
import { PublicUserProfile } from '../../domain/entities/public-user-profile.entity';
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

  constructor(private http: HttpClient) { }

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

  getProfilePicture(): Observable<string> {
    return this.http.get<string>(`${this.apiUrl}/profile-picture`);
  }

  updateProfile(request: UpdateUserProfileRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/profile`, request);
  }

  uploadProfilePicture(file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<UploadResponse>(`${this.apiUrl}/profile-picture`, formData).pipe(
      map((response: UploadResponse) => {
        const parts = response.message.split('|');
        const rawUrl = parts.length > 1 ? parts[1] : '';
        return this.getAbsoluteUrl(rawUrl) || '';
      }),
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/change-password`, request);
  }

  updateEmail(request: UpdateEmailRequest): Observable<{ success: boolean; message: string }> {
    return this.http.put<{ success: boolean; message: string }>(`${this.apiUrl}/email`, request);
  }

  // ============ Follow Methods ============

  followUser(userId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/users/${userId}/follow`, {});
  }

  unfollowUser(userId: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/users/${userId}/unfollow`);
  }

  isFollowing(userId: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/users/${userId}/isfollowing`);
  }

  getFollowers(userId: string, cursor?: string, pageSize: number = 20): Observable<any> {
    let params = new HttpParams().set('pageSize', pageSize.toString());
    if (cursor) {
      params = params.set('cursor', cursor);
    }
    return this.http.get<any>(`${this.apiUrl}/users/${userId}/followers`, { params });
  }

  getFollowing(userId: string, cursor?: string, pageSize: number = 20): Observable<any> {
    let params = new HttpParams().set('pageSize', pageSize.toString());
    if (cursor) {
      params = params.set('cursor', cursor);
    }
    return this.http.get<any>(`${this.apiUrl}/users/${userId}/following`, { params });
  }

  // ============ Public Profile (for viewing other users) ============

  getPublicUserProfile(userId: string): Observable<PublicUserProfile> {
    return this.http.get<PublicUserProfile>(`${this.apiUrl}/users/${userId}/profile`).pipe(
      map((profile: PublicUserProfile) => {
        if (profile.profilePictureUrl) {
          profile.profilePictureUrl = this.getAbsoluteUrl(profile.profilePictureUrl);
        }
        return profile;
      }),
    );
  }
}
