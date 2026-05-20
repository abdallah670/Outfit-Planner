import { Component, OnInit, Inject, inject, signal, computed, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { Store } from '@ngrx/store';
import { PollsActions } from '../../../../core/state/polls/polls.actions';
import { POLLS_REPOSITORY, PollsRepository } from '../../../../domain/repositories/polls.repository';
import {  FeedPost, PostType, Visibility, TaggedUser } from '../../../../domain/entities/feed.entity';
import { FeedUseCases } from '../../../../domain/usecases/feed.usecases';
import { CreatePollRequest, UpdatePollRequest, PollStatus } from '../../../../domain/entities/poll.entity';
import { OutfitsActions } from '../../../../core/state/outfit/outfit.actions';
import { OutfitsUseCases } from '../../../../domain/usecases/outfit.usecases';
import { PollsUseCases } from '../../../../domain/usecases/polls.usecases';
import { Follower } from '../../../../domain/entities/follow.entity';
import { FollowUseCases } from '../../../../domain/usecases/follow.usecases';
import { AuthService } from '../../../../core/services/auth.service';
import { Subject, debounceTime, distinctUntilChanged, forkJoin } from 'rxjs';
import Swal from 'sweetalert2';

interface PollOptionForm {
  description: string;
  outfitId?: string;
}

@Component({
  selector: 'app-create-poll',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatCardModule,
    MatDividerModule,
  ],
  templateUrl: './create-poll.component.html',
  styleUrl: './create-poll.component.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class CreatePollComponent implements OnInit {
  pollForm!: FormGroup;
  maxOptions = 6;
  minOptions = 2;
  privacySetting: 'public' | 'followers' = 'public';
  private pollUseCases = inject(PollsUseCases);
  private feedUseCases = inject(FeedUseCases);
  outfitsUseCases = inject(OutfitsUseCases);
  private route = inject(ActivatedRoute);
  private followUseCases = inject(FollowUseCases);
  private authService = inject(AuthService);

  private searchSubject = new Subject<string>();

  isEditMode = false;
  pollId: string | null = null;

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

  // Per-option image upload state
  optionFiles = signal<(File | null)[]>([]);
  optionPreviews = signal<(string | null)[]>([]);
  isSubmitting = signal(false);

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private store: Store,
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.setupSearchDebounce();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.pollId = id;
      this.loadPollForEdit(id);
    } else {
      // Add initial 2 options for creation mode
      this.addOption();
      this.addOption();
    }
  }

  private initForm(): void {
    this.pollForm = this.fb.group({
      question: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(500)]],
      duration: ['7d', Validators.required],
      privacy: ['public', Validators.required],
      options: this.fb.array([], [Validators.required, Validators.minLength(this.minOptions)]),
    });
  }

  private loadPollForEdit(id: string): void {
    this.feedUseCases.getPostById(id).subscribe({
      next: (post) => {
        if (!post.poll) return;
        const poll = post.poll;

        this.pollForm.patchValue({
          question: poll.question,
          duration: this.getDurationString(poll.expiresAt),
        });
        
        // Map privacy settings
        this.privacySetting = post.visibility === Visibility.Followers ? 'followers' : 'public';
        this.pollForm.patchValue({ privacy: this.privacySetting }); 
        
        // Clear default options
        while (this.options.length) {
          this.options.removeAt(0);
        }
        
        // Populate existing options
        const previews = poll.options.map(opt => opt.outfitThumbnail || null);
        this.optionPreviews.set(previews);
        
        const files = poll.options.map(() => null);
        this.optionFiles.set(files);
        
        poll.options.forEach((option) => {
          const optionGroup = this.fb.group({
            id: [option.id],
            description: [option.description || '', [Validators.required, Validators.minLength(1), Validators.maxLength(200)]],
            outfitId: [option.outfitId],
          });
          this.options.push(optionGroup);
        });

        // Handle tagged users if they exist in the post
        if (post.taggedUsers && post.taggedUsers.length > 0) {
          this.taggedUsers.set(post.taggedUsers);
        }
      },
      error: (err) => {
        console.error('Failed to load poll for editing', err);
      }
    });
  }

  private getDurationString(expiresAt: Date | string): string {
    const expiry = new Date(expiresAt);
    const now = new Date();
    const diffMs = expiry.getTime() - now.getTime();
    if (diffMs <= 0) return '24h';
    const diffDays = Math.round(diffMs / (1000 * 60 * 60 * 24));
    if (diffDays >= 7) return '7d';
    if (diffDays >= 3) return '3d';
    return '24h';
  }

  get options(): FormArray {
    return this.pollForm.get('options') as FormArray;
  }
  getexpirationDate(): Date {
    const durationValue = this.pollForm.get('duration')?.value || '7d';
    const now = new Date();
    const durationMatch = durationValue.match(/^(\d+)([smhd])$/);
    if (durationMatch) {
      const amount = parseInt(durationMatch[1], 10);
      const unit = durationMatch[2];
      switch (unit) {
        case 's':
          return new Date(now.getTime() + amount * 1000);
        case 'm':
          return new Date(now.getTime() + amount * 60 * 1000);
        case 'h':
          return new Date(now.getTime() + amount * 60 * 60 * 1000);
        case 'd':
          return new Date(now.getTime() + amount * 24 * 60 * 60 * 1000);
      }
    }
    // Default to 7 days if parsing fails
    return new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
  }

  addOption(): void {
    if (this.options.length >= this.maxOptions) {
      return;
    }

    const optionGroup = this.fb.group({
      description: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(200)]],
      outfitId: [null],
    });

    this.options.push(optionGroup);
    
    // Dynamically expand previews and files signals
    this.optionPreviews.update(prev => [...prev, null]);
    this.optionFiles.update(files => [...files, null]);
  }

  removeOption(index: number): void {
    if (this.options.length <= this.minOptions) {
      return;
    }
    this.options.removeAt(index);
    
    // Dynamically shrink previews and files signals
    this.optionPreviews.update(prev => {
      const copy = [...prev];
      copy.splice(index, 1);
      return copy;
    });
    this.optionFiles.update(files => {
      const copy = [...files];
      copy.splice(index, 1);
      return copy;
    });
  }

  canAddOption(): boolean {
    return this.options.length < this.maxOptions;
  }

  canRemoveOption(): boolean {
    return this.options.length > this.minOptions;
  }

  onFileSelectedForOption(event: Event, index: number): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      const files = [...this.optionFiles()];
      files[index] = file;
      this.optionFiles.set(files);

      const previews = [...this.optionPreviews()];
      const reader = new FileReader();
      reader.onload = (e) => {
        previews[index] = e.target?.result as string;
        this.optionPreviews.set(previews);
      };
      reader.readAsDataURL(file);
    }
  }

  removeImageFromOption(index: number): void {
    const files = [...this.optionFiles()];
    files[index] = null;
    this.optionFiles.set(files);

    const previews = [...this.optionPreviews()];
    previews[index] = null;
    this.optionPreviews.set(previews);

    const options = this.pollForm.get('options') as FormArray;
    const optionGroup = options.at(index) as FormGroup;
    optionGroup.patchValue({ outfitId: null });
  }

  getImagePreview(index: number): string | undefined {
    return this.optionPreviews()[index] ?? undefined;
  }

  isUploading(index: number): boolean {
    return false;
  }

  async onSubmit(): Promise<void> {
    if (this.pollForm.invalid) {
      this.markFormGroupTouched(this.pollForm);
      
      const invalidControls: string[] = [];
      if (this.pollForm.get('question')?.invalid) {
        invalidControls.push('Poll Question');
      }
      
      const optionsArray = this.options;
      for (let i = 0; i < optionsArray.length; i++) {
        const optGroup = optionsArray.at(i) as FormGroup;
        if (optGroup.get('description')?.invalid) {
          invalidControls.push(`Description for Option ${this.getOptionLetter(i)}`);
        }
      }
      Swal.fire({
        icon: 'error',
        title: 'Please fill out all required fields',
        text: invalidControls.join('\n- '),
      });
      return;
    }

    // Validate that all options have an image preview (uploaded or selected)
    let allOptionsHaveImage = true;
    for (let i = 0; i < this.options.length; i++) {
      if (!this.getImagePreview(i)) {
        allOptionsHaveImage = false;
        break;
      }
    }

    if (!allOptionsHaveImage) {
      alert('Please upload a photo for all poll options before publishing.');
      return;
    }

    this.isSubmitting.set(true);
    const formValue = this.pollForm.value;

    try {
      // Upload images for each option and create outfits, collecting the outfit IDs
      const outfitIds: string[] = [];
      for (let i = 0; i < formValue.options.length; i++) {
        const file = this.optionFiles()[i];
        if (file) {
          // Create outfit from the uploaded image
          const createdOutfit = await new Promise<any>((resolve, reject) => {
            this.outfitsUseCases.createOutfitWithImage(file).subscribe({
              next: resolve,
              error: reject,
            });
          });
          outfitIds[i] = createdOutfit.id;
        } else {
          outfitIds[i] = formValue.options[i].outfitId;
        }
      }

      if (this.isEditMode && this.pollId) {
        const request: UpdatePollRequest = {
          question: formValue.question,
          context: '',
          expiresAt: this.getexpirationDate().toISOString(),
          options: formValue.options.map((opt: any, index: number) => ({
            id: opt.id,
            outfitId: outfitIds[index],
            displayOrder: index + 1,
            description: opt.description,
          })),
          tags: this.taggedUsers().map(u => u.userName),
        };

        console.log('Updating poll with data:', request);

        this.pollUseCases.updatePoll(this.pollId, request).subscribe({
          next: () => {
            this.isSubmitting.set(false);
            this.router.navigate(['/social/polls', this.pollId]);
          },
          error: (err) => {
            console.error('Failed to update poll', err);
            this.isSubmitting.set(false);
          }
        });
      } else {
        const request: CreatePollRequest = {
          question: formValue.question,
          expiresAt: this.getexpirationDate(),
          context: '',
          visibility: this.privacySetting === 'public' ? Visibility.Public : this.privacySetting === 'followers' ? Visibility.Followers : Visibility.Private,
          options: formValue.options.map((opt: PollOptionForm, index: number) => ({
            description: opt.description,
            outfitId: outfitIds[index],
            displayOrder: index + 1,
          })),
          tags: this.taggedUsers().map(u => u.userName),
        };

        console.log('Creating poll with data:', request);

        this.pollUseCases.createPoll(request).subscribe({
          next: (response: any) => {
            this.pollId = response.id;
            this.isSubmitting.set(false);
               Swal.fire({
                      title: 'Success!',
                      text: 'Your poll has been created.',
                      icon: 'success',
                      timer: 2000,
                      showConfirmButton: false,
                    }).then(() => {
                      this.router.navigate(['/social/polls',this.pollId]);
                    })
          
          },
          error: (err) => {
            console.error('Failed to create poll', err);
            this.isSubmitting.set(false);
          }
        });
      }
    } catch (error) {
      console.error('Failed to upload outfits', error);
      this.isSubmitting.set(false);
    }
  }

  onCancel(): void {
    this.router.navigate(['/social']);
  }

  setPrivacy(privacy: 'public' | 'followers'): void {
    this.privacySetting = privacy;
    this.pollForm.get('privacy')?.setValue(privacy);
  }

  onSelectOutfit(optionIndex: number): void {
    this.router.navigate(['/outfits'], {
      queryParams: {
        selectForPoll: true,
        optionIndex: optionIndex,
      },
    });
  }

  private markFormGroupTouched(formGroup: FormGroup | FormArray): void {
    Object.values(formGroup.controls).forEach((control: AbstractControl) => {
      control.markAsTouched();
      if (control instanceof FormGroup || control instanceof FormArray) {
        this.markFormGroupTouched(control);
      }
    });
  }

  getOptionLetter(index: number): string {
    return String.fromCharCode(65 + index);
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

  private loadFollowers(search?: string): void {
    const userId = this.authService.currentUser()?.id;
    if (!userId) return;

    forkJoin({
      followers: this.followUseCases.getFollowers(userId, undefined, 20, search)
    }).subscribe({
      next: ({ followers }: any) => {
        const map = new Map<string, Follower>();
        
        if (followers.items) {
          followers.items.forEach((f: any) => {
            map.set(f.useId, f);
          });
        }
        this.followersList.set(Array.from(map.values()));
      },
      error: (err: any) => {
        console.error('Failed to load following list:', err);
      },
    });
  }

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
}
