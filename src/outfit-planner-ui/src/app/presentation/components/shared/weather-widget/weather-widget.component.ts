import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-weather-widget',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="weather-widget glass-card">
      <div class="header">
        <span class="location">San Francisco</span>
        <i class="fa-solid fa-cloud-sun weather-icon"></i>
      </div>
      <div class="temp-display">
        <span class="temp">22°C</span>
        <div class="condition-info">
          <span class="condition">Partly Cloudy</span>
          <span class="feels-like">Feels like 24°</span>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .weather-widget {
        padding: 1.5rem;
        background: var(--glass-bg);
        border: 1px solid var(--glass-border);
        border-radius: var(--radius-lg);
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }

      .header {
        display: flex;
        justify-content: space-between;
        align-items: center;

        .location {
          font-size: 0.9rem;
          color: var(--text-secondary);
          font-weight: 500;
        }

        .weather-icon {
          color: var(--accent-secondary);
          font-size: 2.25rem;
          display: flex;
          align-items: center;
          justify-content: center;
        }
      }

      .temp-display {
        .temp {
          font-size: 3rem;
          font-weight: 700;
          line-height: 1;
          display: block;
        }

        .condition-info {
          display: flex;
          flex-direction: column;
          margin-top: 0.25rem;

          .condition {
            font-size: 0.95rem;
            color: var(--text-primary);
          }

          .feels-like {
            font-size: 0.85rem;
            color: var(--text-dim);
          }
        }
      }
    `,
  ],
})
export class WeatherWidgetComponent {}
