import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { selectAdminLoading } from '../../../../core/state/admin/admin.selectors';
import { AdminUseCases } from '../../../../domain/usecases/admin.usecases';
import { AdminPollDto, ContentFilterRequest, BulkPollOperationRequest } from '../../../../domain/entities/admin.entity';

@Component({
  selector: 'app-admin-polls',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatCheckboxModule
  ],
  template: `
    <div class="admin-polls-container">
      <div class="page-header">
        <h2>Polls Management</h2>
        <div class="filters">
          <mat-form-field appearance="outline">
            <mat-label>Search</mat-label>
            <input matInput [(ngModel)]="searchFilter" placeholder="Search polls...">
          </mat-form-field>
          
          <mat-form-field appearance="outline">
            <mat-label>Status</mat-label>
            <mat-select [(ngModel)]="statusFilter">
              <mat-option value="">All Status</mat-option>
              <mat-option value="0">Active</mat-option>
              <mat-option value="1">Closed</mat-option>
              <mat-option value="2">Featured</mat-option>
              <mat-option value="3">Hidden</mat-option>
            </mat-select>
          </mat-form-field>
          
          <button mat-raised-button color="primary" (click)="applyFilter()">
            <mat-icon>search</mat-icon>
            Search
          </button>
          
          <button mat-button (click)="bulkFeature()" [disabled]="selectedPolls.size === 0">
            <mat-icon>star</mat-icon>
            Feature Selected
          </button>
          
          <button mat-button (click)="bulkClose()" [disabled]="selectedPolls.size === 0">
            <mat-icon>lock</mat-icon>
            Close Selected
          </button>
          
          <button mat-button color="accent" (click)="bulkDelete()" [disabled]="selectedPolls.size === 0">
            <mat-icon>delete</mat-icon>
            Delete Selected
          </button>
        </div>
      </div>

      <div class="table-container">
        @if (loading$ | async) {
          <div class="loading-container">
            <mat-spinner diameter="50"></mat-spinner>
            <p>Loading polls...</p>
          </div>
        } @else {
          <table mat-table [dataSource]="polls" class="polls-table">
            <!-- Checkbox Column -->
            <ng-container matColumnDef="select">
              <th mat-header-cell *matHeaderCellDef>
                <mat-checkbox (change)="toggleAllPolls()" [checked]="isAllSelected()"></mat-checkbox>
              </th>
              <td mat-cell *matCellDef="let poll">
                <mat-checkbox (change)="togglePollSelection(poll.id)" [checked]="selectedPolls.has(poll.id)"></mat-checkbox>
              </td>
            </ng-container>

            <!-- ID Column -->
            <ng-container matColumnDef="id">
              <th mat-header-cell *matHeaderCellDef> ID </th>
              <td mat-cell *matCellDef="let poll"> {{poll.id.substring(0, 8)}}... </td>
            </ng-container>

            <!-- Question Column -->
            <ng-container matColumnDef="question">
              <th mat-header-cell *matHeaderCellDef> Question </th>
              <td mat-cell *matCellDef="let poll"> 
                <div class="poll-question">
                  <strong>{{poll.question}}</strong>
                  <div class="poll-options">{{poll.options.length}} options</div>
                </div>
              </td>
            </ng-container>

            <!-- Author Column -->
            <ng-container matColumnDef="userName">
              <th mat-header-cell *matHeaderCellDef> Author </th>
              <td mat-cell *matCellDef="let poll"> {{poll.userName}} </td>
            </ng-container>

            <!-- Status Column -->
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef> Status </th>
              <td mat-cell *matCellDef="let poll">
                <span class="status-badge" [class]="getStatusClass(poll.status)">
                  {{getStatusText(poll.status)}}
                </span>
                @if (poll.isFeatured) {
                  <mat-icon class="featured-icon" matTooltip="Featured">star</mat-icon>
                }
              </td>
            </ng-container>

            <!-- Votes Column -->
            <ng-container matColumnDef="votes">
              <th mat-header-cell *matHeaderCellDef> Votes </th>
              <td mat-cell *matCellDef="let poll">
                <div class="vote-stats">
                  <span class="total-votes">{{poll.totalVotes}}</span>
                  <div class="vote-distribution">
                    @for (vote of poll.optionVotes; track $index) {
                      <span class="vote-bar" [style.width.%]="getVotePercentage(vote, poll.totalVotes)"></span>
                    }
                  </div>
                </div>
              </td>
            </ng-container>

            <!-- Date Column -->
            <ng-container matColumnDef="createdAt">
              <th mat-header-cell *matHeaderCellDef> Created </th>
              <td mat-cell *matCellDef="let poll"> {{poll.createdAt | date:'medium'}} </td>
            </ng-container>

            <!-- Actions Column -->
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef> Actions </th>
              <td mat-cell *matCellDef="let poll">
                <button mat-icon-button (click)="featurePoll(poll.id)" [disabled]="poll.isFeatured" 
                        matTooltip="Feature">
                  <mat-icon>star</mat-icon>
                </button>
                <button mat-icon-button (click)="unfeaturePoll(poll.id)" [disabled]="!poll.isFeatured"
                        matTooltip="Unfeature">
                  <mat-icon>star_border</mat-icon>
                </button>
                <button mat-icon-button (click)="closePoll(poll.id)" [disabled]="poll.status === 1"
                        matTooltip="Close">
                  <mat-icon>lock</mat-icon>
                </button>
                <button mat-icon-button (click)="deletePoll(poll.id)" color="warn"
                        matTooltip="Delete">
                  <mat-icon>delete</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>

          <mat-paginator
            [length]="totalPolls"
            [pageSize]="pageSize"
            [pageSizeOptions]="[10, 25, 50, 100]"
            (page)="onPageChange($event)"
            showFirstLastButtons>
          </mat-paginator>
        }
      </div>
    </div>
  `,
  styles: [`
    .admin-polls-container {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }
    
    .page-header {
      margin-bottom: 24px;
    }
    
    .page-header h2 {
      margin: 0 0 16px 0;
      color: #333;
    }
    
    .filters {
      display: flex;
      gap: 16px;
      flex-wrap: wrap;
      align-items: end;
    }
    
    .filters mat-form-field {
      min-width: 200px;
    }
    
    .table-container {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      overflow: hidden;
    }
    
    .loading-container {
      display: flex;
      justify-content: center;
      align-items: center;
      height: 200px;
      flex-direction: column;
      gap: 16px;
    }
    
    .polls-table {
      width: 100%;
    }
    
    .poll-question {
      max-width: 300px;
    }
    
    .poll-options {
      font-size: 12px;
      color: #666;
      margin-top: 4px;
    }
    
    .vote-stats {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    
    .total-votes {
      font-weight: 500;
      font-size: 14px;
    }
    
    .vote-distribution {
      display: flex;
      gap: 2px;
      height: 8px;
    }
    
    .vote-bar {
      background: #1976d2;
      min-width: 2px;
      border-radius: 1px;
    }
    
    .status-badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
      display: inline-flex;
      align-items: center;
      gap: 4px;
    }
    
    .status-badge.active {
      background: #e8f5e8;
      color: #2e7d32;
    }
    
    .status-badge.closed {
      background: #ffebee;
      color: #c62828;
    }
    
    .status-badge.featured {
      background: #fff3e0;
      color: #f57c00;
    }
    
    .status-badge.hidden {
      background: #f5f5f5;
      color: #666;
    }
    
    .featured-icon {
      font-size: 14px;
      color: #ffc107;
    }
  `]
})
export class AdminPollsComponent implements OnInit {
  private readonly adminUseCases = inject(AdminUseCases);
  private readonly snackBar = inject(MatSnackBar);
  private readonly store = inject(Store);

  polls: AdminPollDto[] = [];
  totalPolls = 0;
  pageSize = 25;
  currentPage = 1;
  loading$: Observable<boolean> = this.store.select(selectAdminLoading);

  displayedColumns: string[] = ['select', 'id', 'question', 'userName', 'status', 'votes', 'createdAt', 'actions'];
  selectedPolls = new Set<string>();

  searchFilter = '';
  statusFilter = '';

  ngOnInit(): void {
    this.loadPolls();
  }

  loadPolls(): void {
    const filter: ContentFilterRequest = {
      search: this.searchFilter || undefined,
      status: this.statusFilter || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.adminUseCases.getPolls(filter).subscribe({
      next: (response) => {
        this.polls = response.data;
        this.totalPolls = response.total;
      },
      error: (error) => {
        this.snackBar.open('Failed to load polls', 'Close', { duration: 3000 });
      }
    });
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadPolls();
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadPolls();
  }

  togglePollSelection(pollId: string): void {
    if (this.selectedPolls.has(pollId)) {
      this.selectedPolls.delete(pollId);
    } else {
      this.selectedPolls.add(pollId);
    }
  }

  toggleAllPolls(): void {
    if (this.isAllSelected()) {
      this.selectedPolls.clear();
    } else {
      this.polls.forEach(poll => this.selectedPolls.add(poll.id));
    }
  }

  isAllSelected(): boolean {
    return this.polls.length > 0 && this.selectedPolls.size === this.polls.length;
  }

  featurePoll(pollId: string): void {
    this.adminUseCases.featurePoll(pollId).subscribe({
      next: () => {
        this.snackBar.open('Poll featured successfully', 'Close', { duration: 3000 });
        this.loadPolls();
      },
      error: (error) => {
        this.snackBar.open('Failed to feature poll', 'Close', { duration: 3000 });
      }
    });
  }

  unfeaturePoll(pollId: string): void {
    this.adminUseCases.unfeaturePoll(pollId).subscribe({
      next: () => {
        this.snackBar.open('Poll unfeatured successfully', 'Close', { duration: 3000 });
        this.loadPolls();
      },
      error: (error) => {
        this.snackBar.open('Failed to unfeature poll', 'Close', { duration: 3000 });
      }
    });
  }

  closePoll(pollId: string): void {
    this.adminUseCases.closePoll(pollId).subscribe({
      next: () => {
        this.snackBar.open('Poll closed successfully', 'Close', { duration: 3000 });
        this.loadPolls();
      },
      error: (error) => {
        this.snackBar.open('Failed to close poll', 'Close', { duration: 3000 });
      }
    });
  }

  deletePoll(pollId: string): void {
    if (confirm('Are you sure you want to delete this poll?')) {
      this.adminUseCases.deletePoll(pollId).subscribe({
        next: () => {
          this.snackBar.open('Poll deleted successfully', 'Close', { duration: 3000 });
          this.loadPolls();
        },
        error: (error) => {
          this.snackBar.open('Failed to delete poll', 'Close', { duration: 3000 });
        }
      });
    }
  }

  bulkFeature(): void {
    const operations = Array.from(this.selectedPolls).map(pollId => ({
      pollId,
      type: 'feature'
    }));

    this.adminUseCases.bulkPollOperations({ operations }).subscribe({
      next: (response) => {
        this.snackBar.open(`Featured ${response.successfulOperations} polls`, 'Close', { duration: 3000 });
        this.selectedPolls.clear();
        this.loadPolls();
      },
      error: (error) => {
        this.snackBar.open('Failed to feature polls', 'Close', { duration: 3000 });
      }
    });
  }

  bulkClose(): void {
    const operations = Array.from(this.selectedPolls).map(pollId => ({
      pollId,
      type: 'close',
      reason: 'Bulk closed by admin'
    }));

    this.adminUseCases.bulkPollOperations({ operations }).subscribe({
      next: (response) => {
        this.snackBar.open(`Closed ${response.successfulOperations} polls`, 'Close', { duration: 3000 });
        this.selectedPolls.clear();
        this.loadPolls();
      },
      error: (error) => {
        this.snackBar.open('Failed to close polls', 'Close', { duration: 3000 });
      }
    });
  }

  bulkDelete(): void {
    if (confirm(`Are you sure you want to delete ${this.selectedPolls.size} polls?`)) {
      const operations = Array.from(this.selectedPolls).map(pollId => ({
        pollId,
        type: 'delete'
      }));

      this.adminUseCases.bulkPollOperations({ operations }).subscribe({
        next: (response) => {
          this.snackBar.open(`Deleted ${response.successfulOperations} polls`, 'Close', { duration: 3000 });
          this.selectedPolls.clear();
          this.loadPolls();
        },
        error: (error) => {
          this.snackBar.open('Failed to delete polls', 'Close', { duration: 3000 });
        }
      });
    }
  }

  getStatusText(status: number): string {
    switch (status) {
      case 0: return 'Active';
      case 1: return 'Closed';
      case 2: return 'Featured';
      case 3: return 'Hidden';
      default: return 'Unknown';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 0: return 'active';
      case 1: return 'closed';
      case 2: return 'featured';
      case 3: return 'hidden';
      default: return '';
    }
  }

  getVotePercentage(votes: number, totalVotes: number): number {
    return totalVotes > 0 ? (votes / totalVotes) * 100 : 0;
  }
}
