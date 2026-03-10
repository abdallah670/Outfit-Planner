import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ClothingItem } from '../../../domain/entities/clothing-item.entity';

@Component({
  selector: 'app-clothing-card',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './clothing-card.component.html',
  styleUrl: './clothing-card.component.scss',
})
export class ClothingCardComponent {
  @Input({ required: true }) item!: ClothingItem;
  @Output() delete = new EventEmitter<string>();
  @Output() recordWear = new EventEmitter<string>();

  imageError = false;

  onDelete(event: Event) {
    event.stopPropagation();
    this.delete.emit(this.item.id);
  }

  onImageError(): void {
    this.imageError = true;
  }

  get categoryEmoji(): string {
    const type = this.item?.type?.toLowerCase() || '';
    if (type.includes('top') || type.includes('shirt')) return '👕';
    if (type.includes('bottom') || type.includes('pant')) return '👖';
    if (type.includes('dress')) return '👗';
    if (type.includes('outer') || type.includes('jacket')) return '🧥';
    if (type.includes('shoe') || type.includes('foot')) return '👟';
    if (type.includes('access')) return '👜';
    return '👔';
  }

  get colorBackground(): string {
    const color = this.item?.primaryColor?.toLowerCase() || '';
    const colorMap: Record<string, string> = {
      black: '#374151',
      white: '#f3f4f6',
      blue: '#bfdbfe',
      red: '#fecaca',
      green: '#bbf7d0',
      pink: '#fce7f3',
      beige: '#fef3c7',
      brown: '#d6b78a',
      gray: '#e5e7eb',
      grey: '#e5e7eb',
      yellow: '#fef9c3',
      purple: '#e9d5ff',
      orange: '#fed7aa',
      navy: '#93c5fd',
    };
    return colorMap[color] || '#f3f4f6';
  }

  onRecordWear(event: Event) {
    event.stopPropagation();
    this.recordWear.emit(this.item.id);
  }
}
