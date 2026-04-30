import { Component, OnInit, signal, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Store } from '@ngrx/store';
import { PollsActions } from '../../../core/state/polls/polls.actions';
import { POLLS_REPOSITORY, PollsRepository } from '../../../domain/repositories/polls.repository';

interface PollOptionForm {
  description: string;
  outfitId?: string;
  imageFile?: File;
  imagePreview?: string;
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
    MatDatepickerModule,
    MatNativeDateModule,
    MatCardModule,
    MatDividerModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './create-poll.component.html',
  styleUrl: './create-poll.component.scss',
})
export class CreatePollComponent implements OnInit {
  pollForm!: FormGroup;
  minDate = new Date();
  maxOptions = 6;
  minOptions = 2;

  // Track uploading state for each option (index -> boolean)
  uploadingOptions = signal<Set<number>>(new Set());

  // Store uploaded image URLs for each option
  uploadedImages = signal<Map<number, string>>(new Map());

  constructor(
    private fb: FormBuilder,
    private store: Store,
    private router: Router,
    @Inject(POLLS_REPOSITORY) private pollsRepository: PollsRepository,
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
      context: [''],
      expiresAt: [this.getDefaultExpiryDate(), Validators.required],
      options: this.fb.array([], [Validators.required, Validators.minLength(this.minOptions)]),
    });
  }

  private getDefaultExpiryDate(): Date {
    const date = new Date();
    date.setDate(date.getDate() + 7);
    return date;
  }

  get options(): FormArray {
    return this.pollForm.get('options') as FormArray;
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

  onSubmit(): void {
    if (this.pollForm.invalid) {
      this.markFormGroupTouched(this.pollForm);
      return;
    }

    const formValue = this.pollForm.value;

    const request = {
      question: formValue.question,
      context: formValue.context,
      expiresAt: formValue.expiresAt,
      options: formValue.options.map((opt: PollOptionForm, index: number) => ({
        description: opt.description,
        outfitId: opt.outfitId,
        displayOrder: index + 1,
      })),
    };

    this.store.dispatch(PollsActions.createPoll({ request }));
  }

  onCancel(): void {
    this.router.navigate(['/social']);
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

  /**
   * Handle image file selection for a poll option
   */
  onImageSelected(event: Event, optionIndex: number): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];

    if (!file.type.startsWith('image/')) {
      console.error('Please select an image file');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      console.error('File size must be less than 5MB');
      return;
    }

    const currentUploading = this.uploadingOptions();
    currentUploading.add(optionIndex);
    this.uploadingOptions.set(new Set(currentUploading));

    const reader = new FileReader();
    reader.onload = (e) => {
      const currentImages = this.uploadedImages();
      currentImages.set(optionIndex, (e.target as FileReader).result as string);
      this.uploadedImages.set(new Map(currentImages));
    };
    reader.readAsDataURL(file);

    this.pollsRepository.uploadPollImage(file).subscribe({
      next: (imageUrl: string) => {
        const currentImages = this.uploadedImages();
        currentImages.set(optionIndex, imageUrl);
        this.uploadedImages.set(new Map(currentImages));

        const currentUploading = this.uploadingOptions();
        currentUploading.delete(optionIndex);
        this.uploadingOptions.set(new Set(currentUploading));

        const options = this.pollForm.get('options') as FormArray;
        const optionGroup = options.at(optionIndex) as FormGroup;
        optionGroup.patchValue({ outfitId: imageUrl });
      },
      error: () => {
        const currentUploading = this.uploadingOptions();
        currentUploading.delete(optionIndex);
        this.uploadingOptions.set(new Set(currentUploading));
      },
    });
  }

  /**
   * Select an existing outfit for this poll option
   */
  onSelectOutfit(optionIndex: number): void {
    this.router.navigate(['/outfits'], {
      queryParams: {
        selectForPoll: true,
        optionIndex: optionIndex,
      },
    });
  }

  /**
   * Check if an option is currently uploading
   */
  isUploading(optionIndex: number): boolean {
    return this.uploadingOptions().has(optionIndex);
  }

  /**
   * Get image preview for an option
   */
  getImagePreview(optionIndex: number): string | undefined {
    return this.uploadedImages().get(optionIndex);
  }

  /**
   * Remove uploaded image from an option
   */
  removeImage(optionIndex: number): void {
    const currentImages = this.uploadedImages();
    currentImages.delete(optionIndex);
    this.uploadedImages.set(new Map(currentImages));

    const options = this.pollForm.get('options') as FormArray;
    const optionGroup = options.at(optionIndex) as FormGroup;
    optionGroup.patchValue({ outfitId: null });
  }
}
