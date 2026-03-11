import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { Store } from '@ngrx/store';
import { CalendarActions } from '../../../../core/state/calendar/calendar.actions';
import { Outfit } from '../../../../domain/entities/outfit.entity';

export interface ScheduleOutfitModalData {
  date: Date;
  outfits: Outfit[];
}

@Component({
  selector: 'app-schedule-outfit-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
  ],
  templateUrl: './schedule-outfit-modal.component.html',
  styleUrl: './schedule-outfit-modal.component.scss',
})
export class ScheduleOutfitModalComponent implements OnInit {
  scheduleForm!: FormGroup;
  occasions = ['Casual', 'Work', 'Date Night', 'Party', 'Sport', 'Formal', 'Travel'];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ScheduleOutfitModalComponent>,
    private store: Store,
    @Inject(MAT_DIALOG_DATA) public data: ScheduleOutfitModalData,
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    this.scheduleForm = this.fb.group({
      outfitId: ['', Validators.required],
      scheduledDate: [this.data.date, Validators.required],
      occasion: [''],
      notes: [''],
    });
  }

  onSubmit(): void {
    if (this.scheduleForm.invalid) {
      this.markFormGroupTouched(this.scheduleForm);
      return;
    }

    const formValue = this.scheduleForm.value;
    
    this.store.dispatch(
      CalendarActions.scheduleOutfit({
        request: {
          outfitId: formValue.outfitId,
          scheduledDate: formValue.scheduledDate,
          occasion: formValue.occasion,
          notes: formValue.notes,
        },
      }),
    );

    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
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
