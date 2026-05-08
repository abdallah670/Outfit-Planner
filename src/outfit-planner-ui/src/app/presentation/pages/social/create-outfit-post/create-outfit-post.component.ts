import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import Swal from 'sweetalert2';
import { OutfitPostsActions } from '../../../../core/state/outfit-posts/outfit-posts.actions';
import { selectOutfitPostsLoading } from '../../../../core/state/outfit-posts/outfit-posts.selectors';
import { Visibility } from '../../../../domain/entities/feed.entity';
import { take } from 'rxjs/operators';

@Component({
  selector: 'app-create-outfit-post',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule,
  ],
  templateUrl: './create-outfit-post.component.html',
  styleUrls: ['./create-outfit-post.component.scss'],
})
export class CreateOutfitPostComponent implements OnInit {
  private fb = inject(FormBuilder);
  private store = inject(Store);
  private router = inject(Router);

  outfitPostForm!: FormGroup;
  userOutfits: any[] = [];
  loading$ = this.store.select(selectOutfitPostsLoading);
  selectedOutfit: any | null = null;

  // Mock data - in real app, this would come from user's wardrobe
  mockOutfits = [
    { id: '1', name: 'Summer Casual', imageUrl: 'assets/placeholder.jpg' },
    { id: '2', name: 'Business Professional', imageUrl: 'assets/placeholder.jpg' },
    { id: '3', name: 'Weekend Vibes', imageUrl: 'assets/placeholder.jpg' },
  ];

  visibilityOptions = [
    { value: Visibility.Public, label: 'Public' },
    { value: Visibility.FriendsOnly, label: 'Friends Only' },
    { value: Visibility.Private, label: 'Private' },
  ];

  ngOnInit(): void {
    this.initForm();
    this.loadUserOutfits();
  }

  private initForm(): void {
    this.outfitPostForm = this.fb.group({
      outfitId: ['', Validators.required],
      caption: ['', [Validators.maxLength(500)]],
      visibility: [Visibility.Public, Validators.required],
    });
  }

  private loadUserOutfits(): void {
    // In real implementation, load from user's wardrobe
    this.userOutfits = this.mockOutfits;
  }

  selectOutfit(outfit: any): void {
    this.selectedOutfit = outfit;
    this.outfitPostForm.patchValue({ outfitId: outfit.id });
  }

  onSubmit(): void {
    if (this.outfitPostForm.valid && this.selectedOutfit) {
      const formValue = this.outfitPostForm.value;
      
      this.store.dispatch(
        OutfitPostsActions.createOutfitPost({
          outfitId: formValue.outfitId,
          caption: formValue.caption,
          visibility: formValue.visibility,
        })
      );

      // Show success and navigate
      Swal.fire({
        title: 'Success!',
        text: 'Your outfit post has been shared.',
        icon: 'success',
        timer: 2000,
        showConfirmButton: false,
      }).then(() => {
        this.router.navigate(['/social/my-posts']);
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/social/my-posts']);
  }

  getCaptionCharCount(): number {
    const caption = this.outfitPostForm.get('caption')?.value || '';
    return caption.length;
  }

  getCaptionRemaining(): number {
    return 500 - this.getCaptionCharCount();
  }
}
