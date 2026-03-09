import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';

interface Notification {
  id: string;
  type: 'social' | 'weather' | 'reminder' | 'system';
  title: string;
  message: string;
  timestamp: Date;
  isRead: boolean;
  actionUrl?: string;
}

@Component({
  selector: 'app-notifications-center',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTabsModule],
  templateUrl: './notifications-center.component.html',
  styleUrl: './notifications-center.component.scss'
})
export class NotificationsCenterComponent implements OnInit {
  activeTab = signal(0);
  
  // Mock notifications
  notifications = signal<Notification[]>([
    {
      id: '1',
      type: 'social',
      title: 'New Like',
      message: 'Sarah liked your "Date Night Outfit"',
      timestamp: new Date(Date.now() - 1000 * 60 * 5), // 5 minutes ago
      isRead: false,
      actionUrl: '/outfits/1'
    },
    {
      id: '2',
      type: 'weather',
      title: 'Weather Alert',
      message: 'Rain expected tomorrow. Your planned outfit might need adjustment.',
      timestamp: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
      isRead: false,
      actionUrl: '/outfits/today'
    },
    {
      id: '3',
      type: 'reminder',
      title: 'Outfit Reminder',
      message: 'You have "Business Meeting" scheduled for tomorrow at 2:00 PM',
      timestamp: new Date(Date.now() - 1000 * 60 * 60), // 1 hour ago
      isRead: true,
      actionUrl: '/calendar'
    },
    {
      id: '4',
      type: 'social',
      title: 'New Comment',
      message: 'Mike commented on your "Weekend Brunch Look"',
      timestamp: new Date(Date.now() - 1000 * 60 * 60 * 2), // 2 hours ago
      isRead: true,
      actionUrl: '/outfits/2'
    },
    {
      id: '5',
      type: 'system',
      title: 'Welcome to Outfit Planner!',
      message: 'Get started by adding your first clothing items',
      timestamp: new Date(Date.now() - 1000 * 60 * 60 * 24), // 1 day ago
      isRead: true,
      actionUrl: '/wardrobe/new'
    },
    {
      id: '6',
      type: 'reminder',
      title: 'Wear Count Update',
      message: 'You\'ve worn your "Blue Denim Jacket" 10 times this month!',
      timestamp: new Date(Date.now() - 1000 * 60 * 60 * 48), // 2 days ago
      isRead: true,
      actionUrl: '/wardrobe/1'
    }
  ]);

  ngOnInit(): void {
    // Initialize
  }

  get filteredNotifications(): Notification[] {
    const tab = this.activeTab();
    if (tab === 1) {
      return this.notifications().filter(n => !n.isRead);
    }
    return this.notifications();
  }

  get unreadCount(): number {
    return this.notifications().filter(n => !n.isRead).length;
  }

  markAsRead(id: string): void {
    this.notifications.update(notifications =>
      notifications.map(n => {
        if (n.id === id) {
          return { ...n, isRead: true };
        }
        return n;
      })
    );
  }

  markAllAsRead(): void {
    this.notifications.update(notifications =>
      notifications.map(n => ({ ...n, isRead: true }))
    );
  }

  deleteNotification(id: string): void {
    this.notifications.update(notifications =>
      notifications.filter(n => n.id !== id)
    );
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

  formatTimestamp(date: Date): string {
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
