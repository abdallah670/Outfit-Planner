import { Component, OnInit, inject, Signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import Swal from 'sweetalert2';

import { WardrobeState } from '../../../core/state/wardrobe/wardrobe.reducer';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';
import { WardrobeActions } from '../../../core/state/wardrobe/wardrobe.actions';
import {
  selectSelectedItem,
  selectWardrobeLoading,
} from '../../../core/state/wardrobe/wardrobe.selectors';

@Component({
  selector: 'app-clothing-item-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    DatePipe,
    MatProgressSpinnerModule,
    MatIconModule,
    MatButtonModule,
    MatSnackBarModule,
  ],
  templateUrl: './clothing-item-detail.html',
  styleUrl: './clothing-item-detail.scss',
})
export class ClothingItemDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private store = inject(Store);
  private snackBar = inject(MatSnackBar);

  item: Signal<ClothingItem | null> = toSignal(this.store.select(selectSelectedItem), {
    initialValue: null,
  });
  loading: Signal<boolean> = toSignal(this.store.select(selectWardrobeLoading), {
    initialValue: false,
  });

  imageError = false;

  ngOnInit() {
    this.route.paramMap.subscribe((params: any) => {
      const id = params.get('id');
      if (id) {
        this.store.dispatch(WardrobeActions.loadClothingItemById({ id }));
      }
    });
  }

  onRecordWear() {
    const currentItem = this.item();
    if (currentItem) {
      this.store.dispatch(WardrobeActions.recordWear({ id: currentItem.id }));
      this.snackBar.open('Wear recorded! 👕', 'Nice!', { duration: 3000 });
    }
  }

  onDelete() {
    const currentItem = this.item();
    if (currentItem) {
      Swal.fire({
        title: 'Delete Item?',
        text: 'This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        background: '#ffffff',
        color: '#2D3436',
        confirmButtonColor: '#E17055',
        cancelButtonColor: '#DFE6E9',
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
      }).then((result) => {
        if (result.isConfirmed) {
          this.store.dispatch(WardrobeActions.deleteClothingItem({ id: currentItem.id }));
        }
      });
    }
  }
}
