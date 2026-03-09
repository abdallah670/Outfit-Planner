import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { Store } from '@ngrx/store';
import { SocialActions } from '../../../core/state/social/social.actions';

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
    MatDatepickerModule,
    MatNativeDateModule,
    MatCardModule,
    MatDividerModule,
  ],
  templateUrl: './create-poll.component.html',
  styleUrl: './create-poll.component.scss',
})
export class CreatePollComponent implements OnInit {
  pollForm!: FormGroup;
  minDate = new Date();
  maxOptions = 6;
  minOptions = 2;

  constructor(
    private fb: FormBuilder,
    private store: Store,
    private router: Router,
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
    date.setDate(date.getDate() + 7); // Default to 7 days from now
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

    this.store.dispatch(SocialActions.createPoll({ request }));
  }

  onCancel(): void {
    this.router.navigate(['/social']);
  }

  private markFormGroupTouched(formGroup: FormGroup | FormArray): void {
    Object.values(formGroup.controls).forEach((control) => {
      control.markAsTouched();
      if (control instanceof FormGroup || control instanceof FormArray) {
        this.markFormGroupTouched(control);
      }
    });
  }

  getOptionLetter(index: number): string {
    return String.fromCharCode(65 + index); // A, B, C, D, E, F
  }
}
