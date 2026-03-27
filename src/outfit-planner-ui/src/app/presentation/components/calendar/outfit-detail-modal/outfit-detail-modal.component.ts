import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';
import { CalendarActions } from '../../../../core/state/calendar/calendar.actions';
import { CalendarEvent } from '../../../../domain/entities/wear-event.entity';

export interface OutfitDetailModalData {
  event: CalendarEvent;
}

@Component({
  selector: 'app-outfit-detail-modal',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  templateUrl: './outfit-detail-modal.component.html',
  styleUrl: './outfit-detail-modal.component.scss',
})
export class OutfitDetailModalComponent {
  constructor(
    private dialogRef: MatDialogRef<OutfitDetailModalComponent>,
    private store: Store,
    @Inject(MAT_DIALOG_DATA) public data: OutfitDetailModalData,
  ) {}

  onMarkAsWorn(): void {
    this.store.dispatch(
      CalendarActions.markAsWorn({ eventId: this.data.event.id }),
    );
    this.dialogRef.close({ action: 'mark-worn' });
  }

  onDelete(): void {
    this.store.dispatch(
      CalendarActions.deleteWearEvent({ eventId: this.data.event.id }),
    );
    this.dialogRef.close({ action: 'delete' });
  }

  onClose(): void {
    this.dialogRef.close();
  }

  handleImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = '/outfit_placeholder.png';
  }
}
