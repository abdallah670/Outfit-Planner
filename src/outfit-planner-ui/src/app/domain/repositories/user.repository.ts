import { Observable } from 'rxjs';
import {
  UserProfile,
  UpdateUserProfileRequest,
  ChangePasswordRequest,
  UpdateEmailRequest,
} from '../entities/user-profile.entity';

export interface UserRepository {
  getProfile(): Observable<UserProfile>;
  getProfilePicture(): Observable<string>;
  getPublicProfile(userId: string): Observable<UserProfile>;
  updateProfile(request: UpdateUserProfileRequest): Observable<void>;
  uploadProfilePicture(file: File): Observable<string>;
  changePassword(request: ChangePasswordRequest): Observable<void>;
  updateEmail(request: UpdateEmailRequest): Observable<{ success: boolean; message: string }>;
}

export const USER_REPOSITORY_TOKEN = 'UserRepository';
