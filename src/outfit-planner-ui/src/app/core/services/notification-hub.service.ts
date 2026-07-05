import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { NotificationService } from './notification.service';
import { CookieService } from 'ngx-cookie-service';

@Injectable({ providedIn: 'root' })
export class NotificationHubService {
  private readonly notificationService = inject(NotificationService);
  private readonly cookieService = inject(CookieService);
  private hubConnection?: signalR.HubConnection;

  connect(): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;

    const token = this.cookieService.get('token');
    if (!token) {
      console.warn('No token available for SignalR connection');
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.resourceBaseUrl}/notifications/hub`, {
        accessTokenFactory: () => token,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: any) => {
      if (!notification) return;
      const notifId = notification?.id ?? notification?.Id;
      if (!notifId) return;

      console.log('[NotificationHub] Received notification:', notification);
      this.notificationService.notifications.update((notifs: any[]) => {
        const exists = notifs.some((n: any) => (n.id ?? n.Id) === notifId);
        if (exists) {
          console.log('[NotificationHub] Notification already exists, skipping:', notifId);
          return notifs;
        }
        console.log('[NotificationHub] Notification added to list:', notifId);
        return [notification, ...notifs];
      });
      // Recalculate unread count from the updated notifications list
      this.notificationService.updateUnreadCount();
    });

    this.hubConnection.onreconnecting(() => {
      console.warn('[NotificationHub] SignalR reconnecting...');
      this.notificationService.isLoading.set(true);
    });
    this.hubConnection.onreconnected(() => {
      console.log('[NotificationHub] SignalR reconnected');
      this.notificationService.isLoading.set(false);
    });
    this.hubConnection.onclose(() => {
      console.log('[NotificationHub] SignalR connection closed');
      this.notificationService.isLoading.set(false);
    });

    this.hubConnection.start().then(() => {
      console.log('[NotificationHub] SignalR connected successfully');
      this.notificationService.isLoading.set(false);
    }).catch((err: any) => {
      console.error('[NotificationHub] SignalR connection failed:', err);
      this.notificationService.isLoading.set(false);
    });
  }

  disconnect(): void {
    this.hubConnection?.stop().catch(() => {});
  }
}