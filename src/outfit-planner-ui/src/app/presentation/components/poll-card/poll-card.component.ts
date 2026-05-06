import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatRadioModule } from '@angular/material/radio';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { Poll, PollOption, PollStatus } from '../../../domain/entities/poll.entity';

@Component({
  selector: 'app-poll-card',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatRadioModule,
    MatProgressBarModule,
    MatTooltipModule,
    FormsModule,
  ],
  templateUrl: './poll-card.component.html',
  styleUrls: ['./poll-card.component.scss'],
})
export class PollCardComponent {
  @Input() poll!: Poll;
  @Input() showResults = false;
  @Input() userVoteOptionId: string | null = null;
  @Output() vote = new EventEmitter<string>();
  @Output() viewDetails = new EventEmitter<string>();

  selectedOptionId: string | null = null;

  getStatusColor(status: PollStatus): string {
    switch (status) {
      case PollStatus.Active:
        return 'primary';
      case PollStatus.Expired:
        return 'warn';
      case PollStatus.Closed:
        return 'accent';
      default:
        return 'primary';
    }
  }

  getStatusLabel(status: PollStatus): string {
    switch (status) {
      case PollStatus.Active:
        return 'Active';
      case PollStatus.Expired:
        return 'Expired';
      case PollStatus.Closed:
        return 'Closed';
      default:
        return 'Active';
    }
  }

  canVote(): boolean {
    return this.poll.status === PollStatus.Active && !this.showResults;
  }

  onVote(): void {
    if (this.selectedOptionId) {
      this.vote.emit(this.selectedOptionId);
      this.showResults = true;
    }
  }

  onViewDetails(): void {
    this.viewDetails.emit(this.poll.id);
  }

  getVotePercentage(option: PollOption): number {
    if (this.poll.totalVotes === 0) return 0;
    return Math.round((option.voteCount / this.poll.totalVotes) * 100);
  }

  getTimeRemaining(): string {
    if (!this.poll.expiresAt) return 'No expiration';
    
    const now = new Date();
    const expires = new Date(this.poll.expiresAt);
    
    if (expires < now) {
      return 'Expired';
    }
    
    const diffMs = expires.getTime() - now.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    const diffHours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    
    if (diffDays > 0) {
      return `${diffDays}d ${diffHours}h remaining`;
    } else if (diffHours > 0) {
      return `${diffHours}h remaining`;
    } else {
      return 'Less than 1h remaining';
    }
  }
}
