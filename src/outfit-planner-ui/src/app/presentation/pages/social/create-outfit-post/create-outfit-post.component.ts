import { Component, OnInit, inject, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import Swal from 'sweetalert2';
import { OutfitPostsActions } from '../../../../core/state/outfit-posts/outfit-posts.actions';
import { selectOutfitPostsLoading } from '../../../../core/state/outfit-posts/outfit-posts.selectors';
import { TaggedUser, Visibility } from '../../../../domain/entities/feed.entity';
import { OutfitDataSource } from '../../../../data/datasources/outfit.datasource';
import { Outfit } from '../../../../domain/entities/outfit.entity';
import { ClothingItem } from '../../../../domain/entities/clothing-item.entity';
import { FollowDataSource } from '../../../../data/datasources/follow.datasource';
import { AuthService } from '../../../../core/services/auth.service';
import { Follower } from '../../../../domain/entities/follow.entity';
import { WardrobeActions } from '../../../../core/state/wardrobe/wardrobe.actions';
import { selectAllItems } from '../../../../core/state/wardrobe/wardrobe.selectors';
import { toSignal } from '@angular/core/rxjs-interop';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { firstValueFrom, forkJoin, Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { FollowUseCases } from '../../../../domain/usecases/follow.usecases';
import { OutfitsUseCases } from '../../../../domain/usecases/outfit.usecases';

import { CreateOutfitPostRequest, UpdateOutfitPostRequest } from '../../../../domain/entities/outfitpost.entity';
import { OutfitPostUseCases } from '../../../../domain/usecases/outfit-posts.usecases';

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
  private outfitsUseCases= inject(OutfitsUseCases);
  private outfitPostUseCases = inject(OutfitPostUseCases);
  private followUseCases = inject(FollowUseCases);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private http = inject(HttpClient);

  private searchSubject = new Subject<string>();

  outfitPostForm!: FormGroup;
  loading$ = this.store.select(selectOutfitPostsLoading);
  isEditMode = signal(false);
  editPostId = signal<string | null>(null);
  currentOutfitId = signal<string | null>(null);

  // Current user for preview
  currentUser = this.authService.currentUser;

  // Photo upload
  selectedFile = signal<File | null>(null);
  photoPreviewUrl = signal<string | null>(null);
  isUploading = signal(false);

  visibilityOptions = [
    { value: Visibility.Public, label: 'Public' },
    { value: Visibility.Followers, label: 'Followers Only' },
    { value: Visibility.Private, label: 'Private' },
  ];
  // Follower tagging
  followersList = signal<Follower[]>([]);
  tagSearchQuery = signal('');
  taggedUsers = signal<TaggedUser[]>([]);
  showTagDropdown = signal(false);

  filteredFollowers = computed(() => {
    const query = this.tagSearchQuery().trim();
    const taggedIds = this.taggedUsers().map(t => t.userId);
    const all = this.followersList();
    if (!query) return [];
    return all.filter(f => !taggedIds.includes(f.userId));
  });

  canPost = computed(() => {
    if (this.isEditMode()) return true;
    return !!this.selectedFile() && !!this.photoPreviewUrl();
  });

  ngOnInit(): void {
    this.initForm();
    this.setupSearchDebounce();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.editPostId.set(id);
      this.loadPostData(id);
    }
  }

  private setupSearchDebounce(): void {
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(query => {
      const trimmedQuery = query.trim();
      if (trimmedQuery.length > 0) {
        this.loadFollowers(trimmedQuery);
      } else {
        this.followersList.set([]);
      }
    });
  }

  private loadPostData(id: string): void {
    this.isUploading.set(true);
    this.outfitPostUseCases.getOutfitPost(id).subscribe({
      next: (post) => {
        this.isUploading.set(false);
        this.currentOutfitId.set(post.outfitId || null);
        this.outfitPostForm.patchValue({
          caption: post.caption,
          visibility: post.visibility,
          // We don't have outfitName/occasion/season here, but we could if needed
        });
        
        if (post.outfit?.imageUrl) {
          this.photoPreviewUrl.set(post.outfit.imageUrl);
        }

        // Handle tagged users if they exist in the post
        if (post.taggedUsers && post.taggedUsers?.length > 0) {
          this.taggedUsers.set(post.taggedUsers);
        }
      },
      error: (err) => {
        this.isUploading.set(false);
        console.error('Failed to load post data:', err);
        Swal.fire('Error', 'Could not load post data.', 'error');
      }
    });
  }

 
  private initForm(): void {
    this.outfitPostForm = this.fb.group({
      outfitName: ['My Awesome Outfit', [Validators.required, Validators.maxLength(100)]],
      outfitOccasion: ['Casual', [Validators.required]],
      outfitSeason: ['AllSeason', [Validators.required]],
      caption: ['', [Validators.maxLength(500)]],
      visibility: [Visibility.Public, Validators.required],
    });
  }

  private loadFollowers(search?: string): void {
    const userId = this.authService.currentUser()?.id;
    if (!userId) return;
    this.followUseCases.getFollowers(userId, undefined, 10, search).subscribe({
      next: (result) => {
        this.followersList.set(result.items || []);
      },
      error: (err) => {
        console.error('Failed to load following list:', err);
      },
    });
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

      this.selectedFile.set(file);

      const reader = new FileReader();
      reader.onload = (e) => {
        this.photoPreviewUrl.set(e.target?.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  removePhoto(): void {
    this.selectedFile.set(null);
    this.photoPreviewUrl.set(null);
  }

  async onSubmit(): Promise<void> {
    if (this.outfitPostForm.invalid) {
      this.markFormGroupTouched(this.outfitPostForm);
      return;
    }

    if (this.isEditMode()) {
       this.dispatchUpdatePost();
       return;
    }

    if (this.selectedFile()) {
      this.isUploading.set(true);

      this.outfitsUseCases.createOutfitWithImage(this.selectedFile()!).subscribe({
        next: (outfit) => {
          this.isUploading.set(false);
          this.dispatchCreatePost(outfit.id);
        },
        error: (err) => {
          this.isUploading.set(false);
          console.error('Failed to create outfit from photo:', err);
          Swal.fire('Error', 'Failed to upload outfit photo. Please try again.', 'error');
        },
      });
    }
  }

  private dispatchUpdatePost(): void {
    const id = this.editPostId();
    if (!id) return;

    this.isUploading.set(true);
    const dto: UpdateOutfitPostRequest = {
      outfitId: this.currentOutfitId() || undefined,
      caption: this.outfitPostForm.value.caption || '',
      visibility: this.outfitPostForm.value.visibility || Visibility.Public,
      tags: this.taggedUsers().map(u => u.userName),
    };

    this.outfitPostUseCases.updateOutfitPost(id, dto).subscribe({
      next: (response) => {
        this.isUploading.set(false);
        Swal.fire({
          title: 'Updated!',
          text: 'Your outfit post has been updated.',
          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
        }).then(() => {
          this.router.navigate(['/social/posts', id]);
        });
      },
      error: (err) => {
        this.isUploading.set(false);
        console.error('Failed to update post:', err);
        Swal.fire('Error', 'Failed to update post. Please try again.', 'error');
      }
    });
  }

  private dispatchCreatePost(outfitId: string): void {
    const dto: CreateOutfitPostRequest = {
      outfitId: outfitId,
      caption: this.outfitPostForm.value.caption || '',
      visibility: this.outfitPostForm.value.visibility || Visibility.Public,
      tags:this.taggedUsers().map(u => u.userName),
    };
   
    this.outfitPostUseCases.createOutfitPost(dto).subscribe({
      next: (response) => {
        console.log('Post created successfully:', response);
        this.isUploading.set(false);
        Swal.fire({
          title: 'Success!',
          text: 'Your outfit post has been shared.',
          icon: 'success',
            timer: 2000,
          showConfirmButton: false,
        }).then(() => {
          this.router.navigate(['/social/posts', response.id]);
        });
      },
        error: (err) => {
          this.isUploading.set(false);
          console.error('Failed to create outfit from photo:', err);
          Swal.fire('Error', 'Failed to upload outfit photo. Please try again.', 'error');
        }
    });
  }

  onCancel(): void {
    this.router.navigate(['/social']);
  }

  // Tag management
  tagUser(user: Follower): void {
    const tagged: TaggedUser = {
      userId: user.userId,
      userName: user.userName,
      profilePictureUrl: user.userAvatarUrl
    };
    this.taggedUsers.update(users => [...users, tagged]);
    this.tagSearchQuery.set('');
    this.showTagDropdown.set(false);
  }

  removeTag(userId: string): void {
    this.taggedUsers.update(users => users.filter(u => u.userId !== userId));
  }

  onTagSearchChange(query: string): void {
    this.tagSearchQuery.set(query);
    this.showTagDropdown.set(query.trim().length > 0);
    this.searchSubject.next(query);
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