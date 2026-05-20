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
import { AdminPostDto, ContentFilterRequest, BulkPostOperationRequest } from '../../../../domain/entities/admin.entity';

@Component({
  selector: 'app-admin-posts',
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
  templateUrl: './admin-posts.component.html',
  styleUrls: ['./admin-posts.component.scss']
})
export class AdminPostsComponent implements OnInit {
  private readonly adminUseCases = inject(AdminUseCases);
  private readonly snackBar = inject(MatSnackBar);
  private readonly store = inject(Store);

  posts: AdminPostDto[] = [];
  totalPosts = 0;
  pageSize = 25;
  currentPage = 1;
  loading$: Observable<boolean> = this.store.select(selectAdminLoading);

  displayedColumns: string[] = ['select', 'id', 'title', 'userName', 'status', 'engagement', 'createdAt', 'actions'];
  selectedPosts = new Set<string>();

  searchFilter = '';
  statusFilter = '';

  ngOnInit(): void {
    this.loadPosts();
  }

  loadPosts(): void {
    const filter: ContentFilterRequest = {
      search: this.searchFilter || undefined,
      status: this.statusFilter || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.adminUseCases.getPosts(filter).subscribe({
      next: (response) => {
        this.posts = response.data;
        this.totalPosts = response.total;
      },
      error: (error) => {
        this.snackBar.open('Failed to load posts', 'Close', { duration: 3000 });
      }
    });
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadPosts();
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadPosts();
  }

  togglePostSelection(postId: string): void {
    if (this.selectedPosts.has(postId)) {
      this.selectedPosts.delete(postId);
    } else {
      this.selectedPosts.add(postId);
    }
  }

  toggleAllPosts(): void {
    if (this.isAllSelected()) {
      this.selectedPosts.clear();
    } else {
      this.posts.forEach(post => this.selectedPosts.add(post.id));
    }
  }

  isAllSelected(): boolean {
    return this.posts.length > 0 && this.selectedPosts.size === this.posts.length;
  }

  approvePost(postId: string): void {
    this.adminUseCases.approvePost(postId).subscribe({
      next: () => {
        this.snackBar.open('Post approved successfully', 'Close', { duration: 3000 });
        this.loadPosts();
      },
      error: (error) => {
        this.snackBar.open('Failed to approve post', 'Close', { duration: 3000 });
      }
    });
  }

  rejectPost(postId: string): void {
    const reason = 'Rejected by admin';
    this.adminUseCases.rejectPost(postId, { reason }).subscribe({
      next: () => {
        this.snackBar.open('Post rejected successfully', 'Close', { duration: 3000 });
        this.loadPosts();
      },
      error: (error) => {
        this.snackBar.open('Failed to reject post', 'Close', { duration: 3000 });
      }
    });
  }

  deletePost(postId: string): void {
    if (confirm('Are you sure you want to delete this post?')) {
      this.adminUseCases.deletePost(postId).subscribe({
        next: () => {
          this.snackBar.open('Post deleted successfully', 'Close', { duration: 3000 });
          this.loadPosts();
        },
        error: (error) => {
          this.snackBar.open('Failed to delete post', 'Close', { duration: 3000 });
        }
      });
    }
  }

  bulkApprove(): void {
    const operations = Array.from(this.selectedPosts).map(postId => ({
      postId,
      type: 'approve'
    }));

    this.adminUseCases.bulkPostOperations({ operations }).subscribe({
      next: (response) => {
        this.snackBar.open(`Approved ${response.successfulOperations} posts`, 'Close', { duration: 3000 });
        this.selectedPosts.clear();
        this.loadPosts();
      },
      error: (error) => {
        this.snackBar.open('Failed to approve posts', 'Close', { duration: 3000 });
      }
    });
  }

  bulkReject(): void {
    const operations = Array.from(this.selectedPosts).map(postId => ({
      postId,
      type: 'reject',
      reason: 'Bulk rejected by admin'
    }));

    this.adminUseCases.bulkPostOperations({ operations }).subscribe({
      next: (response) => {
        this.snackBar.open(`Rejected ${response.successfulOperations} posts`, 'Close', { duration: 3000 });
        this.selectedPosts.clear();
        this.loadPosts();
      },
      error: (error) => {
        this.snackBar.open('Failed to reject posts', 'Close', { duration: 3000 });
      }
    });
  }

  bulkDelete(): void {
    if (confirm(`Are you sure you want to delete ${this.selectedPosts.size} posts?`)) {
      const operations = Array.from(this.selectedPosts).map(postId => ({
        postId,
        type: 'delete'
      }));

      this.adminUseCases.bulkPostOperations({ operations }).subscribe({
        next: (response) => {
          this.snackBar.open(`Deleted ${response.successfulOperations} posts`, 'Close', { duration: 3000 });
          this.selectedPosts.clear();
          this.loadPosts();
        },
        error: (error) => {
          this.snackBar.open('Failed to delete posts', 'Close', { duration: 3000 });
        }
      });
    }
  }

  getStatusText(post: AdminPostDto): string {
    if (post.isApproved) return 'Approved';
    if (post.status === '3') return 'Rejected';
    if (post.status === '4') return 'Hidden';
    return 'Published';
  }

  getStatusClass(post: AdminPostDto): string {
    switch (post.status) {
      case '1': return 'published';
      case '2': return 'approved';
      case '3': return 'rejected';
      case '4': return 'hidden';
      default: return '';
    }
  }
}
