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
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { selectReports, selectTotalReports, selectAdminLoading } from '../../../../core/state/admin/admin.selectors';
import { loadReports, resolveReport } from '../../../../core/state/admin/admin.actions';
import { ContentReport, ReportFilterRequest } from '../../../../domain/entities/admin.entity';

@Component({
  selector: 'app-admin-reports',
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
    MatChipsModule
  ],
  template: `
    <div class="admin-reports-container">
      <div class="page-header">
        <h2>Content Reports</h2>
        <div class="filters">
          <mat-form-field appearance="outline">
            <mat-label>Status</mat-label>
            <mat-select [(ngModel)]="selectedStatus" (selectionChange)="applyFilter()">
              <mat-option value="">All Status</mat-option>
              <mat-option value="Pending">Pending</mat-option>
              <mat-option value="InReview">In Review</mat-option>
              <mat-option value="Resolved">Resolved</mat-option>
              <mat-option value="Dismissed">Dismissed</mat-option>
            </mat-select>
          </mat-form-field>
          
          <mat-form-field appearance="outline">
            <mat-label>Content Type</mat-label>
            <mat-select [(ngModel)]="selectedContentType" (selectionChange)="applyFilter()">
              <mat-option value="">All Types</mat-option>
              <mat-option value="FeedPost">Feed Post</mat-option>
              <mat-option value="Poll">Poll</mat-option>
              <mat-option value="Comment">Comment</mat-option>
              <mat-option value="User">User</mat-option>
            </mat-select>
          </mat-form-field>
          
          <button mat-raised-button color="primary" (click)="applyFilter()">
            <mat-icon>search</mat-icon>
            Search
          </button>
        </div>
      </div>

      <div class="table-container">
        @if (loading$ | async) {
          <div class="loading-container">
            <mat-spinner diameter="50"></mat-spinner>
            <p>Loading reports...</p>
          </div>
        } @else {
          <table mat-table [dataSource]="(reports$ | async) || []">
            <!-- ID Column -->
            <ng-container matColumnDef="id">
              <th mat-header-cell *matHeaderCellDef> ID </th>
              <td mat-cell *matCellDef="let report"> {{report.id.substring(0, 8)}}... </td>
            </ng-container>

            <!-- Reporter Column -->
            <ng-container matColumnDef="reporterUserName">
              <th mat-header-cell *matHeaderCellDef> Reporter </th>
              <td mat-cell *matCellDef="let report"> {{report.reporterUserName || 'Anonymous'}} </td>
            </ng-container>

            <!-- Content Type Column -->
            <ng-container matColumnDef="contentType">
              <th mat-header-cell *matHeaderCellDef> Type </th>
              <td mat-cell *matCellDef="let report">
                <mat-chip [color]="getContentTypeColor(report.contentType)">
                  {{report.contentType}}
                </mat-chip>
              </td>
            </ng-container>

            <!-- Reason Column -->
            <ng-container matColumnDef="reason">
              <th mat-header-cell *matHeaderCellDef> Reason </th>
              <td mat-cell *matCellDef="let report">
                <mat-chip [color]="getReasonColor(report.reason)">
                  {{formatReason(report.reason)}}
                </mat-chip>
              </td>
            </ng-container>

            <!-- Status Column -->
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef> Status </th>
              <td mat-cell *matCellDef="let report">
                <span class="status-badge" [class]="report.status.toLowerCase()">
                  {{report.status}}
                </span>
              </td>
            </ng-container>

            <!-- Created At Column -->
            <ng-container matColumnDef="createdAt">
              <th mat-header-cell *matHeaderCellDef> Created </th>
              <td mat-cell *matCellDef="let report"> 
                <span [class.old-report]="isOldReport(report.createdAt)">
                  {{report.createdAt | date:'short'}}
                </span>
              </td>
            </ng-container>

            <!-- Actions Column -->
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef> Actions </th>
              <td mat-cell *matCellDef="let report">
                <button mat-icon-button (click)="viewReportDetail(report.id)" matTooltip="View Details">
                  <mat-icon>visibility</mat-icon>
                </button>
                
                @if (report.status === 'Pending') {
                  <button mat-icon-button (click)="resolveReportAction(report)" matTooltip="Resolve Report" color="primary">
                    <mat-icon>check_circle</mat-icon>
                  </button>
                }
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>

          <mat-paginator
            [length]="totalReports$ | async"
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
    .admin-reports-container {
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
    
    table {
      width: 100%;
    }
    
    th.mat-header-cell {
      font-weight: 600;
      color: #333;
    }
    
    td.mat-cell {
      border-bottom: 1px solid #e0e0e0;
    }
    
    .status-badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
    }
    
    .status-badge.pending {
      background: #fff3e0;
      color: #f57c00;
    }
    
    .status-badge.inreview {
      background: #e3f2fd;
      color: #1976d2;
    }
    
    .status-badge.resolved {
      background: #e8f5e8;
      color: #2e7d32;
    }
    
    .status-badge.dismissed {
      background: #fafafa;
      color: #757575;
    }
    
    .old-report {
      color: #f44336;
      font-weight: 500;
    }
  `]
})
export class AdminReportsComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);

  reports$: Observable<ContentReport[]> = this.store.select(selectReports);
  totalReports$: Observable<number> = this.store.select(selectTotalReports);
  loading$: Observable<boolean> = this.store.select(selectAdminLoading);

  displayedColumns: string[] = ['id', 'reporterUserName', 'contentType', 'reason', 'status', 'createdAt', 'actions'];
  pageSize = 25;
  currentPage = 1;
  selectedStatus = '';
  selectedContentType = '';

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    const filter: ReportFilterRequest = {
      status: this.selectedStatus || undefined,
      contentType: this.selectedContentType || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };
    this.store.dispatch(loadReports({ filter }));
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadReports();
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadReports();
  }

  viewReportDetail(reportId: string): void {
    // Navigate to report detail page
    // this.router.navigate(['/admin/reports', reportId]);
  }

  resolveReportAction(report: ContentReport): void {
    const resolution = prompt('Enter resolution:');
    if (resolution) {
      this.store.dispatch(resolveReport({ 
        reportId: report.id, 
        resolution, 
        takeAction: confirm('Take action on reported content?') 
      }));
      this.snackBar.open('Report resolved successfully', 'Close', { duration: 3000 });
    }
  }

  getContentTypeColor(contentType: string): string {
    const colors: { [key: string]: string } = {
      'FeedPost': 'primary',
      'Poll': 'accent',
      'Comment': 'warn',
      'User': 'primary'
    };
    return colors[contentType] || 'primary';
  }

  getReasonColor(reason: string): string {
    const colors: { [key: string]: string } = {
      'Spam': 'warn',
      'Harassment': 'accent',
      'InappropriateContent': 'primary',
      'FakeAccount': 'warn',
      'Other': 'primary'
    };
    return colors[reason] || 'primary';
  }

  formatReason(reason: string): string {
    return reason.replace(/([A-Z])/g, ' $1').trim();
  }

  isOldReport(createdAt: string): boolean {
    const reportDate = new Date(createdAt);
    const threeDaysAgo = new Date();
    threeDaysAgo.setDate(threeDaysAgo.getDate() - 3);
    return reportDate < threeDaysAgo;
  }
}
