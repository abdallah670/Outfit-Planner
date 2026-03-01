import { createFeatureSelector, createSelector } from '@ngrx/store';
import { WeatherState } from './weather.reducer';

export const selectWeatherState = createFeatureSelector<WeatherState>('weather');

export const selectCurrentWeather = createSelector(
  selectWeatherState,
  (state: WeatherState) => state.current
);

export const selectWeatherForecast = createSelector(
  selectWeatherState,
  (state: WeatherState) => state.forecast
);

export const selectWeatherLoading = createSelector(
  selectWeatherState,
  (state: WeatherState) => state.loading
);

export const selectWeatherError = createSelector(
  selectWeatherState,
  (state: WeatherState) => state.error
);

export const selectCurrentCity = createSelector(
  selectCurrentWeather,
  (weather) => weather?.city ?? null
);

export const selectCurrentTemperature = createSelector(
  selectCurrentWeather,
  (weather) => weather?.temperature ?? null
);

export const selectCurrentCondition = createSelector(
  selectCurrentWeather,
  (weather) => weather?.condition ?? null
);

export const selectCurrentWeatherIcon = createSelector(
  selectCurrentWeather,
  (weather) => weather?.icon ?? null
);

// OpenWeatherMap icon URL builder
export const selectCurrentWeatherIconUrl = createSelector(
  selectCurrentWeatherIcon,
  (icon) => icon ? `https://openweathermap.org/img/wn/${icon}@2x.png` : null
);
