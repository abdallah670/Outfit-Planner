import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Poll, PollOption, PollStatus } from '../../../domain/entities/poll.entity';

@Component({
  selector: 'app-poll-card',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './poll-card.component.html',
  styleUrls: ['./poll-card.component.scss'],
})
export class PollCardComponent {
  @Input() poll!: Poll;
  @Input() userVoteOptionId: string | null = null;
  @Output() vote = new EventEmitter<string>();

  selectedOptionId: string | null = null;

  // Expose Math to template for calculations
  protected readonly Math = Math;

  getStatusColor(status: PollStatus): string {
    switch (status) {
      case PollStatus.Active:
        return 'var(--primary)';
      case PollStatus.Expired:
        return 'var(--destructive)';
      case PollStatus.Closed:
        return 'var(--muted-foreground)';
      default:
        return 'var(--primary)';
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
    return this.poll.status === PollStatus.Active && !this.userVoteOptionId;
  }

  onOptionClick(optionId: string): void {
    if (this.canVote()) {
      this.vote.emit(optionId);
    }
  }

  getVotePercentage(option: PollOption): number {
    if (this.poll.totalVotes === 0) return 0;
    return Math.round((option.voteCount / this.poll.totalVotes) * 100);
  }
}
