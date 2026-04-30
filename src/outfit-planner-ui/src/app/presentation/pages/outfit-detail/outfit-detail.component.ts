import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { Observable } from 'rxjs';
import Swal from 'sweetalert2';
import { OutfitPostsActions } from '../../../core/state/outfit-posts/outfit-posts.actions';
import { OutfitsActions } from '../../../core/state/outfit/outfit.actions';
import {
  selectSelectedItem,
  selectOutfitLoading,
} from '../../../core/state/outfit/outfit.selectors';
import { OutfitState } from '../../../core/state/outfit/outfit.reducer';
import { Outfit } from '../../../domain/entities/outfit.entity';
import { OutfitCanvasService } from '../../../core/services/outfit-canvas.service';

@Component({
  selector: 'app-outfit-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule,
  ],
  templateUrl: './outfit-detail.component.html',
  styleUrl: './outfit-detail.component.scss',
})
export class OutfitDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private store = inject(Store<{ outfit: OutfitState }>);
  private canvasService = inject(OutfitCanvasService);

  outfit$: Observable<Outfit | null> = this.store.select(selectSelectedItem);
  loading$: Observable<boolean> = this.store.select(selectOutfitLoading);
  
  combinedImageUrl: string | null = null;
  isGeneratingCombined = false;

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.store.dispatch(OutfitsActions.loadOutfitById({ id }));
    }
  }
  
  async generateCombinedImage(items: any[]): Promise<void> {
    if (!items || items.length === 0 || this.combinedImageUrl) return;
    
    this.isGeneratingCombined = true;
    try {
      this.combinedImageUrl = await this.canvasService.combineOutfitImages(items);
    } catch (error) {
      console.error('Failed to generate combined image:', error);
    } finally {
      this.isGeneratingCombined = false;
    }
  }
  
  downloadCombinedImage(): void {
    if (this.combinedImageUrl) {
      this.canvasService.downloadCombinedImage(this.combinedImageUrl, 'outfit-detail');
    }
  }

  onEdit(id: string) {
    this.router.navigate(['/outfits/build', id]);
  }

  onDelete(id: string) {
    Swal.fire({
      title: 'Are you sure?',
      text: "You won't be able to revert this!",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#f8b4c4',
      cancelButtonColor: '#64748b',
      confirmButtonText: 'Yes, delete it!',
      background: '#ffffff',
      color: '#2d3436',
    }).then((result) => {
      if (result.isConfirmed) {
        this.store.dispatch(OutfitsActions.deleteOutfit({ id }));
      }
    });
  }

  onRecordWear(id: string) {
    this.store.dispatch(OutfitsActions.recordOutfitWear({ id }));
  }

  onShareToFeed(id: string): void {
    this.store.dispatch(OutfitPostsActions.createOutfitPost({
      outfitId: id,
      caption: '',
      visibility: 0 // Public = 0
    }));
  }
}
