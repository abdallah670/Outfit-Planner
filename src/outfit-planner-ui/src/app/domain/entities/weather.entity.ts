export interface Weather {
  temperature: number;
  condition: string;
  humidity: number;
  windSpeed: number;
  icon: string;
  city: string;
  description: string;
  feelsLike: number;
  highTemp: number;
  lowTemp: number;
}

export interface WeatherForecast {
  date: string; // ISO date string
  highTemp: number;
  lowTemp: number;
  condition: string;
  icon: string;
  humidity: number;
  windSpeed: number;
  description: string;
}
