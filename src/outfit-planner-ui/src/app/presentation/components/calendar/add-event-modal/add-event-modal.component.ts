import { Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { Store } from '@ngrx/store';
import { CalendarActions } from '../../../../core/state/calendar/calendar.actions';
import { Outfit } from '../../../../domain/entities/outfit.entity';
import { CalendarEventType, CreateCalendarEventRequest } from '../../../../domain/entities/wear-event.entity';

export interface AddEventModalData {
  date: Date;
  outfits: Outfit[];
}

@Component({
  selector: 'app-add-event-modal',
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
    MatIconModule,
    MatCheckboxModule,
  ],
  templateUrl: './add-event-modal.component.html',
  styleUrl: './add-event-modal.component.scss',
})
export class AddEventModalComponent implements OnInit {
  eventForm!: FormGroup;
  
  // Event types for dropdown - map to enum values
  eventTypeOptions = [
    { value: CalendarEventType.General, label: 'General' },
    { value: CalendarEventType.Work, label: 'Work' },
    { value: CalendarEventType.Meeting, label: 'Meeting' },
    { value: CalendarEventType.Social, label: 'Social' },
    { value: CalendarEventType.Date, label: 'Date' },
    { value: CalendarEventType.Party, label: 'Party' },
    { value: CalendarEventType.Sport, label: 'Sport' },
    { value: CalendarEventType.Travel, label: 'Travel' },
    { value: CalendarEventType.Appointment, label: 'Appointment' },
  ];
  
  // Toggle for outfit association
  associateWithOutfit = signal(false);
  
  // Selected outfit
  selectedOutfitId = signal<string | null>(null);

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AddEventModalComponent>,
    private store: Store,
    @Inject(MAT_DIALOG_DATA) public data: AddEventModalData,
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    // Set default times: start = current time, end = 1 hour later
    const now = new Date();
    const oneHourLater = new Date(now.getTime() + 60 * 60 * 1000);
    const startTimeStr = this.formatTime(now);
    const endTimeStr = this.formatTime(oneHourLater);

    this.eventForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(2)]],
      description: [''],
      location: [''],
      eventDate: [this.data.date, Validators.required],
      startTime: [startTimeStr],
      endTime: [endTimeStr],
      eventType: [CalendarEventType.General, Validators.required],
      notes: [''],
      associateOutfit: [false],
      outfitId: [null],
    });

    // Listen for associate outfit checkbox changes
    this.eventForm.get('associateOutfit')?.valueChanges.subscribe((value: boolean) => {
      this.associateWithOutfit.set(value);
      if (!value) {
        this.selectedOutfitId.set(null);
        this.eventForm.get('outfitId')?.setValue(null);
      }
    });

    // Listen for outfit selection changes
    this.eventForm.get('outfitId')?.valueChanges.subscribe((value: string | null) => {
      this.selectedOutfitId.set(value);
    });
  }

  // Select outfit
  selectOutfit(outfitId: string): void {
    this.selectedOutfitId.set(outfitId);
    this.eventForm.get('outfitId')?.setValue(outfitId);
  }

  isOutfitSelected(outfitId: string): boolean {
    return this.selectedOutfitId() === outfitId;
  }

  // Get selected outfit name
  getSelectedOutfitName(): string {
    const outfit = this.data.outfits.find(o => o.id === this.selectedOutfitId());
    return outfit?.name || '';
  }

  // Submit handler
  onSubmit(): void {
    if (this.eventForm.invalid) {
      this.markFormGroupTouched(this.eventForm);
      return;
    }

    const formValue = this.eventForm.value;
    
    const request: CreateCalendarEventRequest = {
      title: formValue.title,
      description: formValue.description,
      location: formValue.location,
      eventDate: formValue.eventDate,
      startTime: formValue.startTime,
      endTime: formValue.endTime,
      eventType: formValue.eventType as CalendarEventType,
      outfitId: formValue.associateOutfit ? formValue.outfitId : undefined,
      notes: formValue.notes,
    };

    this.store.dispatch(CalendarActions.createCalendarEvent({ event: request }));

    this.dialogRef.close({ success: true, event: request });
  }

  onCancel(): void {
    this.dialogRef.close({ success: false });
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach((control) => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  // Helper getters for template
  get canSubmit(): boolean {
    const formValid = this.eventForm.valid;
    if (this.associateWithOutfit()) {
      return formValid && !!this.selectedOutfitId();
    }
    return formValid;
  }

  // Format time for input type="time"
  private formatTime(date: Date): string {
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
  }
}
