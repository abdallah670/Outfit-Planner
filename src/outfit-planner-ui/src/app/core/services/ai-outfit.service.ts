import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { OutfitItem } from '../../domain/entities/outfit.entity';
import { firstValueFrom } from 'rxjs';

/**
 * AI-powered outfit image generation service using Hugging Face
 * Creates attractive backgrounds and places clothing items as separate elements
 */
@Injectable({
  providedIn: 'root'
})
export class AiOutfitService {
  private http = inject(HttpClient);
  
  // Hugging Face API configuration
  private readonly hfApiKey = environment.huggingFaceApiKey;
  private readonly hfApiUrl = environment.huggingFaceApiUrl;
  
  // Model for text-to-image generation (background)
  private readonly backgroundModel = 'stabilityai/stable-diffusion-2-1-base';
  
  /**
   * Checks if Hugging Face API is configured
   * Currently disabled due to CORS issues when calling from browser
   * TODO: Enable when backend proxy is set up
   */
  get isConfigured(): boolean {
    // Temporarily disabled - Hugging Face API blocks cross-origin requests from localhost
    // To enable, set up a backend proxy or use a CORS proxy
    return false;
    // return !!this.hfApiKey && this.hfApiKey.length > 0;
  }

  /**
   * Generates an AI background and places clothing items as separate elements on it
   * @param items Array of outfit items with image URLs
   * @param outfitName Optional name for the outfit
   * @returns Promise<string> Base64 data URL of the final image
   */
  async generateAiCombinedImage(items: OutfitItem[], outfitName?: string): Promise<string> {
    if (!this.isConfigured) {
      throw new Error('Hugging Face API key not configured');
    }

    if (!items || items.length === 0) {
      throw new Error('No items to generate image from');
    }

    try {
      // Generate the flat-lay outfit image using AI
      const prompt = this.generateFlatLayPrompt(items, outfitName);
      console.log('Generating AI flat-lay with prompt:', prompt);
      
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${this.hfApiKey}`,
        'Content-Type': 'application/json'
      });
      
      const body = {
        inputs: prompt,
        parameters: {
          negative_prompt: 'blurry, low quality, distorted, deformed, text, watermark, logo, cartoon, illustration, 3d render',
          guidance_scale: 8,
          num_inference_steps: 50,
          width: 768,
          height: 768
        }
      };
      
      // Call Hugging Face API for image generation
      const response = await firstValueFrom(
        this.http.post(`${this.hfApiUrl}/${this.backgroundModel}`, body, {
          headers,
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
    } catch (error) {
      console.error('AI image generation failed:', error);
      throw error;
    }
  }

  /**
   * Generates a flat-lay prompt for the AI
   */
  private generateFlatLayPrompt(items: OutfitItem[], outfitName?: string): string {
    // Get item names from the outfit
    const itemNames = items.map(item => item.clothingItemName || 'clothing item').join(', ');

    // Create a detailed prompt for realistic flat-lay
    return `Create a realistic fashion flat-lay outfit image.

Combine the following clothing items into a single aesthetically pleasing outfit composition:
- ${itemNames}

Style requirements:
- Top-down flat lay photography
- Items arranged naturally as a complete outfit
- Each item folded neatly and positioned professionally
- Items not overlapping too much

Visual style:
- Minimal modern fashion catalog style
- Soft natural lighting
- Clean neutral background (light wood table or soft beige surface)
- Realistic shadows
- High resolution
- Instagram / Pinterest fashion aesthetic

Composition:
- Balanced layout
- Clothing pieces arranged as a complete outfit
- Looks like a professional fashion photoshoot

Output:
A single realistic outfit image showing the full look as if prepared for a fashion shoot.`;
  }

  /**
   * Creates a fallback gradient background
   */
  private createFallbackBackground(): string {
    const canvas = document.createElement('canvas');
    canvas.width = 800;
    canvas.height = 600;
    const ctx = canvas.getContext('2d');
    
    if (ctx) {
      // Create an elegant gradient
      const gradient = ctx.createLinearGradient(0, 0, 800, 600);
      gradient.addColorStop(0, '#f5f5f5');
      gradient.addColorStop(0.5, '#ffffff');
      gradient.addColorStop(1, '#e8e8e8');
      
      ctx.fillStyle = gradient;
      ctx.fillRect(0, 0, 800, 600);
      
      // Add subtle pattern
      ctx.fillStyle = 'rgba(0, 0, 0, 0.03)';
      for (let i = 0; i < 20; i++) {
        ctx.beginPath();
        ctx.arc(Math.random() * 800, Math.random() * 600, Math.random() * 50 + 20, 0, Math.PI * 2);
        ctx.fill();
      }
    }
    
    return canvas.toDataURL('image/png');
  }
}
