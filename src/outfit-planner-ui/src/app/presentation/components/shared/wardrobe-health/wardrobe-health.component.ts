import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'app-wardrobe-health',
  standalone: true,
  imports: [CommonModule, MatProgressBarModule],
  template: `
    <div class="health-widget glass-card">
      <h3 class="title">Wardrobe Health</h3>
      <div class="metrics">
        <div class="metric-row">
          <span class="label">Items Worn</span>
          <span class="value">68%</span>
        </div>
        <mat-progress-bar mode="determinate" value="68" class="health-bar"></mat-progress-bar>
      </div>
    </div>
  `,
  styles: [
    `
      .health-widget {
        padding: 1.5rem;
        background: var(--glass-bg);
        border: 1px solid var(--glass-border);
        border-radius: var(--radius-lg);
      }

      .title {
        margin: 0 0 1rem 0;
        font-size: 1rem;
        font-weight: 600;
        color: var(--text-secondary);
      }

      .metric-row {
        display: flex;
        justify-content: space-between;
        margin-bottom: 0.5rem;

        .label {
          font-size: 0.9rem;
          color: var(--text-primary);
        }

        .value {
          font-size: 0.9rem;
          font-weight: 700;
          color: var(--accent-vibrant);
        }
      }

      .health-bar {
        height: 8px;
        border-radius: 4px;

        ::ng-deep .mat-progress-bar-fill::after {
          background-color: var(--accent-vibrant);
        }

        ::ng-deep .mat-progress-bar-buffer {
          background-color: rgba(255, 255, 255, 0.05);
        }
      }
    `,
  ],
})
export class WardrobeHealthComponent {}
