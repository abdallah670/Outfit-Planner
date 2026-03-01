import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { Weather, WeatherForecast } from '../entities/weather.entity';

export const WEATHER_REPOSITORY = new InjectionToken<WeatherRepository>('WeatherRepository');

export interface WeatherRepository {
  getCurrentWeather(city?: string, lat?: number, lon?: number): Observable<Weather>;
  getWeatherForecast(city?: string, lat?: number, lon?: number, days?: number): Observable<WeatherForecast[]>;
}
