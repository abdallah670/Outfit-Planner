import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { CookieService } from 'ngx-cookie-service';
import { Subject } from 'rxjs';

export interface SocialPostDto {
  id: string;
  userId: string;
  userName: string;
  userAvatarUrl?: string;
  caption?: string;
  imageUrl?: string;
  createdAt: string;
  likesCount: number;
  commentsCount: number;
  postType: 'Outfit' | 'Poll';
  outfitId?: string;
  pollId?: string;
}

@Injectable({ providedIn: 'root' })
export class SocialHubService {
  private readonly cookieService = inject(CookieService);
  private hubConnection?: signalR.HubConnection;

  // Observable streams for components to subscribe to
  private newPostSubject = new Subject<SocialPostDto>();
  private commentUpdateSubject = new Subject<{ postId: string; count: number }>();
  private reactionUpdateSubject = new Subject<{ postId: string; count: number }>();
  private pollVoteUpdateSubject = new Subject<{ postId: string; totalVotes: number; optionVotes: { [optionId: string]: number } }>();

  // Public observables
  newPost$ = this.newPostSubject.asObservable();
  commentUpdate$ = this.commentUpdateSubject.asObservable();
  reactionUpdate$ = this.reactionUpdateSubject.asObservable();
  pollVoteUpdate$ = this.pollVoteUpdateSubject.asObservable();

  connect(): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;

    const token = this.cookieService.get('token');
    if (!token) {
      console.warn('[SocialHub] No token available for SignalR connection');
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.resourceBaseUrl}/social/hub`, {
        accessTokenFactory: () => token,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .build();

    // Register event handlers
    this.hubConnection.on('NewPost', (post: SocialPostDto) => {
      console.log('[SocialHub] Received new post:', post);
      this.newPostSubject.next(post);
    });

    this.hubConnection.on('CommentUpdate', (postId: string, count: number) => {
      console.log('[SocialHub] Comment update for post', postId, ':', count);
      this.commentUpdateSubject.next({ postId, count });
    });

    this.hubConnection.on('ReactionUpdate', (postId: string, count: number) => {
      console.log('[SocialHub] Reaction update for post', postId, ':', count);
      this.reactionUpdateSubject.next({ postId, count });
    });

    this.hubConnection.on('PollVoteUpdate', (postId: string, totalVotes: number, optionVotes: { [optionId: string]: number }) => {
      console.log('[SocialHub] Poll vote update for post', postId, '- totalVotes:', totalVotes, 'optionVotes:', optionVotes);
      this.pollVoteUpdateSubject.next({ postId, totalVotes, optionVotes });
    });

    // Connection lifecycle handlers
    this.hubConnection.onreconnecting(() => {
      console.warn('[SocialHub] SignalR reconnecting...');
    });
    this.hubConnection.onreconnected(() => {
      console.log('[SocialHub] SignalR reconnected');
      this.hubConnection?.invoke('JoinFeed').catch(err => 
        console.error('[SocialHub] Failed to rejoin feed:', err)
      );
    });
    this.hubConnection.onclose(() => {
      console.log('[SocialHub] SignalR connection closed');
    });

    // Start connection
    this.hubConnection.start().then(() => {
      console.log('[SocialHub] SignalR connected successfully');
      this.hubConnection?.invoke('JoinFeed').catch(err => 
        console.error('[SocialHub] Failed to join feed:', err)
      );
    }).catch((err: any) => {
      console.error('[SocialHub] SignalR connection failed:', err);
    });
  }

  disconnect(): void {
    this.hubConnection?.stop().catch(() => {});
  }

  isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }
}
