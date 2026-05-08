import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Weather, WeatherForecast } from '../../domain/entities/weather.entity';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class WeatherDataSource {
  private readonly apiUrl =`${environment.baseUrl}/weather`;

  constructor(private readonly http: HttpClient) {}

  getCurrentWeather(city?: string, lat?: number, lon?: number): Observable<Weather> {
    const params: { [key: string]: string | number | boolean } = {};
    if (city) params['city'] = city;
    if (lat !== undefined) params['lat'] = lat;
    if (lon !== undefined) params['lon'] = lon;

    return this.http.get<Weather>(`${this.apiUrl}/current`, { params });
  }

  getWeatherForecast(
    city?: string,
    lat?: number,
    lon?: number,
    days?: number,
  ): Observable<WeatherForecast[]> {
    const params: { [key: string]: string | number | boolean } = {};
    if (city) params['city'] = city;
    if (lat !== undefined) params['lat'] = lat;
    if (lon !== undefined) params['lon'] = lon;
    if (days !== undefined) params['days'] = days;

    return this.http.get<WeatherForecast[]>(`${this.apiUrl}/forecast`, { params });
  }
}
