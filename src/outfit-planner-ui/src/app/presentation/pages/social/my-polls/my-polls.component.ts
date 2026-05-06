import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import Swal from 'sweetalert2';
import { PollsActions } from '../../../../core/state/polls/polls.actions';
import { selectUserPolls, selectUserPollsLoading } from '../../../../core/state/polls/polls.selectors';
import { Poll, PollStatus } from '../../../../domain/entities/poll.entity';

@Component({
  selector: 'app-my-polls',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatMenuModule,
    MatTooltipModule,
  ],
  templateUrl: './my-polls.component.html',
  styleUrls: ['./my-polls.component.scss'],
})
export class MyPollsComponent implements OnInit {
  private store = inject(Store);

  userPolls$ = this.store.select(selectUserPolls);
  loading$ = this.store.select(selectUserPollsLoading);

  ngOnInit(): void {
    this.store.dispatch(PollsActions.loadUserPolls());
  }

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

  canEdit(poll: Poll): boolean {
    return poll.status === PollStatus.Active;
  }

  canClose(poll: Poll): boolean {
    return poll.status === PollStatus.Active;
  }

  onDeletePoll(pollId: string): void {
    Swal.fire({
      title: 'Delete Poll?',
      text: 'This action cannot be undone.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      confirmButtonColor: '#d33',
    }).then((result) => {
      if (result.isConfirmed) {
        this.store.dispatch(PollsActions.deletePoll({ pollId }));
      }
    });
  }

  onClosePoll(pollId: string): void {
    Swal.fire({
      title: 'Close Poll?',
      text: 'No new votes will be accepted.',
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Close',
      cancelButtonText: 'Cancel',
    }).then((result) => {
      if (result.isConfirmed) {
        this.store.dispatch(PollsActions.closePoll({ pollId }));
      }
    });
  }

  trackByPollId(index: number, poll: Poll): string {
    return poll.id;
  }
}
