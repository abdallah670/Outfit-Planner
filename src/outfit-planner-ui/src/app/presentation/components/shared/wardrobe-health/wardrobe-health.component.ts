import { Component, Input, computed, Signal, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ClothingItem } from '../../../../domain/entities/clothing-item.entity';

@Component({
  selector: 'app-wardrobe-health',
  standalone: true,
  imports: [CommonModule, MatProgressBarModule],
  templateUrl: './wardrobe-health.component.html',
  styleUrls: ['./wardrobe-health.component.scss'],
})
export class WardrobeHealthComponent {
  items = signal<ClothingItem[]>([]);

  @Input() set itemsInput(value: ClothingItem[]) {
    this.items.set(value || []);
  }

  // Percentage of items worn at least once
  healthPercentage = computed(() => {
    const items = this.items();
    if (items.length === 0) return 0;

    const wornItems = items.filter((item) => (item.wearCount || 0) > 0);
    return Math.round((wornItems.length / items.length) * 100);
  });
}
