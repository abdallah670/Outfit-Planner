import { Injectable, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class NotificationHubService {
  private store = inject(Store);
  private hubConnection?: signalR.HubConnection;

  connect(token: string): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiBaseUrl}/notifications/hub`, {
        accessTokenFactory: () => token,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: any) => {
      // Import notification actions dynamically to avoid circular deps
      import('../state/notifications/notifications.actions').then(m => {
        this.store.dispatch(m.NotificationActions.receiveNotification({ notification }));
      });
    });

    this.hubConnection.onreconnecting(() => console.warn('SignalR reconnecting...'));
    this.hubConnection.onreconnected(() => console.log('SignalR reconnected'));
    this.hubConnection.onclose(() => console.log('SignalR connection closed'));

    this.hubConnection.start().catch(err => console.error('SignalR connection failed:', err));
  }

  disconnect(): void {
    this.hubConnection?.stop().catch(() => {});
  }
}