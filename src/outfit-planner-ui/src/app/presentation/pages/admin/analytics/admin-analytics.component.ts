import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { selectAdminLoading } from '../../../../core/state/admin/admin.selectors';
import { AdminUseCases } from '../../../../domain/usecases/admin.usecases';
import { DetailedAnalyticsDto, AnalyticsFilterRequest, ExportAnalyticsRequest } from '../../../../domain/entities/admin.entity';

@Component({
  selector: 'app-admin-analytics',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './admin-analytics.component.html',
  styleUrls: ['./admin-analytics.component.scss']
})
export class AdminAnalyticsComponent implements OnInit {
  private readonly adminUseCases = inject(AdminUseCases);
  private readonly snackBar = inject(MatSnackBar);
  private readonly store = inject(Store);

  analytics: DetailedAnalyticsDto | null = null;
  loading$: Observable<boolean> = this.store.select(selectAdminLoading);

  startDate = '';
  endDate = '';

  ngOnInit(): void {
    this.loadAnalytics();
  }

  loadAnalytics(): void {
    const filter: AnalyticsFilterRequest = {
      startDate: this.startDate || undefined,
      endDate: this.endDate || undefined
    };

    this.adminUseCases.getDetailedAnalytics(filter).subscribe({
      next: (response) => {
        this.analytics = response;
      },
      error: (error) => {
        this.snackBar.open('Failed to load analytics', 'Close', { duration: 3000 });
      }
    });
  }

  exportAnalytics(): void {
    const request: ExportAnalyticsRequest = {
      format: 'csv',
      startDate: this.startDate || undefined,
      endDate: this.endDate || undefined
    };

    this.adminUseCases.exportAnalytics(request).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `analytics-${new Date().toISOString().split('T')[0]}.csv`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        
        this.snackBar.open('Analytics exported successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        this.snackBar.open('Failed to export analytics', 'Close', { duration: 3000 });
      }
    });
  }
}
