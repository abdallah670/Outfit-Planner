import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { selectAnalytics, selectAdminLoading } from '../../../../core/state/admin/admin.selectors';
import { loadDashboardAnalytics } from '../../../../core/state/admin/admin.actions';
import { AnalyticsDashboard } from '../../../../domain/entities/admin.entity';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
})
export class AdminDashboardComponent implements OnInit {
  private readonly store = inject(Store);

  analytics$: Observable<AnalyticsDashboard | null> = this.store.select(selectAnalytics);
  loading$: Observable<boolean> = this.store.select(selectAdminLoading);

  ngOnInit(): void {
    this.store.dispatch(loadDashboardAnalytics({}));
  }
}