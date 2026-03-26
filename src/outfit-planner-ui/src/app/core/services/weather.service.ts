import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface WeatherForecast {
  date: string;
  highTemp: number;
  lowTemp: number;
  temperature: number;
  icon: string;
  condition: string;
  description: string;
  humidity: number;
  windSpeed: number;
}

export interface WeatherData {
  date: Date;
  temperature: number;
  condition: string;
  icon: string;
  humidity?: number;
  windSpeed?: number;
}

@Injectable({
  providedIn: 'root',
})
export class WeatherService {
  private http = inject(HttpClient);
  private baseUrl = environment.baseUrl;

  /**
   * Get weather forecast for a month
   */
  getWeatherForMonth(year: number, month: number): Observable<Map<string, WeatherData>> {
    return this.http
      .get<WeatherForecast[]>(`${this.baseUrl}/weather/forecast/month`, {
        params: { year: year.toString(), month: month.toString() },
      })
      .pipe(
        map((forecasts: WeatherForecast[]) => {
          const map = new Map<string, WeatherData>();
          forecasts.forEach((f: WeatherForecast) => {
            const dateKey = new Date(f.date).toISOString().split('T')[0];
            map.set(dateKey, {
              date: new Date(f.date),
              temperature: Math.round((f.highTemp + f.lowTemp) / 2),
              condition: f.condition,
              icon: f.icon,
              humidity: f.humidity,
              windSpeed: f.windSpeed,
            });
          });
          return map;
        })
      );
  }

  /**
   * Get weather for a specific date
   */
  getWeatherForDate(date: Date): Observable<WeatherData | null> {
    const dateStr = date.toISOString().split('T')[0];
    return this.http
      .get<WeatherForecast>(`${this.baseUrl}/weather/forecast/by-date`, {
        params: { date: dateStr },
      })
      .pipe(
        map((f: WeatherForecast) =>
          f
            ? {
                date: new Date(f.date),
                temperature: Math.round((f.highTemp + f.lowTemp) / 2),
                condition: f.condition,
                icon: f.icon,
                humidity: f.humidity,
                windSpeed: f.windSpeed,
              }
            : null
        )
      );
  }
}
