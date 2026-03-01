import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Weather } from '../../../domain/entities/weather.entity';

@Component({
  selector: 'app-weather-display',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="weather-display" *ngIf="weather">
      <div class="weather-header">
        <span class="city-name">{{ weather.city }}</span>
        <img *ngIf="iconUrl" [src]="iconUrl" [alt]="weather.condition" class="weather-icon" />
      </div>

      <div class="weather-main">
        <span class="temperature">{{ weather.temperature | number: '1.0-0' }}°</span>
        <span class="condition">{{ weather.description }}</span>
      </div>

      <div class="weather-details">
        <div class="detail-item">
          <mat-icon>water_drop</mat-icon>
          <span>{{ weather.humidity }}%</span>
        </div>
        <div class="detail-item">
          <mat-icon>air</mat-icon>
          <span>{{ weather.windSpeed | number: '1.0-1' }} m/s</span>
        </div>
        <div class="detail-item" *ngIf="showFeelsLike">
          <mat-icon>thermostat</mat-icon>
          <span>Feels {{ weather.feelsLike | number: '1.0-0' }}°</span>
        </div>
      </div>

      <div class="temp-range" *ngIf="showTempRange">
        <span class="high">H: {{ weather.highTemp | number: '1.0-0' }}°</span>
        <span class="low">L: {{ weather.lowTemp | number: '1.0-0' }}°</span>
      </div>
    </div>

    <div class="weather-loading" *ngIf="loading">
      <mat-icon class="spin">refresh</mat-icon>
      <span>Loading weather...</span>
    </div>
  `,
  styles: [
    `
      .weather-display {
        padding: 1.5rem;
        background: var(--glass-bg);
        border: 1px solid var(--glass-border);
        border-radius: var(--radius-lg);
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
        min-width: 200px;
      }

      .weather-header {
        display: flex;
        justify-content: space-between;
        align-items: center;

        .city-name {
          font-size: 1rem;
          color: var(--text-secondary);
          font-weight: 500;
        }

        .weather-icon {
          width: 50px;
          height: 50px;
        }
      }

      .weather-main {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;

        .temperature {
          font-size: 3rem;
          font-weight: 700;
          line-height: 1;
          color: var(--text-primary);
        }

        .condition {
          font-size: 0.95rem;
          color: var(--text-secondary);
          text-transform: capitalize;
        }
      }

      .weather-details {
        display: flex;
        gap: 1rem;
        margin-top: 0.5rem;
        padding-top: 0.75rem;
        border-top: 1px solid var(--glass-border);

        .detail-item {
          display: flex;
          align-items: center;
          gap: 0.25rem;
          font-size: 0.85rem;
          color: var(--text-dim);

          mat-icon {
            font-size: 1rem;
            width: 1rem;
            height: 1rem;
          }
        }
      }

      .temp-range {
        display: flex;
        gap: 1rem;
        font-size: 0.85rem;

        .high {
          color: var(--accent-primary);
        }

        .low {
          color: var(--accent-secondary);
        }
      }

      .weather-loading {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.5rem;
        padding: 2rem;
        color: var(--text-dim);

        mat-icon.spin {
          animation: spin 1s linear infinite;
        }
      }

      @keyframes spin {
        from {
          transform: rotate(0deg);
        }
        to {
          transform: rotate(360deg);
        }
      }
    `,
  ],
})
export class WeatherDisplayComponent {
  @Input() weather: Weather | null = null;
  @Input() loading = false;
  @Input() showFeelsLike = true;
  @Input() showTempRange = true;

  get iconUrl(): string | null {
    if (!this.weather?.icon) return null;
    return `https://openweathermap.org/img/wn/${this.weather.icon}@2x.png`;
  }
}
