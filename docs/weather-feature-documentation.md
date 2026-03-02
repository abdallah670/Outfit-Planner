# Weather Feature Implementation Documentation

## 1. Summary of All Files Created and Modified

### .NET Backend Files

#### Application/DTOs/Weather/

| File                    | Path                                                               | Description                                                                                                                                              |
| ----------------------- | ------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `WeatherDto.cs`         | `src/OutfitPlanner.Application/DTOs/Weather/WeatherDto.cs`         | Current weather data transfer object with properties: Temperature, Condition, Humidity, WindSpeed, Icon, City, Description, FeelsLike, HighTemp, LowTemp |
| `WeatherForecastDto.cs` | `src/OutfitPlanner.Application/DTOs/Weather/WeatherForecastDto.cs` | Forecast data transfer object with properties: Date, HighTemp, LowTemp, Condition, Icon, Humidity, WindSpeed, Description                                |

#### Application/Features/Weather/

| File                                | Path                                                                                                | Description                                                                                 |
| ----------------------------------- | --------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| `GetCurrentWeatherQuery.cs`         | `src/OutfitPlanner.Application/Features/Weather/Requests/Queries/GetCurrentWeatherQuery.cs`         | CQRS query for current weather with City, Latitude, Longitude properties                    |
| `GetCurrentWeatherQueryHandler.cs`  | `src/OutfitPlanner.Application/Features/Weather/Handlers/Queries/GetCurrentWeatherQueryHandler.cs`  | Handler implementing IRequestHandler that calls IWeatherService                             |
| `GetWeatherForecastQuery.cs`        | `src/OutfitPlanner.Application/Features/Weather/Requests/Queries/GetWeatherForecastQuery.cs`        | CQRS query for weather forecast with City, Latitude, Longitude, Days (default 5) properties |
| `GetWeatherForecastQueryHandler.cs` | `src/OutfitPlanner.Application/Features/Weather/Handlers/Queries/GetWeatherForecastQueryHandler.cs` | Handler that validates parameters and clamps days to 1-16 range                             |

#### Application/Contracts/Infrastructure/

| File                 | Path                                                                        | Description                                                                           |
| -------------------- | --------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| `IWeatherService.cs` | `src/OutfitPlanner.Application/Contracts/Infrastructure/IWeatherService.cs` | Service interface defining GetCurrentWeatherAsync and GetWeatherForecastAsync methods |

#### Infrastructure Layer

| File                                  | Path                                                                        | Description                                                                                            |
| ------------------------------------- | --------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| `WeatherApiSettings.cs`               | `src/OutfitPlanner.Infrastructure/Configuration/WeatherApiSettings.cs`      | Configuration class with ApiKey, BaseUrl, Units, TimeoutSeconds properties                             |
| `OpenWeatherMapWeatherService.cs`     | `src/OutfitPlanner.Infrastructure/Services/OpenWeatherMapWeatherService.cs` | Real API implementation using HttpClient to call OpenWeatherMap                                        |
| `OpenWeatherResponses.cs`             | `src/OutfitPlanner.Infrastructure/Services/Models/OpenWeatherResponses.cs`  | JSON deserialization models for OpenWeatherMap API responses                                           |
| `DependencyInjection.cs`              | `src/OutfitPlanner.Infrastructure/DependencyInjection.cs`                   | **Modified** - Added WeatherApiSettings configuration and IWeatherService registration with HttpClient |
| `OutfitPlanner.Infrastructure.csproj` | `src/OutfitPlanner.Infrastructure/OutfitPlanner.Infrastructure.csproj`      | **Modified** - Added Microsoft.Extensions.Http package reference                                       |

#### API Layer

| File                   | Path                                                     | Description                                                                                                       |
| ---------------------- | -------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| `WeatherController.cs` | `src/OutfitPlanner.Api/Controllers/WeatherController.cs` | **Modified** - Added endpoints: GET /api/weather/current and GET /api/weather/forecast with [Authorize] attribute |
| `appsettings.json`     | `src/OutfitPlanner.Api/appsettings.json`                 | **Modified** - Added WeatherApi section with your API key configured                                              |

#### Test Files

| File                                     | Path                                                                                       | Description                            |
| ---------------------------------------- | ------------------------------------------------------------------------------------------ | -------------------------------------- |
| `GetCurrentWeatherQueryHandlerTests.cs`  | `tests/OutfitPlanner.Application.UnitTests/Weather/GetCurrentWeatherQueryHandlerTests.cs`  | Unit tests for current weather handler |
| `GetWeatherForecastQueryHandlerTests.cs` | `tests/OutfitPlanner.Application.UnitTests/Weather/GetWeatherForecastQueryHandlerTests.cs` | Unit tests for forecast handler        |
| `OpenWeatherMapWeatherServiceTests.cs`   | `tests/OutfitPlanner.Application.UnitTests/Weather/OpenWeatherMapWeatherServiceTests.cs`   | Unit tests for weather service         |

---

### Angular Frontend Files

#### Domain Layer

| File                    | Path                                                                      | Description                                     |
| ----------------------- | ------------------------------------------------------------------------- | ----------------------------------------------- |
| `weather.entity.ts`     | `src/outfit-planner-ui/src/app/domain/entities/weather.entity.ts`         | Weather and WeatherForecast entity interfaces   |
| `weather.repository.ts` | `src/outfit-planner-ui/src/app/domain/repositories/weather.repository.ts` | WeatherRepository interface with InjectionToken |

#### Data Layer

| File                         | Path                                                                         | Description                                                |
| ---------------------------- | ---------------------------------------------------------------------------- | ---------------------------------------------------------- |
| `weather.datasource.ts`      | `src/outfit-planner-ui/src/app/data/datasources/weather.datasource.ts`       | HttpClient-based data source for /api/weather/\* endpoints |
| `weather.repository.impl.ts` | `src/outfit-planner-ui/src/app/data/repositories/weather.repository.impl.ts` | Repository implementation and provider                     |

#### NgRx State Layer

| File                   | Path                                                                    | Description                                                             |
| ---------------------- | ----------------------------------------------------------------------- | ----------------------------------------------------------------------- |
| `weather.actions.ts`   | `src/outfit-planner-ui/src/app/core/state/weather/weather.actions.ts`   | NgRx actions: loadCurrentWeather, loadWeatherForecast, clearWeatherData |
| `weather.reducer.ts`   | `src/outfit-planner-ui/src/app/core/state/weather/weather.reducer.ts`   | State reducer with initialState and feature selectors                   |
| `weather.effects.ts`   | `src/outfit-planner-ui/src/app/core/state/weather/weather.effects.ts`   | Side effects for loading weather data via repository                    |
| `weather.selectors.ts` | `src/outfit-planner-ui/src/app/core/state/weather/weather.selectors.ts` | Selectors for current, forecast, loading, error states                  |

#### Presentation Layer

| File                           | Path                                                                                                 | Description                                        |
| ------------------------------ | ---------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| `weather-display.component.ts` | `src/outfit-planner-ui/src/app/presentation/components/weather-display/weather-display.component.ts` | Reusable standalone component showing weather info |

#### Configuration

| File            | Path                                          | Description                                                                        |
| --------------- | --------------------------------------------- | ---------------------------------------------------------------------------------- |
| `app.config.ts` | `src/outfit-planner-ui/src/app/app.config.ts` | **Modified** - Added weatherReducer, WeatherEffects, and weatherRepositoryProvider |

---

## 2. NgRx State Management Setup

### State Interface

```typescript
export interface WeatherState {
  current: Weather | null; // Current weather data
  forecast: WeatherForecast[]; // Array of daily forecasts
  loading: boolean; // Loading state indicator
  error: string | null; // Error message if any
}
```

### Initial State

```typescript
export const initialState: WeatherState = {
  current: null,
  forecast: [],
  loading: false,
  error: null,
};
```

### weatherReducer

The reducer handles state transitions for:

- `loadCurrentWeather` - Sets loading=true, clears error
- `loadCurrentWeatherSuccess` - Stores weather data, sets loading=false
- `loadCurrentWeatherFailure` - Stores error message, sets loading=false
- `loadWeatherForecast` / `loadWeatherForecastSuccess` / `loadWeatherForecastFailure` - Same pattern for forecasts
- `clearWeatherData` - Resets to initial state

### WeatherEffects

Effects handle side effects for the two main operations:

**loadCurrentWeather$**:

- Listens for `loadCurrentWeather` actions
- Calls `weatherRepository.getCurrentWeather(city, lat, lon)`
- Dispatches `loadCurrentWeatherSuccess` or `loadCurrentWeatherFailure`

**loadWeatherForecast$**:

- Listens for `loadWeatherForecast` actions
- Calls `weatherRepository.getWeatherForecast(city, lat, lon, days)`
- Dispatches `loadWeatherForecastSuccess` or `loadWeatherForecastFailure`

### Selectors

- `selectCurrentWeather` - Gets current weather object
- `selectWeatherForecast` - Gets forecast array
- `selectWeatherLoading` - Gets loading boolean
- `selectWeatherError` - Gets error string
- `selectCurrentWeatherIconUrl` - Builds OpenWeatherMap icon URL

---

## 3. Repository Pattern Implementation

### Pattern Overview

The repository pattern provides abstraction between the domain layer and data layer, allowing for:

- Testability via dependency injection
- Swappable implementations (HTTP, mock, cache)
- Clean separation of concerns

### Layer Architecture

```
Domain Layer (Entities + Repository Interface)
    ↓
Data Layer (Repository Implementation + DataSource)
    ↓
State Layer (NgRx Effects call Repository)
```

### Domain Layer

**Interface Definition** (`weather.repository.ts`):

```typescript
export interface WeatherRepository {
  getCurrentWeather(
    city?: string,
    lat?: number,
    lon?: number,
  ): Observable<Weather>;
  getWeatherForecast(
    city?: string,
    lat?: number,
    lon?: number,
    days?: number,
  ): Observable<WeatherForecast[]>;
}

export const WEATHER_REPOSITORY = new InjectionToken<WeatherRepository>(
  "WeatherRepository",
);
```

### Data Layer

**DataSource** (`weather.datasource.ts`):

- Direct HTTP calls using Angular HttpClient
- Builds query parameters based on optional inputs
- Returns typed Observables

**Repository Implementation** (`weather.repository.impl.ts`):

- Implements WeatherRepository interface
- Wraps DataSource calls
- Provides `weatherRepositoryProvider` for DI registration

### Dependency Injection Setup

In `app.config.ts`:

```typescript
weatherRepositoryProvider,  // Registers WeatherRepositoryImpl as WEATHER_REPOSITORY
provideStore({ weather: weatherReducer }),
provideEffects(WeatherEffects),
```

---

## 4. UI Components Displaying Weather Information

### Primary Component: WeatherDisplayComponent

**Location**: `src/outfit-planner-ui/src/app/presentation/components/weather-display/weather-display.component.ts`

**Features**:

- Displays city name and weather icon from OpenWeatherMap
- Shows large temperature display
- Shows condition description (e.g., "Clear sky")
- Details section with humidity (water_drop icon), wind speed (air icon), feels-like temp
- Optional high/low temperature range display
- Loading spinner during data fetch
- Glass-morphism styling matching app design system

**Inputs**:

- `@Input() weather: Weather | null` - Weather data to display
- `@Input() loading: boolean` - Show loading state
- `@Input() showFeelsLike: boolean` - Toggle feels-like display (default: true)
- `@Input() showTempRange: boolean` - Toggle high/low display (default: true)

### Usage Example

```typescript
// In a page component
import { WeatherActions } from "../core/state/weather/weather.actions";
import {
  selectCurrentWeather,
  selectWeatherLoading,
} from "../core/state/weather/weather.selectors";

@Component({
  template: `
    <app-weather-display
      [weather]="weather$ | async"
      [loading]="loading$ | async"
    />
  `,
})
export class DashboardComponent {
  private store = inject(Store);

  weather$ = this.store.select(selectCurrentWeather);
  loading$ = this.store.select(selectWeatherLoading);

  ngOnInit() {
    // Load weather for Cairo
    this.store.dispatch(WeatherActions.loadCurrentWeather({ city: "Cairo" }));

    // Or load by coordinates
    this.store.dispatch(
      WeatherActions.loadCurrentWeather({
        lat: 30.0626,
        lon: 31.2497,
      }),
    );

    // Load 5-day forecast
    this.store.dispatch(
      WeatherActions.loadWeatherForecast({
        city: "Cairo",
        days: 5,
      }),
    );
  }
}
```

### Where Weather Displays in the App

The `WeatherDisplayComponent` is now integrated into:

#### 1. Home Page (Dashboard) - **IMPLEMENTED**

**File**: `src/outfit-planner-ui/src/app/presentation/pages/home/home.component.ts`

The home page now displays real weather data in the sidebar:

- Imports `WeatherDisplayComponent` and weather state dependencies
- Dispatches `WeatherActions.loadCurrentWeather({ city: 'Cairo' })` on init
- Uses signals to get weather data and loading state
- Passes data to `<app-weather-display>` component

**Code Integration**:

```typescript
// Component class
weather: Signal<Weather | null> = toSignal(this.store.select(selectCurrentWeather), {
  initialValue: null,
});

weatherLoading: Signal<boolean> = toSignal(this.store.select(selectWeatherLoading), {
  initialValue: false,
});

ngOnInit() {
  this.store.dispatch(WardrobeActions.loadClothingItems());
  // Load weather for Cairo
  this.store.dispatch(WeatherActions.loadCurrentWeather({ city: 'Cairo' }));
}
```

```html
<!-- Template -->
<aside class="sidebar">
  <app-weather-display [weather]="weather()" [loading]="weatherLoading()">
  </app-weather-display>
  <app-daily-pick></app-daily-pick>
  <app-wardrobe-health></app-wardrobe-health>
</aside>
```

#### 2. Wardrobe Dashboard

**File**: `src/outfit-planner-ui/src/app/presentation/pages/wardrobe-dashboard/wardrobe-dashboard.component.ts`

- Can be integrated similarly to home page
- Shows weather alongside wardrobe statistics

#### 3. Daily Suggestion Page (if exists)

- Weather drives clothing recommendations
- Shows current conditions + forecast for planning

### Navigation to Weather-Enabled Pages

Users can access weather information by navigating to:

- `/home` - Home page with live weather widget (Cairo weather by default)
- Future: Can be extended to use browser geolocation for user's location

The weather component automatically loads data when the page initializes using NgRx effects triggered by dispatching the load actions.

---

## API Endpoints Reference

### Backend Endpoints

```
GET /api/weather/current?city={city}&lat={lat}&lon={lon}
GET /api/weather/forecast?city={city}&lat={lat}&lon={lon}&days={days}
```

### Frontend Repository Methods

```typescript
weatherRepository.getCurrentWeather(city?, lat?, lon?): Observable<Weather>
weatherRepository.getWeatherForecast(city?, lat?, lon?, days?): Observable<WeatherForecast[]>
```

### NgRx Actions

```typescript
WeatherActions.loadCurrentWeather({ city?: string, lat?: number, lon?: number })
WeatherActions.loadWeatherForecast({ city?: string, lat?: number, lon?: number, days?: number })
WeatherActions.clearWeatherData()
```
