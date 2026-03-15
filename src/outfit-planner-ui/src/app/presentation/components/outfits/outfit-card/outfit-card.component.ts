import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Outfit } from '../../../../domain/entities/outfit.entity';
import { OutfitCanvasService } from '../../../../core/services/outfit-canvas.service';

/**
 * Simplified outfit card component that relies on the backend
 * for AI guide-based image composition
 */
@Component({
  selector: 'app-outfit-card',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './outfit-card.component.html',
  styleUrl: './outfit-card.component.scss',
})
export class OutfitCardComponent implements OnInit {
  @Input({ required: true }) outfit!: Outfit;

  isGenerating = false;
  combinedImageUrl: string | null = null;
  errorMessage: string | null = null;
  imageError: boolean[] = []; // Track image load errors by index

  get hasMultipleItems(): boolean {
    return this.outfit.items && this.outfit.items.length > 1;
  }

  get showCombinedView(): boolean {
    return this._showCombinedView && this.hasMultipleItems;
  }

  private _showCombinedView = true;

  set showCombinedView(value: boolean) {
    this._showCombinedView = value;
  }

  constructor(private canvasService: OutfitCanvasService) {}

  ngOnInit() {
    // Use pre-saved imageUrl if available, otherwise try to generate on-demand
    if (this.outfit.imageUrl) {
      this.combinedImageUrl = this.outfit.imageUrl;
    } else if (this.hasMultipleItems) {
      this.generateCombinedImage();
    }
  }

  get itemThumbnailUrls(): string[] {
    return this.outfit.items
      .map((item) => item.clothingItemImageUrl)
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

  toggleView(event: Event): void {
    event.stopPropagation();
    this._showCombinedView = !this._showCombinedView;
  }

  /**
   * Generates the combined outfit image using the enhanced backend service
   * The backend now handles:
   * - Bounding box detection
   * - Auto-rotation of horizontal images
   * - Category-based scaling (shirts, pants, shoes have different sizes)
   * - Smart vertical layout (tops first, then bottoms, then footwear)
   */
  async generateCombinedImage(): Promise<void> {
    if (!this.hasMultipleItems || !this.outfit.items || this.outfit.items.length === 0) return;
    if (this.combinedImageUrl) return; // Already generated

    this.isGenerating = true;
    this.errorMessage = null;

    try {
      // Use the enhanced backend service that applies AI guide principles
      const result = await this.canvasService.getCombinedImageFromBackend(this.outfit.id);
      if (result) {
        this.combinedImageUrl = result;
      } else {
        // Images not available - will show item grid view instead
        this.errorMessage = 'Outfit preview not available - some item images are missing';
      }
    } catch (error) {
      console.error('Failed to generate combined image:', error);
      this.errorMessage = 'Failed to generate outfit preview';
    } finally {
      this.isGenerating = false;
    }
  }

  /**
   * Downloads the combined outfit image
   */
  async downloadCombinedImage(event: Event): Promise<void> {
    event.stopPropagation();

    if (this.isGenerating) return;

    // If image not generated yet, generate it first
    if (!this.combinedImageUrl) {
      await this.generateCombinedImage();
    }

    if (this.combinedImageUrl) {
      this.canvasService.downloadCombinedImage(this.combinedImageUrl, this.outfit.name);
    }
  }

  /**
   * Retry image generation if it failed
   */
  async retryGeneration(event: Event): Promise<void> {
    event.stopPropagation();
    this.combinedImageUrl = null;
    this.errorMessage = null;
    await this.generateCombinedImage();
  }

  /**
   * Handle image load error for a specific item thumbnail
   */
  onImageError(index: number): void {
    this.imageError[index] = true;
  }
}
