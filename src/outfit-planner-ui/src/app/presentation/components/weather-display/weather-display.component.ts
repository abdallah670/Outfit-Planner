import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Weather } from '../../../domain/entities/weather.entity';

@Component({
  selector: 'app-weather-display',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './weather-display.component.html',
  styleUrls: ['./weather-display.component.scss'],
})
export class WeatherDisplayComponent {
  @Input() weather: Weather | null = null;
  @Input() loading = false;
  @Input() showFeelsLike = true;
  @Input() showTempRange = true;

  get weatherEmoji(): string {
    const condition = this.weather?.condition?.toLowerCase() || '';
    if (condition.includes('cloud')) return '⛅';
    if (condition.includes('rain')) return '🌧️';
    if (condition.includes('clear')) return '☀️';
    if (condition.includes('snow')) return '❄️';
    return '🌡️';
  }
}
