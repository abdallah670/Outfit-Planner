import { Inject, Injectable } from '@angular/core';
import { USER_REPOSITORY_TOKEN, UserRepository } from '../repositories/user.repository';
import { Observable } from 'rxjs';
import { PublicUserProfile } from '../entities/public-user-profile.entity';
import { ChangePasswordRequest, UpdateEmailRequest, UpdateUserProfileRequest, UserProfile } from '../entities/user-profile.entity';


@Injectable({
  providedIn: 'root',
})
export class UserUseCases {
  constructor(
   @Inject(USER_REPOSITORY_TOKEN) private readonly userRepository: UserRepository,
  ) {}

  //Get Profile
  getProfile(): Observable<UserProfile> {
    return this.userRepository.getProfile();
  }
  //Get Profile Picture
  getProfilePicture(): Observable<string> {
    return this.userRepository.getProfilePicture();
  }
  // Get public profile for any user
  getPublicProfile(userId: string): Observable<PublicUserProfile> {
    return this.userRepository.getPublicProfile(userId);
  }
  
  // Update profile
  updateProfile(request: UpdateUserProfileRequest): Observable<void> {
    return this.userRepository.updateProfile(request);
  }
  
  // Upload profile picture
  uploadProfilePicture(file: File): Observable<string> {
    return this.userRepository.uploadProfilePicture(file);
  }
  
  // Change password
  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.userRepository.changePassword(request);
  }
  
  // Update email
  updateEmail(request: UpdateEmailRequest): Observable<{ success: boolean; message: string }> {
    return this.userRepository.updateEmail(request);
  }


}
