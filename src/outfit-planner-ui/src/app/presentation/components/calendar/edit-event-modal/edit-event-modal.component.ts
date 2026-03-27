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
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CalendarEventItem, CalendarEventType } from '../../../../domain/entities/wear-event.entity';

export interface EditEventModalData {
  event: CalendarEventItem;
}

@Component({
  selector: 'app-edit-event-modal',
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
    MatSnackBarModule,
  ],
  templateUrl: './edit-event-modal.component.html',
  styleUrl: './edit-event-modal.component.scss',
})
export class EditEventModalComponent implements OnInit {
  eventForm!: FormGroup;

  // Event types for dropdown
  eventTypes = [
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

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditEventModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EditEventModalData,
    private snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    const event = this.data.event;
    
    console.log('Edit event data:', event); // Debug log
    
    // Convert time from HH:mm:ss to HH:mm for the time input
    const startTime = this.formatTimeForInput(event.startTime);
    const endTime = this.formatTimeForInput(event.endTime);
    
    console.log('Formatted times:', { startTime, endTime }); // Debug log
    
    this.eventForm = this.fb.group({
      title: [event.title || '', [Validators.required, Validators.minLength(2)]],
      description: [event.description || ''],
      location: [event.location || ''],
      eventDate: [new Date(event.eventDate), Validators.required],
      startTime: [startTime],
      endTime: [endTime],
      eventType: [event.eventType ?? CalendarEventType.General, Validators.required],
      notes: [event.notes || ''],
    });
  }

  onSubmit(): void {
    if (this.eventForm.invalid) {
      this.markFormGroupTouched(this.eventForm);
      this.snackBar.open('Please fill in all required fields', 'Close', {
        duration: 3000,
        panelClass: 'error-snackbar',
      });
      return;
    }

    const formValue = this.eventForm.value;
    const updatedEvent = {
      id: this.data.event.id,
      title: formValue.title,
      description: formValue.description,
      location: formValue.location,
      eventDate: formValue.eventDate,
      startTime: this.formatTimeForApi(formValue.startTime) ?? undefined,
      endTime: this.formatTimeForApi(formValue.endTime) ?? undefined,
      eventType: formValue.eventType as CalendarEventType,
      notes: formValue.notes,
      isRecurring: this.data.event.isRecurring,
      hasOutfit: this.data.event.hasOutfit,
      outfitName: this.data.event.outfitName,
      outfitImageUrl: this.data.event.outfitImageUrl,
      wearEventId: this.data.event.wearEventId,
    };

    this.snackBar.open('Event updated successfully!', 'Close', {
      duration: 3000,
      panelClass: 'success-snackbar',
    });

    this.dialogRef.close({ success: true, event: updatedEvent });
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

  // Format time for input (HH:mm:ss → HH:mm)
  private formatTimeForInput(timeStr: string | undefined | null): string {
    if (!timeStr) return '';
    // If time has seconds (HH:mm:ss), truncate to HH:mm
    if (timeStr.length >= 8) {
      return timeStr.substring(0, 5);
    }
    // If already in HH:mm format, return as-is
    if (timeStr.length === 5 && timeStr.includes(':')) {
      return timeStr;
    }
    return '';
  }

  // Format time for API (HH:mm:ss string)
  private formatTimeForApi(timeStr: string): string | null {
    if (!timeStr) return null;
    if (timeStr.includes(':') && timeStr.split(':').length === 2) {
      return `${timeStr}:00`;
    }
    return timeStr;
  }
}
