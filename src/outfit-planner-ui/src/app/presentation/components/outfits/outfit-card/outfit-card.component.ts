import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { Outfit } from '../../../../domain/entities/outfit.entity';

@Component({
  selector: 'app-outfit-card',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatChipsModule, MatIconModule],
  templateUrl: './outfit-card.component.html',
  styleUrl: './outfit-card.component.scss',
})
export class OutfitCardComponent {
  @Input({ required: true }) outfit!: Outfit;

  get itemThumbnailUrls(): string[] {
    return this.outfit.items
      .map((item) => (item as any).clothingItem?.imageUrl) // Assuming clothingItem is populated or we'll need to fetch it
      .filter((url) => !!url)
      .slice(0, 4);
  }

  get occasionEmoji(): string {
    const occasion = this.outfit.occasion?.toLowerCase() || '';
    if (occasion.includes('formal')) return '👔';
    if (occasion.includes('work')) return '💼';
    if (occasion.includes('athletic')) return '🏃';
    if (occasion.includes('social')) return '🥂';
    if (occasion.includes('date')) return '❤️';
    if (occasion.includes('travel')) return '✈️';
    return '👕';
  }
}
