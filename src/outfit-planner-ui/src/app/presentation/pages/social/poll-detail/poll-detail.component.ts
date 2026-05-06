import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';
import { PollsActions } from '../../../../core/state/polls/polls.actions';
import { selectSelectedPoll, selectPollsLoading } from '../../../../core/state/polls/polls.selectors';
import { Poll, PollOption, CastVoteRequest, PollStatus } from '../../../../domain/entities/poll.entity';

@Component({
  selector: 'app-poll-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatProgressBarModule,
  ],
  template: `
    <div class="poll-detail-page" *ngIf="poll() as p">
      <header class="detail-header">
        <button mat-icon-button routerLink="/social/feed">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Poll Details</h1>
      </header>

      <mat-card class="main-card">
        <mat-card-header>
          <mat-card-title>{{ p.question }}</mat-card-title>
          <mat-card-subtitle>
            {{ p.totalVotes }} total votes • Expires {{ p.expiresAt | date:'medium' }}
          </mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <div class="options-grid">
            <div class="option-item" *ngFor="let option of p.options; let i = index">
              <div class="option-image">
                <img [src]="option.outfitThumbnail || 'assets/placeholder.jpg'" alt="Option Image">
                <span class="option-letter">{{ getLetter(i) }}</span>
              </div>
              <div class="option-info">
                <p class="description">{{ option.description }}</p>
                
                <!-- Progress Bar for results (shown if expired or voted) -->
                <div class="results-container" *ngIf="p.status !== PollStatus.Active || hasVoted()">
                  <div class="progress-wrapper">
                    <mat-progress-bar 
                      mode="determinate" 
                      [value]="getPercentage(p, option.id)"
                      [color]="isWinner(p, option.id) ? 'accent' : 'primary'">
                    </mat-progress-bar>
                    <span class="percentage">{{ getPercentage(p, option.id) }}%</span>
                  </div>
                  <span class="votes-text">{{ option.voteCount }} votes</span>
                </div>

                <button 
                  mat-raised-button 
                  color="primary" 
                  *ngIf="p.status === PollStatus.Active && !hasVoted()"
                  (click)="vote(p.id, option.id)">
                  Vote {{ getLetter(i) }}
                </button>
              </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>

    <div class="loading-overlay" *ngIf="loading()">
      <mat-icon class="spin">refresh</mat-icon>
      <p>Loading poll details...</p>
    </div>
  `,
  styles: [`
    .poll-detail-page {
      max-width: 900px;
      margin: 2rem auto;
      padding: 0 1rem;
    }
    .detail-header {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 2rem;
    }
    .detail-header h1 {
      margin: 0;
      font-size: 1.5rem;
      font-weight: 600;
    }
    .main-card {
      border-radius: 16px;
      overflow: hidden;
    }
    .options-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 2rem;
      margin-top: 2rem;
    }
    .option-item {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      background: #f9f9f9;
      padding: 1rem;
      border-radius: 12px;
      border: 1px solid #eee;
    }
    .option-image {
      position: relative;
      height: 300px;
      border-radius: 8px;
      overflow: hidden;
    }
    .option-image img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    .option-letter {
      position: absolute;
      top: 1rem;
      left: 1rem;
      background: rgba(0,0,0,0.7);
      color: white;
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 8px;
      font-weight: bold;
    }
    .option-info {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .description {
      margin: 0;
      font-weight: 500;
    }
    .results-container {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .progress-wrapper {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .percentage {
      font-weight: bold;
      min-width: 40px;
    }
    .votes-text {
      font-size: 0.8rem;
      color: #777;
    }
    .loading-overlay {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 60vh;
    }
    .spin {
      animation: spin 1s linear infinite;
      font-size: 48px;
      width: 48px;
      height: 48px;
    }
    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }
  `],
})
export class PollDetailComponent implements OnInit {
  private store = inject(Store);
  private route = inject(ActivatedRoute);

  poll = toSignal(this.store.select(selectSelectedPoll)) as () => Poll | null;
  loading = toSignal(this.store.select(selectPollsLoading), { initialValue: false });

  PollStatus = PollStatus;
  hasVoted = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.store.dispatch(PollsActions.loadPollById({ id }));
    }
  }

  getLetter(index: number): string {
    return String.fromCharCode(65 + index);
  }

  vote(pollId: string, optionId: string): void {
    const request: CastVoteRequest = {
      optionId,
      rating: 5,
      isAnonymous: false
    };
    this.store.dispatch(PollsActions.vote({ pollId, request }));
    this.hasVoted.set(true);
  }

  getPercentage(poll: Poll, optionId: string): number {
    if (poll.totalVotes === 0) return 0;
    const option = poll.options.find((o: PollOption) => o.id === optionId);
    return option ? Math.round((option.voteCount / poll.totalVotes) * 100) : 0;
  }

  isWinner(poll: Poll, optionId: string): boolean {
    if (poll.totalVotes === 0) return false;
    const maxVotes = Math.max(...poll.options.map((o: PollOption) => o.voteCount));
    const optionSource = poll.options.find((o: PollOption) => o.id === optionId);
    return optionSource ? optionSource.voteCount === maxVotes : false;
  }
}
