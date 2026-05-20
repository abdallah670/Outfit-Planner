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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { selectAdminLoading } from '../../../../core/state/admin/admin.selectors';
import { AdminUseCases } from '../../../../domain/usecases/admin.usecases';
import { SystemHealthDto, SystemLogDto, SystemPerformanceDto, SystemLogFilterRequest } from '../../../../domain/entities/admin.entity';

@Component({
  selector: 'app-admin-system',
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
    MatProgressSpinnerModule,
    MatCardModule
  ],
  templateUrl: './admin-system.component.html',
  styleUrls: ['./admin-system.component.scss']
})
export class AdminSystemComponent implements OnInit {
  private readonly adminUseCases = inject(AdminUseCases);
  private readonly snackBar = inject(MatSnackBar);
  private readonly store = inject(Store);

  systemHealth: SystemHealthDto | null = null;
  systemPerformance: SystemPerformanceDto | null = null;
  systemLogs: SystemLogDto[] = [];
  totalLogs = 0;
  pageSize = 25;
  currentPage = 1;
  loading$: Observable<boolean> = this.store.select(selectAdminLoading);

  displayedLogColumns: string[] = ['timestamp', 'level', 'message', 'source'];

  logLevel = '';
  logSearch = '';

  ngOnInit(): void {
    this.loadSystemHealth();
    this.loadSystemPerformance();
    this.loadLogs();
  }

  loadSystemHealth(): void {
    this.adminUseCases.getSystemHealth().subscribe({
      next: (response) => {
        this.systemHealth = response;
      },
      error: (error) => {
        this.snackBar.open('Failed to load system health', 'Close', { duration: 3000 });
      }
    });
  }

  loadSystemPerformance(): void {
    this.adminUseCases.getSystemPerformance().subscribe({
      next: (response) => {
        this.systemPerformance = response;
      },
      error: (error) => {
        this.snackBar.open('Failed to load system performance', 'Close', { duration: 3000 });
      }
    });
  }

  loadLogs(): void {
    const filter: SystemLogFilterRequest = {
      level: this.logLevel || undefined,
      search: this.logSearch || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.adminUseCases.getSystemLogs(filter).subscribe({
      next: (response) => {
        this.systemLogs = response.data;
        this.totalLogs = response.total;
      },
      error: (error) => {
        this.snackBar.open('Failed to load system logs', 'Close', { duration: 3000 });
      }
    });
  }

  onLogPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadLogs();
  }

  createBackup(): void {
    this.adminUseCases.createBackup({ type: 'full', description: 'Manual backup from admin panel' }).subscribe({
      next: () => {
        this.snackBar.open('System backup created successfully', 'Close', { duration: 3000 });
      },
      error: (error: any) => {
        this.snackBar.open('Failed to create system backup', 'Close', { duration: 3000 });
      }
    });
  }

  clearSystemCache(): void {
    if (confirm('Are you sure you want to clear the system cache?')) {
      this.adminUseCases.clearCache({}).subscribe({
        next: () => {
          this.snackBar.open('System cache cleared successfully', 'Close', { duration: 3000 });
        },
        error: (error: any) => {
          this.snackBar.open('Failed to clear system cache', 'Close', { duration: 3000 });
        }
      });
    }
  }

  getHealthClass(systemHealth: SystemHealthDto | null): string {
    if (!systemHealth) return '';
    
    // Determine overall health based on services
    const allHealthy = systemHealth.databaseHealthy && 
                      systemHealth.cacheHealthy && 
                      systemHealth.emailServiceHealthy;
    
    if (allHealthy) return 'healthy';
    
    const anyHealthy = systemHealth.databaseHealthy || 
                      systemHealth.cacheHealthy || 
                      systemHealth.emailServiceHealthy;
    
    if (anyHealthy) return 'warning';
    
    return 'error';
  }

  getHealthStatus(systemHealth: SystemHealthDto | null): string {
    if (!systemHealth) return 'Unknown';
    
    const allHealthy = systemHealth.databaseHealthy && 
                      systemHealth.cacheHealthy && 
                      systemHealth.emailServiceHealthy;
    
    if (allHealthy) return 'Healthy';
    
    const anyHealthy = systemHealth.databaseHealthy || 
                      systemHealth.cacheHealthy || 
                      systemHealth.emailServiceHealthy;
    
    if (anyHealthy) return 'Warning';
    
    return 'Error';
  }

  getLogLevelClass(level: string): string {
    return level.toLowerCase();
  }
}
