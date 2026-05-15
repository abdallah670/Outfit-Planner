import { Component, OnInit, Inject, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
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
import {  FeedPost, PostType, Visibility } from '../../../../domain/entities/feed.entity';
import { FeedUseCases } from '../../../../domain/usecases/feed.usecases';
import { CreatePollRequest, PollStatus } from '../../../../domain/entities/poll.entity';
import { OutfitsActions } from '../../../../core/state/outfit/outfit.actions';
import { OutfitsUseCases } from '../../../../domain/usecases/outfit.usecases';
import { PollsUseCases } from '../../../../domain/usecases/polls.usecases';

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
})
export class CreatePollComponent implements OnInit {
  pollForm!: FormGroup;
  maxOptions = 6;
  minOptions = 2;
  privacySetting: 'public' | 'followers' = 'public';
  private pollUseCases = inject(PollsUseCases);
  outfitsUseCases = inject(OutfitsUseCases);

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
    // Add initial 2 options
    this.addOption();
    this.addOption();
  }

  private initForm(): void {
    this.pollForm = this.fb.group({
      question: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(500)]],
      duration: ['7d', Validators.required],
      privacy: ['public', Validators.required],
      options: this.fb.array([], [Validators.required, Validators.minLength(this.minOptions)]),
    });
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
  }

  removeOption(index: number): void {
    if (this.options.length <= this.minOptions) {
      return;
    }
    this.options.removeAt(index);
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

      const request: CreatePollRequest = {
        question: formValue.question,
        expiresAt: this.getexpirationDate(),
        context: '',
        visibility: this.privacySetting === 'public' ? Visibility.Public : this.privacySetting === 'followers' ? Visibility.FriendsOnly : Visibility.Private,
        options: formValue.options.map((opt: PollOptionForm, index: number) => ({
          description: opt.description,
          outfitId: outfitIds[index],
          displayOrder: index + 1,
        })),
      };

      console.log('Creating poll with data:', request);

      this.pollUseCases.createPoll(request).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.router.navigate(['/social']);
        },
        error: (err) => {
          console.error('Failed to create poll', err);
          this.isSubmitting.set(false);
        }
      });
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
}
