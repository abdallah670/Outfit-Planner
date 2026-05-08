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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { selectAdminLoading } from '../../../../core/state/admin/admin.selectors';
import { AdminUseCases } from '../../../../domain/usecases/admin.usecases';
import { AuditLogEntry } from '../../../../domain/entities/admin.entity';

@Component({
  selector: 'app-admin-audit-logs',
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
    MatDatepickerModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './admin-audit-logs.component.html',
  styleUrls: ['./admin-audit-logs.component.scss']
})
export class AdminAuditLogsComponent implements OnInit {
  private readonly adminUseCases = inject(AdminUseCases);
  private readonly snackBar = inject(MatSnackBar);
  private readonly store = inject(Store);

  auditLogs: AuditLogEntry[] = [];
  totalAuditLogs = 0;
  pageSize = 25;
  currentPage = 1;
  loading$: Observable<boolean> = this.store.select(selectAdminLoading);

  displayedColumns: string[] = ['id', 'userName', 'action', 'entityType', 'timestamp'];
  userIdFilter = '';
  actionFilter = '';
  startDate: Date | null = null;
  endDate: Date | null = null;

  ngOnInit(): void {
    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    const filter = {
      userId: this.userIdFilter || undefined,
      action: this.actionFilter || undefined,
      startDate: this.startDate?.toISOString() || undefined,
      endDate: this.endDate?.toISOString() || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.adminUseCases.getAuditLogs(filter).subscribe({
      next: (response) => {
        this.auditLogs = response.data;
        this.totalAuditLogs = response.total;
      },
      error: (error) => {
        this.snackBar.open('Failed to load audit logs', 'Close', { duration: 3000 });
      }
    });
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadAuditLogs();
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadAuditLogs();
  }

  exportToCsv(): void {
    this.snackBar.open('CSV export functionality coming soon', 'Close', { duration: 3000 });
  }

  getActionClass(action: string): string {
    if (action.startsWith('User_')) return 'user-action';
    if (action.startsWith('Report_')) return 'report-action';
    if (action.startsWith('Setting_')) return 'setting-action';
    return '';
  }

  formatAction(action: string): string {
    return action.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
  }

  isRecent(timestamp: string): boolean {
    const logDate = new Date(timestamp);
    const oneHourAgo = new Date();
    oneHourAgo.setHours(oneHourAgo.getHours() - 1);
    return logDate > oneHourAgo;
  }
}
