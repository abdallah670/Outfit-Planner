import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { OutfitItem } from '../../domain/entities/outfit.entity';
import { environment } from '../../../environments/environment';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OutfitCanvasService {
  private http = inject(HttpClient);
  private readonly apiBaseUrl = environment.resourceBaseUrl || 'http://localhost:5000';
  
  /**
   * Gets combined outfit image from backend (server-side image combination)
   * This uses the .NET backend with ImageSharp for reliable image processing
   * @param outfitId The ID of the outfit
   * @returns Promise<string> Base64 data URL of combined image, or null if not available
   */
  async getCombinedImageFromBackend(outfitId: string): Promise<string | null> {
    try {
      const response = await firstValueFrom(
        this.http.get(`${this.apiBaseUrl}/api/outfits/${outfitId}/combined-image`, {
          responseType: 'blob'
        })
      );
      
      // Convert blob to base64
      return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result as string);
        reader.onerror = reject;
        reader.readAsDataURL(response as Blob);
      });
    } catch (error: any) {
      // Handle 404 - images not available (expected when clothing items have no images)
      if (error.status === 404) {
        console.warn(`Combined image not available for outfit ${outfitId}:`, error.error || 'Images missing');
        return null;
      }
      console.error('Failed to get combined image from backend:', error);
      throw error;
    }
  }
  
  /**
   * Combines multiple clothing item images into a single canvas image (client-side fallback)
   * @param items Array of outfit items with image URLs
   * @returns Promise<string> Base64 data URL of combined image
   */
  async combineOutfitImages(items: OutfitItem[]): Promise<string> {
    if (!items || items.length === 0) {
      throw new Error('No items to combine');
    }

    // Sort items by layeringOrder
    const sortedItems = [...items].sort((a, b) => a.layeringOrder - b.layeringOrder);
    
    // Generate a single timestamp for cache-busting (used for all images in this batch)
    const timestamp = Date.now();
    
    // Create canvas
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    
    if (!ctx) {
      throw new Error('Failed to get canvas context');
    }
    
    // Canvas dimensions
    const canvasWidth = 400;
    const itemHeight = Math.floor(600 / sortedItems.length); // Distribute height evenly
    
    canvas.width = canvasWidth;
    canvas.height = itemHeight * sortedItems.length;
    
    // Fill with white background
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // Add outfit name as header
    ctx.fillStyle = '#333333';
    ctx.font = 'bold 24px Arial';
    ctx.textAlign = 'center';
    ctx.fillText('Outfit Preview', canvasWidth / 2, 35);
    
    // Load and draw all images (using HTTP to bypass CORS)
    const imageDataUrls = await Promise.all(
      sortedItems.map(item => this.loadImageAsDataUrl(item.clothingItemImageUrl, timestamp))
    );
    
    // Convert data URLs to images for canvas drawing
    const images: HTMLImageElement[] = await Promise.all(
      imageDataUrls.map(dataUrl => {
        return new Promise<HTMLImageElement>((resolve) => {
          const img = new Image();
          img.onload = () => resolve(img);
          img.onerror = () => {
            // Create a placeholder for failed images
            const placeholder = new Image();
            placeholder.src = this.createPlaceholderDataUrl('Failed');
            resolve(placeholder);
          };
          img.src = dataUrl;
        });
      })
    );

    // Calculate proper heights for each item (primary gets more space)
    let yOffset = 60; // Start after header
    
    for (let i = 0; i < sortedItems.length; i++) {
      const img = images[i];
      const item = sortedItems[i];
      
      // Calculate height based on role
      let height = itemHeight - 25;
      if (item.role === 'primary') {
        height = Math.floor(itemHeight * 1.3);
      } else if (item.role === 'secondary') {
        height = Math.floor(itemHeight * 1.1);
      }
      
      // Draw image centered horizontally
      const imgWidth = Math.min(canvasWidth - 20, img.width);
      const imgHeight = Math.min(height, img.height);
      const x = (canvasWidth - imgWidth) / 2;
      
      // Use drawImage with object-fit: cover behavior
      this.drawImageCover(ctx, img, x, yOffset, imgWidth, imgHeight);
      
      // Add small label for the item
      ctx.fillStyle = '#666666';
      ctx.font = '14px Arial';
      ctx.fillText(item.clothingItemName || `Item ${i + 1}`, canvasWidth / 2, yOffset + imgHeight + 15);
      
      yOffset += height;
    }
    
    // Add footer with date
    ctx.fillStyle = '#999999';
    ctx.font = '12px Arial';
    ctx.fillText(`Generated: ${new Date().toLocaleDateString()}`, canvasWidth / 2, canvas.height - 10);
    
    return canvas.toDataURL('image/jpeg', 0.9);
  }
  
  /**
   * Downloads the combined outfit image
   */
  downloadCombinedImage(dataUrl: string, filename: string = 'outfit'): void {
    const link = document.createElement('a');
    link.download = `${filename}-${Date.now()}.jpg`;
    link.href = dataUrl;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
  
  /**
   * Loads an image from URL using HTTP client (bypasses CORS for same-origin)
   */
  private async loadImageAsDataUrl(url: string | null | undefined, timestamp?: number): Promise<string> {
    if (!url) {
      return this.createPlaceholderDataUrl('No image');
    }
    
    let fullUrl = this.getFullImageUrl(url, timestamp);
    
    if (!fullUrl) {
      return this.createPlaceholderDataUrl('No image');
    }
    
    try {
      // Fetch as blob (this works with CORS for same-origin)
      const response = await firstValueFrom(
        this.http.get(fullUrl, { responseType: 'blob' })
      );
      
      // Convert blob to base64
      return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result as string);
        reader.onerror = () => reject(new Error('Failed to read image'));
        reader.readAsDataURL(response);
      });
    } catch (error) {
      console.error('Failed to load image:', fullUrl, error);
      return this.createPlaceholderDataUrl('Image not available');
    }
  }
  
  /**
   * Creates a placeholder data URL
   */
  private createPlaceholderDataUrl(text: string): string {
    const canvas = document.createElement('canvas');
    canvas.width = 200;
    canvas.height = 200;
    const ctx = canvas.getContext('2d');
    if (ctx) {
      ctx.fillStyle = '#e0e0e0';
      ctx.fillRect(0, 0, 200, 200);
      ctx.fillStyle = '#999999';
      ctx.font = '16px Arial';
      ctx.textAlign = 'center';
      ctx.fillText(text, 100, 100);
    }
    return canvas.toDataURL();
  }
  
  /**
   * Creates a placeholder canvas image with text (returns Promise)
   */
  private createPlaceholder(text: string): Promise<HTMLImageElement> {
    return new Promise((resolve) => {
      const placeholder = document.createElement('canvas');
      placeholder.width = 200;
      placeholder.height = 200;
      const ctx = placeholder.getContext('2d');
      if (ctx) {
        ctx.fillStyle = '#e0e0e0';
        ctx.fillRect(0, 0, 200, 200);
        ctx.fillStyle = '#999999';
        ctx.font = '16px Arial';
        ctx.textAlign = 'center';
        ctx.fillText(text, 100, 100);
      }
      const placeholderImg = new Image();
      placeholderImg.onload = () => resolve(placeholderImg);
      placeholderImg.src = placeholder.toDataURL();
    });
  }
  
  /**
   * Gets the full URL for an image (handles relative URLs)
   */
  private getFullImageUrl(url: string | null | undefined, timestamp?: number): string {
    if (!url) return '';
    let fullUrl: string;
    if (url.startsWith('http://') || url.startsWith('https://')) {
      fullUrl = url;
    } else {
      fullUrl = `${this.apiBaseUrl}${url}`;
    }
    // Add cache-busting query parameter to avoid CORS issues with cached responses
    const ts = timestamp || Date.now();
    const separator = fullUrl.includes('?') ? '&' : '?';
    return `${fullUrl}${separator}_t=${ts}`;
  }
  
  /**
   * Draws an image with object-fit: cover behavior
   */
  private drawImageCover(
    ctx: CanvasRenderingContext2D,
    img: HTMLImageElement,
    x: number,
    y: number,
    width: number,
    height: number
  ): void {
    const imgRatio = img.width / img.height;
    const targetRatio = width / height;
    
    let sx = 0, sy = 0, sWidth = img.width, sHeight = img.height;
    
    if (imgRatio > targetRatio) {
      // Image is wider than container - crop sides
      sWidth = img.height * targetRatio;
      sx = (img.width - sWidth) / 2;
    } else {
      // Image is taller than container - crop top/bottom
      sHeight = img.width / targetRatio;
      sy = (img.height - sHeight) / 2;
    }
    
    ctx.drawImage(img, sx, sy, sWidth, sHeight, x, y, width, height);
  }
}
