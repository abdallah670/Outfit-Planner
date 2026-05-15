import { Component, OnInit, inject, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import Swal from 'sweetalert2';
import { OutfitPostsActions } from '../../../../core/state/outfit-posts/outfit-posts.actions';
import { selectOutfitPostsLoading } from '../../../../core/state/outfit-posts/outfit-posts.selectors';
import { Visibility } from '../../../../domain/entities/feed.entity';

interface Outfit {
  id: string;
  name: string;
  imageUrl: string;
  category?: string;
}

type CreateMode = 'photo' | 'items';

@Component({
  selector: 'app-create-outfit-post',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './create-outfit-post.component.html',
  styleUrls: ['./create-outfit-post.component.scss'],
})
export class CreateOutfitPostComponent implements OnInit {
  private fb = inject(FormBuilder);
  private store = inject(Store);
  private router = inject(Router);

  outfitPostForm!: FormGroup;
  loading$ = this.store.select(selectOutfitPostsLoading);

  // Tab management
  activeTab = signal('select');

  // Create mode: photo or items
  createMode = signal<CreateMode>('photo');

  // Mock outfits - in real app, this would come from user's wardrobe
  mockOutfits: Outfit[] = [
    { id: '1', name: 'Weekend Coffee Run', imageUrl: 'https://storage.googleapis.com/banani-generated-images/generated-images/b649967f-80c6-41bb-8c21-25f7c331bb35.jpg', category: 'Spring • 3 items • Casual' },
    { id: '2', name: 'Business Professional', imageUrl: 'assets/placeholder.jpg', category: 'Work • 4 items • Formal' },
    { id: '3', name: 'Weekend Vibes', imageUrl: 'assets/placeholder.jpg', category: 'Weekend • 2 items • Casual' },
    { id: '4', name: 'Date Night', imageUrl: 'assets/placeholder.jpg', category: 'Evening • 2 items • Date' },
  ];

  selectedOutfitId: string | null = null;
  searchQuery = signal('');

  // Photo upload
  selectedFile: File | null = null;
  photoPreviewUrl = signal<string | null>(null);
  isUploading = signal(false);

  // Clothing items for "build from items" mode
  mockClothingItems = [
    { id: 'item1', name: 'White T-Shirt', type: 'Top', imageUrl: 'assets/placeholder.jpg' },
    { id: 'item2', name: 'Blue Jeans', type: 'Bottom', imageUrl: 'assets/placeholder.jpg' },
    { id: 'item3', name: 'Brown Jacket', type: 'Outerwear', imageUrl: 'assets/placeholder.jpg' },
    { id: 'item4', name: 'White Sneakers', type: 'Shoes', imageUrl: 'assets/placeholder.jpg' },
  ];
  selectedClothingItems: string[] = [];

  captionValue = '';

  visibilityOptions = [
    { value: Visibility.Public, label: 'Public' },
    { value: Visibility.FriendsOnly, label: 'Friends Only' },
    { value: Visibility.Private, label: 'Private' },
  ];

  selectedVisibility = Visibility.Public;
  tagFriends: string[] = ['@emily_styles', '@sarah_j'];

  filteredOutfits = computed<Outfit[]>(() => {
    if (!this.searchQuery().trim()) return this.mockOutfits;
    const lower = this.searchQuery().toLowerCase();
    return this.mockOutfits.filter(
      (o) =>
        o.name.toLowerCase().includes(lower) ||
        o.category?.toLowerCase().includes(lower)
    );
  });

  isOutfitSelected = computed(() => {
    return (outfitId: string) => this.selectedOutfitId === outfitId;
  });

  selectedOutfit = computed(() => {
    return this.mockOutfits.find((o) => o.id === this.selectedOutfitId) || null;
  });

  canPost = computed(() => {
    if (this.activeTab() === 'select') {
      return !!this.selectedOutfitId;
    } else {
      if (this.createMode() === 'photo') {
        return !!this.selectedFile && !!this.photoPreviewUrl();
      } else {
        return this.selectedClothingItems.length > 0;
      }
    }
  });

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    this.outfitPostForm = this.fb.group({
      caption: ['', [Validators.maxLength(500)]],
      visibility: [Visibility.Public, Validators.required],
    });
  }

  // Tab switching
  setTab(tab: 'select' | 'create'): void {
    this.activeTab.set(tab);
    this.selectedOutfitId = null;
    this.selectedFile = null;
    this.photoPreviewUrl.set(null);
    this.selectedClothingItems = [];
  }

  setCreateMode(mode: CreateMode): void {
    this.createMode.set(mode);
    if (mode === 'photo') {
      this.selectedClothingItems = [];
    } else {
      this.selectedFile = null;
      this.photoPreviewUrl.set(null);
    }
  }

  // Outfit selection
  selectOutfit(outfitId: string): void {
    this.selectedOutfitId = outfitId;
  }

  // Search
  onSearchChange(query: string): void {
    this.searchQuery.set(query);
  }

  // Photo upload handling
  onUploadAreaClick(): void {
    if (!this.photoPreviewUrl()) {
      const fileInput = document.getElementById('fileInput') as HTMLInputElement;
      if (fileInput) fileInput.click();
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
      if (!allowedTypes.includes(file.type)) {
        Swal.fire('Invalid File', 'Please select a valid image file (JPEG, PNG, or WebP)', 'warning');
        return;
      }

      if (file.size > 5 * 1024 * 1024) {
        Swal.fire('File Too Large', 'File size must be less than 5MB', 'warning');
        return;
      }

      this.selectedFile = file;

      const reader = new FileReader();
      reader.onload = (e) => {
        this.photoPreviewUrl.set(e.target?.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  removePhoto(): void {
    this.selectedFile = null;
    this.photoPreviewUrl.set(null);
  }

  // Clothing item selection
  toggleClothingItem(itemId: string): void {
    if (this.selectedClothingItems.includes(itemId)) {
      this.selectedClothingItems = this.selectedClothingItems.filter((id) => id !== itemId);
    } else {
      this.selectedClothingItems = [...this.selectedClothingItems, itemId];
    }
  }

  isItemSelected(itemId: string): boolean {
    return this.selectedClothingItems.includes(itemId);
  }

  onSubmit(): void {
    if (this.outfitPostForm.invalid) {
      this.markFormGroupTouched(this.outfitPostForm);
      return;
    }

    const formValue = this.outfitPostForm.value;

    this.store.dispatch(
      OutfitPostsActions.createOutfitPost({
        outfitId: this.activeTab() === 'select' ? this.selectedOutfitId! : 'new',
        caption: formValue.caption,
        visibility: formValue.visibility || Visibility.Public,
      })
    );

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

  removeTag(index: number): void {
    this.tagFriends.splice(index, 1);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach((control) => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }
}