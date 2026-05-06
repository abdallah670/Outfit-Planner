import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { UserRepository } from '../../domain/repositories/user.repository';
import {
  UserProfile,
  UpdateUserProfileRequest,
  ChangePasswordRequest,
  UpdateEmailRequest,
} from '../../domain/entities/user-profile.entity';
import { PublicUserProfile } from '../../domain/entities/public-user-profile.entity';
import { UserDataSource } from '../datasources/user.datasource';

@Injectable({
  providedIn: 'root',
})
export class UserRepositoryImpl implements UserRepository {
  constructor(private dataSource: UserDataSource) {}

  getProfile(): Observable<UserProfile> {
    return this.dataSource.getProfile();
  }

  getProfilePicture(): Observable<string> {
    return this.dataSource.getProfilePicture();
  }

  getPublicProfile(userId: string): Observable<PublicUserProfile> {
    return this.dataSource.getPublicUserProfile(userId);
  }

  updateProfile(request: UpdateUserProfileRequest): Observable<void> {
    return this.dataSource.updateProfile(request);
  }

  uploadProfilePicture(file: File): Observable<string> {
    return this.dataSource.uploadProfilePicture(file);
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.dataSource.changePassword(request);
  }

  updateEmail(request: UpdateEmailRequest): Observable<{ success: boolean; message: string }> {
    return this.dataSource.updateEmail(request);
  }
}

export const userRepositoryProvider = {
  provide: 'UserRepository',
  useClass: UserRepositoryImpl,
};