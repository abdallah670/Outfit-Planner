import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Weather, WeatherForecast } from '../../../domain/entities/weather.entity';

export const WeatherActions = createActionGroup({
  source: 'weather',
  events: {
    'Load Current Weather': props<{ city?: string; lat?: number; lon?: number }>(),
    'Load Current Weather Success': props<{ weather: Weather }>(),
    'Load Current Weather Failure': props<{ error: string }>(),

    'Load Weather Forecast': props<{ city?: string; lat?: number; lon?: number; days?: number }>(),
    'Load Weather Forecast Success': props<{ forecast: WeatherForecast[] }>(),
    'Load Weather Forecast Failure': props<{ error: string }>(),

    'Clear Weather Data': emptyProps(),
  },
});
