import { Observable } from 'rxjs';
import { UserProfile, UpdateUserProfileRequest, ChangePasswordRequest } from '../entities/user-profile.entity';

export interface UserRepository {
  getProfile(): Observable<UserProfile>;
  updateProfile(request: UpdateUserProfileRequest): Observable<void>;
  uploadProfilePicture(file: File): Observable<string>;
  changePassword(request: ChangePasswordRequest): Observable<void>;
}

export const USER_REPOSITORY_TOKEN = 'UserRepository';
