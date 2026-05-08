import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { selectUsers, selectTotalUsers, selectAdminLoading } from '../../../../core/state/admin/admin.selectors';
import { loadUsers, banUser, unbanUser } from '../../../../core/state/admin/admin.actions';
import { AdminUser, UserFilterRequest } from '../../../../domain/entities/admin.entity';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatDialogModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="admin-users-container">
      <div class="page-header">
        <h2>User Management</h2>
        <div class="filters">
          <mat-form-field appearance="outline">
            <mat-label>Search</mat-label>
            <input matInput (keyup)="applyFilter()" [(ngModel)]="searchTerm" placeholder="Search users...">
          </mat-form-field>
          
          <mat-form-field appearance="outline">
            <mat-label>Role</mat-label>
            <mat-select [(ngModel)]="selectedRole" (selectionChange)="applyFilter()">
              <mat-option value="">All Roles</mat-option>
              <mat-option value="Admin">Admin</mat-option>
              <mat-option value="Planner">Planner</mat-option>
            </mat-select>
          </mat-form-field>
          
          <mat-form-field appearance="outline">
            <mat-label>Status</mat-label>
            <mat-select [(ngModel)]="selectedStatus" (selectionChange)="applyFilter()">
              <mat-option value="">All Status</mat-option>
              <mat-option value="active">Active</mat-option>
              <mat-option value="locked">Locked</mat-option>
              <mat-option value="banned">Banned</mat-option>
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
            <p>Loading users...</p>
          </div>
        } @else {
          <table mat-table [dataSource]="(users$ | async) || []" matSort>
            <!-- ID Column -->
            <ng-container matColumnDef="id">
              <th mat-header-cell *matHeaderCellDef> ID </th>
              <td mat-cell *matCellDef="let user"> {{user.id.substring(0, 8)}}... </td>
            </ng-container>

            <!-- Username Column -->
            <ng-container matColumnDef="userName">
              <th mat-header-cell *matHeaderCellDef mat-sort-header> Username </th>
              <td mat-cell *matCellDef="let user">
                <span [class.banned]="user.isBanned">{{ user.userName }}</span>
              </td>
            </ng-container>

            <!-- Email Column -->
            <ng-container matColumnDef="email">
              <th mat-header-cell *matHeaderCellDef mat-sort-header> Email </th>
              <td mat-cell *matCellDef="let user"> {{user.email}} </td>
            </ng-container>

            <!-- Name Column -->
            <ng-container matColumnDef="name">
              <th mat-header-cell *matHeaderCellDef mat-sort-header> Name </th>
              <td mat-cell *matCellDef="let user"> {{user.name}} </td>
            </ng-container>

            <!-- Roles Column -->
            <ng-container matColumnDef="roles">
              <th mat-header-cell *matHeaderCellDef> Roles </th>
              <td mat-cell *matCellDef="let user">
                <span class="role-badge" [class.admin]="user.roles.includes('Admin')">
                  {{user.roles.join(', ')}}
                </span>
              </td>
            </ng-container>

            <!-- Status Column -->
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef> Status </th>
              <td mat-cell *matCellDef="let user">
                @if (user.isBanned) {
                  <span class="status-badge banned">Banned</span>
                } @else if (user.isLocked) {
                  <span class="status-badge locked">Locked</span>
                } @else {
                  <span class="status-badge active">Active</span>
                }
              </td>
            </ng-container>

            <!-- Created At Column -->
            <ng-container matColumnDef="createdAt">
              <th mat-header-cell *matHeaderCellDef mat-sort-header> Created </th>
              <td mat-cell *matCellDef="let user"> {{user.createdAt | date:'short'}} </td>
            </ng-container>

            <!-- Actions Column -->
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef> Actions </th>
              <td mat-cell *matCellDef="let user">
                <button mat-icon-button (click)="viewUserDetail(user.id)" matTooltip="View Details">
                  <mat-icon>visibility</mat-icon>
                </button>
                
                @if (!user.isBanned) {
                  <button mat-icon-button (click)="banUser(user.id)" matTooltip="Ban User" color="warn">
                    <mat-icon>block</mat-icon>
                  </button>
                } @else {
                  <button mat-icon-button (click)="unbanUser(user.id)" matTooltip="Unban User" color="primary">
                    <mat-icon>check_circle</mat-icon>
                  </button>
                }
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>

          <mat-paginator
            [length]="totalUsers$ | async"
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
    .admin-users-container {
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
    
    .role-badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
      background: #e3f2fd;
      color: #1976d2;
    }
    
    .role-badge.admin {
      background: #fff3e0;
      color: #f57c00;
    }
    
    .status-badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
    }
    
    .status-badge.active {
      background: #e8f5e8;
      color: #2e7d32;
    }
    
    .status-badge.locked {
      background: #fff3e0;
      color: #f57c00;
    }
    
    .status-badge.banned {
      background: #ffebee;
      color: #c62828;
    }
    
    .banned {
      text-decoration: line-through;
      color: #999;
    }
  `]
})
export class AdminUsersComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);

  users$: Observable<AdminUser[]> = this.store.select(selectUsers);
  totalUsers$: Observable<number> = this.store.select(selectTotalUsers);
  loading$: Observable<boolean> = this.store.select(selectAdminLoading);

  displayedColumns: string[] = ['id', 'userName', 'email', 'name', 'roles', 'status', 'createdAt', 'actions'];
  pageSize = 25;
  currentPage = 1;
  searchTerm = '';
  selectedRole = '';
  selectedStatus = '';

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    const filter: UserFilterRequest = {
      search: this.searchTerm || undefined,
      role: this.selectedRole || undefined,
      status: this.selectedStatus || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };
    this.store.dispatch(loadUsers({ filter }));
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadUsers();
  }

  viewUserDetail(userId: string): void {
    // Navigate to user detail page
    // this.router.navigate(['/admin/users', userId]);
  }

  banUser(userId: string): void {
    const reason = prompt('Enter ban reason:');
    if (reason) {
      this.store.dispatch(banUser({ userId, reason, expiry: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000) }));
      this.snackBar.open('User banned successfully', 'Close', { duration: 3000 });
    }
  }

  unbanUser(userId: string): void {
    if (confirm('Are you sure you want to unban this user?')) {
      this.store.dispatch(unbanUser({ userId }));
      this.snackBar.open('User unbanned successfully', 'Close', { duration: 3000 });
    }
  }
}
