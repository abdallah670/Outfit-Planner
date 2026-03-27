import { Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CalendarEventItem, CalendarEventType } from '../../../../domain/entities/wear-event.entity';

export interface EventDetailsModalData {
  event: CalendarEventItem;
  onEdit?: (event: CalendarEventItem) => void;
  onDelete?: (eventId: string) => void;
}

@Component({
  selector: 'app-event-details-modal',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './event-details-modal.component.html',
  styleUrl: './event-details-modal.component.scss'
})
export class EventDetailsModalComponent implements OnInit {
  event = signal<CalendarEventItem | null>(null);
  loading = signal(false);

  // Event type labels
  eventTypeLabels: Record<CalendarEventType, string> = {
    [CalendarEventType.General]: 'General',
    [CalendarEventType.Work]: 'Work',
    [CalendarEventType.Meeting]: 'Meeting',
    [CalendarEventType.Social]: 'Social',
    [CalendarEventType.Date]: 'Date',
    [CalendarEventType.Party]: 'Party',
    [CalendarEventType.Sport]: 'Sport',
    [CalendarEventType.Travel]: 'Travel',
    [CalendarEventType.Appointment]: 'Appointment'
  };

  // Event type icons
  eventTypeIcons: Record<CalendarEventType, string> = {
    [CalendarEventType.General]: 'event',
    [CalendarEventType.Work]: 'work',
    [CalendarEventType.Meeting]: 'groups',
    [CalendarEventType.Social]: 'celebration',
    [CalendarEventType.Date]: 'favorite',
    [CalendarEventType.Party]: 'cake',
    [CalendarEventType.Sport]: 'sports',
    [CalendarEventType.Travel]: 'flight',
    [CalendarEventType.Appointment]: 'schedule'
  };

  constructor(
    private dialogRef: MatDialogRef<EventDetailsModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EventDetailsModalData
  ) {}

  ngOnInit(): void {
    this.event.set(this.data.event);
  }

  getEventTypeLabel(type: CalendarEventType): string {
    return this.eventTypeLabels[type] || 'General';
  }

  getEventTypeIcon(type: CalendarEventType): string {
    return this.eventTypeIcons[type] || 'event';
  }

  getFormattedDate(date: Date): string {
    return new Date(date).toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  getFormattedTime(): string {
    const event = this.event();
    if (!event) return '';
    
    const start = event.startTimeDisplay || event.startTime;
    const end = event.endTimeDisplay || event.endTime;

    if (start && end) {
      return `${start} - ${end}`;
    } else if (start) {
      return `Starts at ${start}`;
    } else if (end) {
      return `Ends at ${end}`;
    }
    return 'All day';
  }

  onEdit(): void {
    const event = this.event();
    if (event && this.data.onEdit) {
      this.data.onEdit(event);
    }
    this.dialogRef.close({ action: 'edit', event });
  }

  onDelete(): void {
    const event = this.event();
    if (event && this.data.onDelete) {
      this.data.onDelete(event.id);
    }
    this.dialogRef.close({ action: 'delete', eventId: event?.id });
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
