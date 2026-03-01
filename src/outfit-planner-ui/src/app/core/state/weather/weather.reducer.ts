import { createFeature, createReducer, on } from '@ngrx/store';
import { WeatherActions } from './weather.actions';
import { Weather, WeatherForecast } from '../../../domain/entities/weather.entity';

export interface WeatherState {
  current: Weather | null;
  forecast: WeatherForecast[];
  loading: boolean;
  error: string | null;
}

export const initialState: WeatherState = {
  current: null,
  forecast: [],
  loading: false,
  error: null,
};

export const weatherFeature = createFeature({
  name: 'weather',
  reducer: createReducer(
    initialState,
    on(WeatherActions.loadCurrentWeather, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(WeatherActions.loadCurrentWeatherSuccess, (state, { weather }) => ({
      ...state,
      current: weather,
      loading: false,
    })),
    on(WeatherActions.loadCurrentWeatherFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),
    on(WeatherActions.loadWeatherForecast, (state) => ({
      ...state,
      loading: true,
      error: null,
    })),
    on(WeatherActions.loadWeatherForecastSuccess, (state, { forecast }) => ({
      ...state,
      forecast: forecast || [],
      loading: false,
    })),
    on(WeatherActions.loadWeatherForecastFailure, (state, { error }) => ({
      ...state,
      loading: false,
      error,
    })),
    on(WeatherActions.clearWeatherData, () => initialState),
  ),
});

export const {
  name: weatherFeatureKey,
  reducer: weatherReducer,
  selectCurrent,
  selectForecast,
  selectLoading,
  selectError,
} = weatherFeature;
