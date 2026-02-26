import { Component, OnInit, inject, Signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
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
  ],
  templateUrl: './clothing-item-detail.html',
  styleUrl: './clothing-item-detail.scss',
})
export class ClothingItemDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private store = inject(Store);

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

  onDelete() {
    const currentItem = this.item();
    if (currentItem) {
      Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this! The clothing item will be permanently deleted.",
        icon: 'warning',
        showCancelButton: true,
        background: '#1f2937',
        color: '#f9fafb',
        confirmButtonColor: '#ef4444',
        cancelButtonColor: '#4b5563',
        confirmButtonText: 'Yes, delete it!',
      }).then((result) => {
        if (result.isConfirmed) {
          this.store.dispatch(WardrobeActions.deleteClothingItem({ id: currentItem.id }));
        }
      });
    }
  }
}
