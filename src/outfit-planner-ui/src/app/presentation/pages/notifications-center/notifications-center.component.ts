import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NotificationService, Notification } from '../../../core/services/notification.service';

type NotificationCategory = 'all' | 'unread' | 'social' | 'weather' | 'reminder' | 'system';

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
    if (this.selectedCategory === 'weather') {
      return this.notifications.filter(n => n.type === 'weather');
    }
    return this.notifications.filter(n => n.type === this.selectedCategory);
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
      weather: 'Weather',
      reminder: 'Reminders',
      system: 'System'
    };
    return titles[this.selectedCategory];
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
      case 'social': return 'favorite';
      case 'weather': return 'cloud';
      case 'reminder': return 'event';
      case 'system': return 'auto_awesome';
      default: return 'notifications';
    }
  }

  getActionText(type: string): string {
    switch (type) {
      case 'social': return 'View';
      case 'weather': return 'Check';
      case 'reminder': return 'Log now';
      case 'system': return 'View details';
      default: return 'View';
    }
  }

  getActionIcon(type: string): string {
    switch (type) {
      case 'social': return 'arrow_forward';
      case 'weather': return 'wb_sunny';
      case 'reminder': return 'event';
      case 'system': return 'bar_chart';
      default: return 'arrow_forward';
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
