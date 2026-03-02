import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'app-wardrobe-health',
  standalone: true,
  imports: [CommonModule, MatProgressBarModule],
  templateUrl: './wardrobe-health.component.html',
  styleUrls: ['./wardrobe-health.component.scss'],
})
export class WardrobeHealthComponent {}
