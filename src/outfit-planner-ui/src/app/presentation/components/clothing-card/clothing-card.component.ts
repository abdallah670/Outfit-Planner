import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-clothing-card',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
  ],
  templateUrl: './clothing-card.component.html',
  styleUrl: './clothing-card.component.scss',
})
export class ClothingCardComponent {
  @Input({ required: true }) item!: ClothingItem;
  @Output() delete = new EventEmitter<string>();
  @Output() recordWear = new EventEmitter<string>();

  resourceBaseUrl = environment.resourceBaseUrl;

  onDelete(event: Event) {
    event.stopPropagation();
    this.delete.emit(this.item.id);
  }

  onRecordWear(event: Event) {
    event.stopPropagation();
    this.recordWear.emit(this.item.id);
  }
}
