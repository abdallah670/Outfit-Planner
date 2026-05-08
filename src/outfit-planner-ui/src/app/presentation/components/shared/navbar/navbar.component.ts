import { Component, OnInit, HostListener, Signal, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';
import { selectProfilePictureUrl } from '../../../../core/state/user/user.selectors';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatBadgeModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss'],
})
export class NavbarComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly notificationService = inject(NotificationService);
  private readonly authService = inject(AuthService);

  profilePictureUrl$!: Observable<string | null>;
  failedImages: Set<string> = new Set();
  notificationsCount$!: Observable<number>;
  isAdmin: Signal<boolean> = this.authService.isAdmin;

  ngOnInit() {
    this.profilePictureUrl$ = this.store.select(selectProfilePictureUrl);
    this.notificationsCount$ = this.notificationService.getUnreadCount();
  }

  onImageError(event: Event, imageUrl: string | unknown) {
    const img = event.target as HTMLImageElement;
    const url = typeof imageUrl === 'string' ? imageUrl : '';
    if (url && !this.failedImages.has(url)) {
      this.failedImages.add(url);
      img.src = 'assets/default-avatar.png';
    }
  }

  isImageFailed(imageUrl: string | null): boolean {
    return imageUrl ? this.failedImages.has(imageUrl) : false;
  }
}