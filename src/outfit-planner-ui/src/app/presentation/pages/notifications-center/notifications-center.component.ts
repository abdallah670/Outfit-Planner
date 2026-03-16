import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { NotificationService, Notification } from '../../../core/services/notification.service';

@Component({
  selector: 'app-notifications-center',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTabsModule],
  templateUrl: './notifications-center.component.html',
  styleUrl: './notifications-center.component.scss'
})
export class NotificationsCenterComponent implements OnInit {
  private readonly notificationService = inject(NotificationService);
  
  activeTab = signal(0);
  
  // Use service signals
  get notifications(): Notification[] {
    return this.notificationService.notifications();
  }
  
  get unreadCount(): number {
    return this.notificationService.unreadCount();
  }
  
  get isLoading(): boolean {
    return this.notificationService.isLoading();
  }

  ngOnInit(): void {
    // Load notifications from the backend
    this.notificationService.getNotifications().subscribe();
  }

  get filteredNotifications(): Notification[] {
    const tab = this.activeTab();
    if (tab === 1) {
      return this.notifications.filter(n => !n.isRead);
    }
    return this.notifications;
  }

  markAsRead(id: string): void {
    this.notificationService.markAsRead(id).subscribe({
      error: (err: unknown) => console.error('Error marking notification as read:', err)
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      error: (err: unknown) => console.error('Error marking all notifications as read:', err)
    });
  }

  deleteNotification(id: string): void {
    this.notificationService.deleteNotification(id).subscribe({
      error: (err: unknown) => console.error('Error deleting notification:', err)
    });
  }

  getTypeIcon(type: string): string {
    switch (type) {
      case 'social': return 'people';
      case 'weather': return 'cloud';
      case 'reminder': return 'notifications';
      case 'system': return 'settings';
      default: return 'notifications';
    }
  }

  getTypeColor(type: string): string {
    switch (type) {
      case 'social': return '#f8b4c4';
      case 'weather': return '#74b9ff';
      case 'reminder': return '#9caf88';
      case 'system': return '#636e72';
      default: return '#636e72';
    }
  }

  formatTimestamp(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutes = Math.floor(diff / (1000 * 60));
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (minutes < 1) return 'Just now';
    if (minutes < 60) return `${minutes}m ago`;
    if (hours < 24) return `${hours}h ago`;
    if (days < 7) return `${days}d ago`;
    return date.toLocaleDateString();
  }
}
