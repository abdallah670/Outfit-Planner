import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Notification {
  id: string;
  userId: string;
  type: 'social' | 'weather' | 'reminder' | 'system';
  title: string;
  message: string;
  isRead: boolean;
  actionUrl?: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.baseUrl}/notifications`;

  // Signal for notifications
  notifications = signal<Notification[]>([]);
  
  // Signal for unread count
  unreadCount = signal<number>(0);

  // Signal for loading state
  isLoading = signal<boolean>(false);

  // Signal for error
  error = signal<string | null>(null);

  /**
   * Get all notifications for the current user
   */
  getNotifications(): Observable<Notification[]> {
    this.isLoading.set(true);
    this.error.set(null);

    return this.http.get<Notification[]>(`${this.apiUrl}`).pipe(
      tap((notifications) => {
        this.notifications.set(notifications);
        this.updateUnreadCount();
        this.isLoading.set(false);
      }),
      catchError((err) => {
        console.error('Error fetching notifications:', err);
        this.error.set('Failed to load notifications');
        this.isLoading.set(false);
        return of([]);
      })
    );
  }

  /**
   * Get unread notification count
   */
  getUnreadCount(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/unread-count`).pipe(
      tap((count) => {
        this.unreadCount.set(count);
      }),
      catchError((err) => {
        console.error('Error fetching unread count:', err);
        return of(0);
      })
    );
  }

  /**
   * Mark a notification as read
   */
  markAsRead(notificationId: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${notificationId}/read`, {}).pipe(
      tap(() => {
        this.notifications.update((notifications) =>
          notifications.map((n) =>
            n.id === notificationId ? { ...n, isRead: true } : n
          )
        );
        this.updateUnreadCount();
      }),
      catchError((err) => {
        console.error('Error marking notification as read:', err);
        throw err;
      })
    );
  }

  /**
   * Mark all notifications as read
   */
  markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => {
        this.notifications.update((notifications) =>
          notifications.map((n) => ({ ...n, isRead: true }))
        );
        this.unreadCount.set(0);
      }),
      catchError((err) => {
        console.error('Error marking all notifications as read:', err);
        throw err;
      })
    );
  }

  /**
   * Delete a notification
   */
  deleteNotification(notificationId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${notificationId}`).pipe(
      tap(() => {
        const deletedNotification = this.notifications().find(
          (n) => n.id === notificationId
        );
        this.notifications.update((notifications) =>
          notifications.filter((n) => n.id !== notificationId)
        );
        // Update unread count if the deleted notification was unread
        if (deletedNotification && !deletedNotification.isRead) {
          this.updateUnreadCount();
        }
      }),
      catchError((err) => {
        console.error('Error deleting notification:', err);
        throw err;
      })
    );
  }

  /**
   * Update unread count from current notifications
   */
  private updateUnreadCount(): void {
    const count = this.notifications().filter((n) => !n.isRead).length;
    this.unreadCount.set(count);
  }

  /**
   * Refresh notifications from the server
   */
  refresh(): void {
    this.getNotifications().subscribe();
  }
}
