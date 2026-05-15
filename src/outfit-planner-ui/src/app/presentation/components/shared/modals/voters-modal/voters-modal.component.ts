import { Component, Input, OnDestroy, OnInit, inject, ChangeDetectorRef, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../../../../core/services/auth.service';
import { FeedUseCases } from '../../../../../domain/usecases/feed.usecases';
import Swal from 'sweetalert2';
import { VoterInfo } from '../../../../../domain/entities/poll.entity';

@Component({
  selector: 'app-voters-modal',
  standalone: true,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [
    CommonModule,
  ],
  templateUrl: './voters-modal.component.html',
  styleUrls: ['./voters-modal.component.scss']
})
export class VotersModalComponent implements OnInit, OnDestroy {
  @Input() pollId!: string;
  @Input() optionId?: string;

  voters: VoterInfo[] = [];
  loading = false;
  error: string | null = null;

  private feedUseCases = inject(FeedUseCases);
  private authService = inject(AuthService);
  private router = inject(Router);
  private cdRef = inject(ChangeDetectorRef);
  private subscriptions = new Subscription();

  ngOnInit(): void {
    this.loadVoters();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadVoters(): void {
    this.loading = true;
    this.error = null;

    this.subscriptions.add(
      this.feedUseCases.getVotersForPoll(this.pollId, this.optionId).subscribe({
        next: (voters: VoterInfo[]) => {
          this.voters = voters;
          this.loading = false;
          this.cdRef.detectChanges();
        },
        error: (err: any) => {
          console.error('Failed to load voters', err);
          this.error = 'Failed to load voters';
          this.loading = false;
        }
      })
    );
  }

  getCurrentUserId(): string | null {
    return this.authService.currentUser()?.id ?? null;
  }

  isOwnVote(voterId: string): boolean {
    const userId = this.getCurrentUserId();
    return userId !== null && userId === voterId;
  }

  getOptionDisplayText(voter: VoterInfo): string {
    // Prefer description if available, otherwise use label (A, B, C...)
    if (voter.optionDescription && voter.optionDescription.trim()) {
      return voter.optionDescription;
    }
    const labels = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const label = labels[voter.optionDisplayOrder] || `Option ${voter.optionDisplayOrder + 1}`;
    return `Option ${label}`;
  }

  formatTimeAgo(date: Date): string {
    const now = new Date();
    const diff = now.getTime() - new Date(date).getTime();
    const mins = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (mins < 1) return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    if (hours < 24) return `${hours}h ago`;
    if (days < 7) return `${days}d ago`;
    return new Date(date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  goToUserProfile(userId: string): void {
    Swal.close();
    this.router.navigate(['/profile', userId]);
  }

  closeModal(): void {
    Swal.close();
  }
}
