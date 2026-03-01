import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Weather, WeatherForecast } from '../../domain/entities/weather.entity';
import { WeatherRepository, WEATHER_REPOSITORY } from '../../domain/repositories/weather.repository';
import { WeatherDataSource } from '../datasources/weather.datasource';

@Injectable({
  providedIn: 'root'
})
export class WeatherRepositoryImpl implements WeatherRepository {
  constructor(private readonly dataSource: WeatherDataSource) {}

  getCurrentWeather(city?: string, lat?: number, lon?: number): Observable<Weather> {
    return this.dataSource.getCurrentWeather(city, lat, lon);
  }

  getWeatherForecast(city?: string, lat?: number, lon?: number, days?: number): Observable<WeatherForecast[]> {
    return this.dataSource.getWeatherForecast(city, lat, lon, days);
  }
}

export const weatherRepositoryProvider = {
  provide: WEATHER_REPOSITORY,
  useClass: WeatherRepositoryImpl,
};
