import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { catchError, map, mergeMap, of, Observable } from 'rxjs';
import { WeatherActions } from './weather.actions';
import {
  WeatherRepository,
  WEATHER_REPOSITORY,
} from '../../../domain/repositories/weather.repository';
import { Weather, WeatherForecast } from '../../../domain/entities/weather.entity';
import { Inject } from '@angular/core';

@Injectable()
export class WeatherEffects {
  private actions$ = inject(Actions);

  constructor(@Inject(WEATHER_REPOSITORY) private weatherRepository: WeatherRepository) {}

  loadCurrentWeather$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WeatherActions.loadCurrentWeather),
        mergeMap((action: ReturnType<typeof WeatherActions.loadCurrentWeather>) =>
          this.weatherRepository.getCurrentWeather(action.city, action.lat, action.lon).pipe(
            map((weather: Weather) => WeatherActions.loadCurrentWeatherSuccess({ weather })),
            catchError((error) =>
              of(WeatherActions.loadCurrentWeatherFailure({ error: error.message })),
            ),
          ),
        ),
      ) as Observable<Action>,
  );

  loadWeatherForecast$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(WeatherActions.loadWeatherForecast),
        mergeMap((action: ReturnType<typeof WeatherActions.loadWeatherForecast>) =>
          this.weatherRepository
            .getWeatherForecast(action.city, action.lat, action.lon, action.days)
            .pipe(
              map((forecast: WeatherForecast[]) =>
                WeatherActions.loadWeatherForecastSuccess({ forecast }),
              ),
              catchError((error) =>
                of(WeatherActions.loadWeatherForecastFailure({ error: error.message })),
              ),
            ),
        ),
      ) as Observable<Action>,
  );
}
