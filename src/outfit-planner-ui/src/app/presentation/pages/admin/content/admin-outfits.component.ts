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
import { AdminOutfitDto, ContentFilterRequest, BulkOutfitOperationRequest } from '../../../../domain/entities/admin.entity';

@Component({
  selector: 'app-admin-outfits',
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
  templateUrl: './admin-outfits.component.html',
  styleUrls: ['./admin-outfits.component.scss']
})
export class AdminOutfitsComponent implements OnInit {
  private readonly adminUseCases = inject(AdminUseCases);
  private readonly snackBar = inject(MatSnackBar);
  private readonly store = inject(Store);

  outfits: AdminOutfitDto[] = [];
  totalOutfits = 0;
  pageSize = 25;
  currentPage = 1;
  loading$: Observable<boolean> = this.store.select(selectAdminLoading);

  displayedColumns: string[] = ['select', 'id', 'name', 'userName', 'status', 'engagement', 'createdAt', 'actions'];
  selectedOutfits = new Set<string>();

  searchFilter = '';
  statusFilter = '';

  ngOnInit(): void {
    this.loadOutfits();
  }

  loadOutfits(): void {
    const filter: ContentFilterRequest = {
      search: this.searchFilter || undefined,
      status: this.statusFilter || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.adminUseCases.getOutfits(filter).subscribe({
      next: (response) => {
        this.outfits = response.data;
        this.totalOutfits = response.total;
      },
      error: (error) => {
        this.snackBar.open('Failed to load outfits', 'Close', { duration: 3000 });
      }
    });
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadOutfits();
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadOutfits();
  }

  toggleOutfitSelection(outfitId: string): void {
    if (this.selectedOutfits.has(outfitId)) {
      this.selectedOutfits.delete(outfitId);
    } else {
      this.selectedOutfits.add(outfitId);
    }
  }

  toggleAllOutfits(): void {
    if (this.isAllSelected()) {
      this.selectedOutfits.clear();
    } else {
      this.outfits.forEach(outfit => this.selectedOutfits.add(outfit.id));
    }
  }

  isAllSelected(): boolean {
    return this.outfits.length > 0 && this.selectedOutfits.size === this.outfits.length;
  }

  approveOutfit(outfitId: string): void {
    this.adminUseCases.approveOutfit(outfitId).subscribe({
      next: () => {
        this.snackBar.open('Outfit approved successfully', 'Close', { duration: 3000 });
        this.loadOutfits();
      },
      error: (error) => {
        this.snackBar.open('Failed to approve outfit', 'Close', { duration: 3000 });
      }
    });
  }

  rejectOutfit(outfitId: string): void {
    const reason = 'Rejected by admin';
    this.adminUseCases.rejectOutfit(outfitId, { reason }).subscribe({
      next: () => {
        this.snackBar.open('Outfit rejected successfully', 'Close', { duration: 3000 });
        this.loadOutfits();
      },
      error: (error) => {
        this.snackBar.open('Failed to reject outfit', 'Close', { duration: 3000 });
      }
    });
  }

  featureOutfit(outfitId: string): void {
    this.adminUseCases.featureOutfit(outfitId).subscribe({
      next: () => {
        this.snackBar.open('Outfit featured successfully', 'Close', { duration: 3000 });
        this.loadOutfits();
      },
      error: (error) => {
        this.snackBar.open('Failed to feature outfit', 'Close', { duration: 3000 });
      }
    });
  }

  unfeatureOutfit(outfitId: string): void {
    this.adminUseCases.unfeatureOutfit(outfitId).subscribe({
      next: () => {
        this.snackBar.open('Outfit unfeatured successfully', 'Close', { duration: 3000 });
        this.loadOutfits();
      },
      error: (error) => {
        this.snackBar.open('Failed to unfeature outfit', 'Close', { duration: 3000 });
      }
    });
  }

  deleteOutfit(outfitId: string): void {
    if (confirm('Are you sure you want to delete this outfit?')) {
      this.adminUseCases.deleteOutfit(outfitId).subscribe({
        next: () => {
          this.snackBar.open('Outfit deleted successfully', 'Close', { duration: 3000 });
          this.loadOutfits();
        },
        error: (error) => {
          this.snackBar.open('Failed to delete outfit', 'Close', { duration: 3000 });
        }
      });
    }
  }

  bulkApprove(): void {
    const operations = Array.from(this.selectedOutfits).map(outfitId => ({
      outfitId,
      type: 'approve'
    }));

    this.adminUseCases.bulkOutfitOperations({ operations }).subscribe({
      next: (response) => {
        this.snackBar.open(`Approved ${response.successfulOperations} outfits`, 'Close', { duration: 3000 });
        this.selectedOutfits.clear();
        this.loadOutfits();
      },
      error: (error) => {
        this.snackBar.open('Failed to approve outfits', 'Close', { duration: 3000 });
      }
    });
  }

  bulkReject(): void {
    const operations = Array.from(this.selectedOutfits).map(outfitId => ({
      outfitId,
      type: 'reject',
      reason: 'Bulk rejected by admin'
    }));

    this.adminUseCases.bulkOutfitOperations({ operations }).subscribe({
      next: (response) => {
        this.snackBar.open(`Rejected ${response.successfulOperations} outfits`, 'Close', { duration: 3000 });
        this.selectedOutfits.clear();
        this.loadOutfits();
      },
      error: (error) => {
        this.snackBar.open('Failed to reject outfits', 'Close', { duration: 3000 });
      }
    });
  }

  bulkFeature(): void {
    const operations = Array.from(this.selectedOutfits).map(outfitId => ({
      outfitId,
      type: 'feature'
    }));

    this.adminUseCases.bulkOutfitOperations({ operations }).subscribe({
      next: (response) => {
        this.snackBar.open(`Featured ${response.successfulOperations} outfits`, 'Close', { duration: 3000 });
        this.selectedOutfits.clear();
        this.loadOutfits();
      },
      error: (error) => {
        this.snackBar.open('Failed to feature outfits', 'Close', { duration: 3000 });
      }
    });
  }

  bulkDelete(): void {
    if (confirm(`Are you sure you want to delete ${this.selectedOutfits.size} outfits?`)) {
      const operations = Array.from(this.selectedOutfits).map(outfitId => ({
        outfitId,
        type: 'delete'
      }));

      this.adminUseCases.bulkOutfitOperations({ operations }).subscribe({
        next: (response) => {
          this.snackBar.open(`Deleted ${response.successfulOperations} outfits`, 'Close', { duration: 3000 });
          this.selectedOutfits.clear();
          this.loadOutfits();
        },
        error: (error) => {
          this.snackBar.open('Failed to delete outfits', 'Close', { duration: 3000 });
        }
      });
    }
  }

  getStatusText(outfit: AdminOutfitDto): string {
    if (outfit.isApproved) return 'Approved';
    if (outfit.isFeatured) return 'Featured';
    return 'Pending';
  }

  getStatusClass(outfit: AdminOutfitDto): string {
    if (outfit.isApproved) return 'approved';
    if (outfit.isFeatured) return 'featured';
    return 'pending';
  }
}
