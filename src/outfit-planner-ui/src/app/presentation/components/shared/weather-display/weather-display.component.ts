import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Weather } from '../../../../domain/entities/weather.entity';

@Component({
  selector: 'app-weather-display',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './weather-display.component.html',
  styleUrls: ['./weather-display.component.scss'],
})
export class WeatherDisplayComponent {
  @Input() weather: Weather | null = null;
  @Input() loading = false;
  @Input() showFeelsLike = true;
  @Input() showDescription = true;
  @Input() showCity = true;

  get weatherEmoji(): string {
    const condition = this.weather?.condition?.toLowerCase() || '';
    if (condition.includes('cloud')) return '⛅';
    if (condition.includes('rain')) return '🌧️';
    if (condition.includes('clear') || condition.includes('sun')) return '☀️';
    if (condition.includes('snow')) return '❄️';
    if (condition.includes('fog') || condition.includes('mist')) return '🌫️';
    return '🌡️';
  }
}