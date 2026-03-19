import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NotificationService, Notification } from '../../../core/services/notification.service';

type NotificationCategory = 'all' | 'unread' | 'social' | 'reminder' | 'system';

interface NotificationGroup {
  title: string;
  notifications: Notification[];
}

@Component({
  selector: 'app-notifications-center',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule],
  templateUrl: './notifications-center.component.html',
  styleUrl: './notifications-center.component.scss'
})
export class NotificationsCenterComponent implements OnInit {
  private readonly notificationService = inject(NotificationService);
  
  // Category selection
  selectedCategory: NotificationCategory = 'all';
  
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

  // Group notifications by time period
  get groupedNotifications(): NotificationGroup[] {
    const filtered = this.getFilteredNotifications();
    const groups: NotificationGroup[] = [];
    
    const today: Notification[] = [];
    const yesterday: Notification[] = [];
    const lastWeek: Notification[] = [];
    const older: Notification[] = [];
    
    const now = new Date();
    const todayDate = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const yesterdayDate = new Date(todayDate.getTime() - 24 * 60 * 60 * 1000);
    const lastWeekDate = new Date(todayDate.getTime() - 7 * 24 * 60 * 60 * 1000);
    
    filtered.forEach(n => {
      const notifDate = new Date(n.createdAt);
      if (notifDate >= todayDate) {
        today.push(n);
      } else if (notifDate >= yesterdayDate) {
        yesterday.push(n);
      } else if (notifDate >= lastWeekDate) {
        lastWeek.push(n);
      } else {
        older.push(n);
      }
    });
    
    if (today.length > 0) groups.push({ title: 'Today', notifications: today });
    if (yesterday.length > 0) groups.push({ title: 'Yesterday', notifications: yesterday });
    if (lastWeek.length > 0) groups.push({ title: 'Last Week', notifications: lastWeek });
    if (older.length > 0) groups.push({ title: 'Earlier', notifications: older });
    
    return groups;
  }

  private getFilteredNotifications(): Notification[] {
    if (this.selectedCategory === 'all') {
      return this.notifications;
    }
    if (this.selectedCategory === 'unread') {
      return this.notifications.filter(n => !n.isRead);
    }
    // Case-insensitive comparison for notification types
    return this.notifications.filter(n => {
      const typeStr = this.getNormalizedType(n);
      if (this.selectedCategory === 'reminder') {
        return typeStr === 'reminder' || typeStr === 'weather';
      }
      return typeStr === this.selectedCategory;
    });
  }

  // Utility to handle both enum numbers and strings, and property casing
  getNormalizedType(n: any): string {
    const type = n.type !== undefined ? n.type : n.Type;
    if (type === undefined || type === null) return '';
    
    if (typeof type === 'number') {
      const types = ['social', 'reminder', 'weather', 'system'];
      return types[type] || '';
    }
    return String(type).toLowerCase();
  }

  isUnread(n: any): boolean {
    const isRead = n.isRead !== undefined ? n.isRead : n.IsRead;
    return !isRead;
  }

  get filteredNotifications(): Notification[] {
    return this.getFilteredNotifications();
  }

  selectCategory(category: NotificationCategory): void {
    this.selectedCategory = category;
  }

  getCategoryCount(category: string): number {
    return this.notifications.filter(n => n.type === category && !n.isRead).length;
  }

  getCategoryTitle(): string {
    const titles: Record<NotificationCategory, string> = {
      all: 'All Notifications',
      unread: 'Unread',
      social: 'Social',
      reminder: 'Outfit Reminders',
      system: 'System'
    };
    return titles[this.selectedCategory];
  }

  markAsRead(id: any): void {
    if (!id) return;
    this.notificationService.markAsRead(id).subscribe({
      error: (err: unknown) => console.error('Error marking notification as read:', err)
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      error: (err: unknown) => console.error('Error marking all notifications as read:', err)
    });
  }

  deleteNotification(id: any): void {
    if (!id) return;
    this.notificationService.deleteNotification(id).subscribe({
      error: (err: unknown) => console.error('Error deleting notification:', err)
    });
  }

  getTypeIcon(type: any): string {
    const typeStr = this.getNormalizedType({ type: type });
    switch (typeStr) {
      case 'social': return 'favorite_border'; // Matches lucide:heart
      case 'reminder': return 'calendar_month'; // Matches lucide:calendar-clock roughly
      case 'weather': return 'umbrella'; // Matches lucide:umbrella
      case 'system': return 'auto_awesome'; // Matches lucide:sparkles
      default: return 'notifications_none';
    }
  }

  getActionText(n: any): string {
    const actionUrl = (n.actionUrl || n.ActionUrl || '').toLowerCase();
    const title = (n.title || n.Title || '').toLowerCase();

    if (actionUrl.includes('/calendar')) {
      return title.includes('log') ? 'Log now' : 'View calendar';
    }
    if (actionUrl.includes('/outfits/')) {
      return title.includes('comment') ? 'Reply' : 'View outfit';
    }
    if (actionUrl.includes('/profile/stats')) {
      return 'View stats';
    }
    if (actionUrl.includes('/weather')) {
      return 'View forecast';
    }
    if (actionUrl.includes('/settings')) {
      return 'Review settings';
    }
    if (actionUrl.includes('/community')) {
      return 'View profile';
    }
    if (actionUrl.includes('/wardrobe/')) {
      return 'View item';
    }

    return 'View details';
  }

  getActionIcon(n: any): string {
    const actionUrl = (n.actionUrl || n.ActionUrl || '').toLowerCase();
    const title = (n.title || n.Title || '').toLowerCase();

    if (actionUrl.includes('/calendar')) {
      return title.includes('log') ? 'arrow_forward' : 'event';
    }
    if (actionUrl.includes('/outfits/')) {
      return title.includes('comment') ? 'reply' : 'arrow_forward';
    }
    if (actionUrl.includes('/profile/stats')) {
      return 'bar_chart';
    }
    if (actionUrl.includes('/weather')) {
      return 'cloud';
    }
    if (actionUrl.includes('/settings')) {
      return 'security';
    }
    if (actionUrl.includes('/community')) {
      return 'person';
    }
    if (actionUrl.includes('/wardrobe/')) {
      return 'checkroom';
    }

    return 'arrow_forward';
  }

  formatTimestamp(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutes = Math.floor(diff / (1000 * 60));
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    const today = new Date();
    if (date.toDateString() === today.toDateString()) {
      if (minutes < 1) return 'Just now';
      if (minutes < 60) return `${minutes}m ago`;
      if (hours < 24) return `${hours}h ago`;
    }
    
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);
    if (date.toDateString() === yesterday.toDateString()) {
      return 'Yesterday';
    }
    
    if (days < 7) return `${days}d ago`;
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}
