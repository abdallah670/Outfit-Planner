import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { UserProfile } from '../../../../domain/entities/user-profile.entity';
import {
  selectProfilePictureUrl,
  selectUserProfile,
} from '../../../../core/state/user/user.selectors';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatBadgeModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss'],
})
export class NavbarComponent implements OnInit {
  profilePictureUrl$: Observable<string | null>;
  failedImages: Set<string> = new Set();

  constructor(private store: Store) {
    this.profilePictureUrl$ = this.store.select(selectProfilePictureUrl);
  }

  ngOnInit() {}

  onImageError(event: Event, imageUrl: string | unknown) {
    const img = event.target as HTMLImageElement;
    const url = typeof imageUrl === 'string' ? imageUrl : '';
    // Prevent infinite requests by tracking failed images
    if (url && !this.failedImages.has(url)) {
      this.failedImages.add(url);
      // Force re-evaluation of the template to show fallback
      img.style.display = 'none';
    }
  }

  isImageFailed(url: string | null | unknown): boolean {
    if (!url || typeof url !== 'string') return true;
    return this.failedImages.has(url);
  }
}
