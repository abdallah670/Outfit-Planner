import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { take } from 'rxjs/operators';
import { PollsActions } from '../../../../core/state/polls/polls.actions';
import { selectSelectedPoll, selectPollsLoading } from '../../../../core/state/polls/polls.selectors';
import { Poll, UpdatePollRequest, CreatePollOptionRequest } from '../../../../domain/entities/poll.entity';

@Component({
  selector: 'app-edit-poll',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './edit-poll.component.html',
  styleUrls: ['./edit-poll.component.scss'],
})
export class EditPollComponent implements OnInit {
  private fb = inject(FormBuilder);
  private store = inject(Store);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  pollForm!: FormGroup;
  pollId!: string;
  loading$ = this.store.select(selectPollsLoading);
  originalPoll: Poll | null = null;

  ngOnInit(): void {
    this.pollId = this.route.snapshot.paramMap.get('id')!;
    this.initForm();
    this.loadPoll();
  }

  private initForm(): void {
    this.pollForm = this.fb.group({
      question: ['', [Validators.required, Validators.minLength(5)]],
      context: [''],
      expiresAt: [null, Validators.required],
      options: this.fb.array([]),
    });
  }

  private loadPoll(): void {
    this.store.dispatch(PollsActions.loadPollById({ id: this.pollId }));
    this.store.select(selectSelectedPoll)
      .pipe(take(1))
      .subscribe((poll) => {
        if (poll) {
          this.originalPoll = poll;
          this.populateForm(poll);
        }
      });
  }

  private populateForm(poll: Poll): void {
    this.pollForm.patchValue({
      question: poll.question,
      context: poll.context || '',
      expiresAt: poll.expiresAt,
    });

    // Clear existing options
    while (this.optionsArray.length) {
      this.optionsArray.removeAt(0);
    }

    // Add poll options
    poll.options.forEach((option) => {
      this.optionsArray.push(
        this.fb.group({
          id: [option.id],
        })
      );
    });
  }

  get optionsArray(): FormArray {
    return this.pollForm.get('options') as FormArray;
  }

  addOption(): void {
    this.optionsArray.push(
      this.fb.group({
        id: [null],
        description: ['', Validators.required],
      })
    );
  }

  removeOption(index: number): void {
    if (this.optionsArray.length > 2) {
      this.optionsArray.removeAt(index);
    }
  }

  onSubmit(): void {
    if (this.pollForm.valid && this.originalPoll) {
      const formValue = this.pollForm.value;
      
      const updateRequest: UpdatePollRequest = {
        question: formValue.question,
        context: formValue.context,
        expiresAt: formValue.expiresAt ? formValue.expiresAt.toISOString() : undefined,
        options: formValue.options.map((opt: any): CreatePollOptionRequest => ({
         
        })),
      };

      this.store.dispatch(
        PollsActions.updatePoll({
          pollId: this.pollId,
          request: updateRequest,
        })
      );

      // Navigate back after update
      this.router.navigate(['/social/my-polls']);
    }
  }

  onCancel(): void {
    this.router.navigate(['/social/my-polls']);
  }
}
