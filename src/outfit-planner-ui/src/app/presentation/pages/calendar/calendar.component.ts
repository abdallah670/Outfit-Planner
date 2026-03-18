import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Store } from '@ngrx/store';
import { toSignal } from '@angular/core/rxjs-interop';
import { CalendarActions } from '../../../core/state/calendar/calendar.actions';
import {
  selectEvents,
  selectStats,
  selectCurrentYear,
  selectCurrentMonth,
  selectLoading,
  selectCalendarEvents,
  selectSelectedDayCalendarEvents,
  selectWeatherData,
} from '../../../core/state/calendar/calendar.selectors';
import { CalendarEvent, MonthlyStats, CalendarEventItem, CalendarEventType, WeatherData } from '../../../domain/entities/wear-event.entity';

interface CalendarDay {
  date: Date;
  dayNumber: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  outfits: ScheduledOutfit[];
  weather?: { temp: number; icon: string; condition: string };
}

interface ScheduledOutfit {
  id: string;
  name: string;
  date: Date;
  occasion: string;
  worn: boolean;
  imageUrl?: string;
  items?: string[];
}

interface WeatherDisplayData {
  temp: number;
  icon: string;
  condition: string;
}

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.scss'
})
export class CalendarComponent implements OnInit {
  private store = inject(Store);

  currentDate = signal(new Date());
  currentMonth = signal('');
  currentYear = signal(0);
  
  // View toggle (month/week)
  currentView = signal<'month' | 'week'>('month');
  
  weekDays = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'];
  
  // NgRx signals - get data from store
  private eventsSignal = toSignal(this.store.select(selectEvents), {
    initialValue: [] as CalendarEvent[],
  }) as () => CalendarEvent[];
  stats = toSignal(this.store.select(selectStats), {
    initialValue: null as MonthlyStats | null,
  }) as () => MonthlyStats | null;
  loading = toSignal(this.store.select(selectLoading), { initialValue: false }) as () => boolean;
  
  // Calendar events from store (time-based events)
  private calendarEventsSignal = toSignal(this.store.select(selectCalendarEvents), {
    initialValue: [] as CalendarEventItem[],
  }) as () => CalendarEventItem[];

  // Weather data from store (as a Map)
  private weatherMapSignal = toSignal(this.store.select(selectWeatherData), {
    initialValue: new Map<string, WeatherData>(),
  }) as () => Map<string, WeatherData>;
  
  // Get current year/month from store
  year = toSignal(this.store.select(selectCurrentYear), {
    initialValue: new Date().getFullYear(),
  }) as () => number;
  month = toSignal(this.store.select(selectCurrentMonth), {
    initialValue: new Date().getMonth() + 1,
  }) as () => number;

  // Selected date signals for sidebar display
  selectedDate = signal<Date | null>(null);
  
  // Calendar events for selected day (from store, not mock data)
  selectedDayEvents = computed((): CalendarEventItem[] => {
    const date = this.selectedDate();
    const calendarEvents = this.calendarEventsSignal();
    if (!date || !calendarEvents.length) return [];
    
    return calendarEvents.filter(event => {
      const eventDate = new Date(event.eventDate);
      return (
        eventDate.getDate() === date.getDate() &&
        eventDate.getMonth() === date.getMonth() &&
        eventDate.getFullYear() === date.getFullYear()
      );
    });
  });
  
  // Computed signals for selected day data
  selectedDayOutfits = computed((): ScheduledOutfit[] => {
    const date = this.selectedDate();
    if (!date) return [];
    
    const events = this.eventsSignal();
    return events
      .filter(event => {
        const eventDate = new Date(event.scheduledDate);
        return eventDate.toDateString() === date.toDateString();
      })
      .map(event => ({
        id: event.outfitId,
        name: event.outfitName,
        date: new Date(event.scheduledDate),
        occasion: event.occasion || 'Casual',
        worn: event.worn,
        imageUrl: event.outfitImageUrl,
        items: []
      }));
  });
  
  selectedDayWeather = computed((): WeatherDisplayData | null => {
    const date = this.selectedDate();
    if (!date) return null;
    
    // Get weather from store based on selected date
    const weatherMap = this.weatherMapSignal();
    const dateKey = date.toISOString().split('T')[0];
    const weather = weatherMap.get(dateKey);
    
    if (weather) {
      return {
        temp: weather.temperature,
        icon: weather.icon,
        condition: weather.condition
      };
    }
    
    // Fallback: return null to show empty weather widget
    return null;
  });

  // Calendar days computed from events and weather
  calendarDays = computed((): CalendarDay[] => {
    const yr = this.year();
    const monthNum = this.month();
    const events = this.eventsSignal();
    const weatherMap = this.weatherMapSignal();
    return this.generateCalendarDays(yr, monthNum, events, weatherMap);
  });

  ngOnInit(): void {
    // Load scheduled outfits from API
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth() + 1;
    
    this.store.dispatch(CalendarActions.loadScheduledOutfits({ year, month }));
    this.store.dispatch(CalendarActions.loadMonthlyStats({ year, month }));
    
    // Load calendar events (time-based) for current month
    this.store.dispatch(CalendarActions.loadCalendarEvents({ year, month }));
    
    // Load weather forecast for current month
    this.store.dispatch(CalendarActions.loadWeatherForecast({ year, month }));
    
    // Set current month display
    this.currentMonth.set(now.toLocaleString('default', { month: 'long' }));
    this.currentYear.set(year);
  }

  /**
   * Generate calendar days for the given month
   */
  private generateCalendarDays(year: number, month: number, events: CalendarEvent[], weatherMap: Map<string, WeatherData>): CalendarDay[] {
    const firstDay = new Date(year, month - 1, 1);
    const lastDay = new Date(year, month, 0);
    const today = new Date();
    
    const days: CalendarDay[] = [];
    const startDayOfWeek = firstDay.getDay();
    
    // Previous month days
    const prevMonthLastDay = new Date(year, month - 1, 0).getDate();
    for (let i = startDayOfWeek - 1; i >= 0; i--) {
      const dayNum = prevMonthLastDay - i;
      const date = new Date(year, month - 2, dayNum);
      const dateKey = date.toISOString().split('T')[0];
      const weather = weatherMap.get(dateKey);
      days.push({
        date,
        dayNumber: dayNum,
        isCurrentMonth: false,
        isToday: false,
        outfits: this.getOutfitsForDate(date, events),
        weather: weather ? this.mapWeatherToDisplay(weather) : undefined
      });
    }
    
    // Current month days
    for (let day = 1; day <= lastDay.getDate(); day++) {
      const date = new Date(year, month - 1, day);
      const isToday = date.toDateString() === today.toDateString();
      const dateKey = date.toISOString().split('T')[0];
      const weather = weatherMap.get(dateKey);
      days.push({
        date,
        dayNumber: day,
        isCurrentMonth: true,
        isToday,
        outfits: this.getOutfitsForDate(date, events),
        weather: weather ? this.mapWeatherToDisplay(weather) : undefined
      });
    }
    
    // Next month days to fill the grid
    const remainingDays = 42 - days.length;
    for (let day = 1; day <= remainingDays; day++) {
      const date = new Date(year, month, day);
      const dateKey = date.toISOString().split('T')[0];
      const weather = weatherMap.get(dateKey);
      days.push({
        date,
        dayNumber: day,
        isCurrentMonth: false,
        isToday: false,
        outfits: this.getOutfitsForDate(date, events),
        weather: weather ? this.mapWeatherToDisplay(weather) : undefined
      });
    }
    
    return days;
  }

  /**
   * Map weather API data to display format with icon
   */
  private mapWeatherToDisplay(weather: WeatherData): { temp: number; icon: string; condition: string } {
    return {
      temp: weather.temperature,
      icon: this.getWeatherIconName(weather.icon),
      condition: weather.condition
    };
  }

  /**
   * Convert OpenWeatherMap icon code to Material icon name
   */
  getWeatherIconName(iconCode: string): string {
    // Map OpenWeatherMap icon codes to Material icon names
    const iconMap: Record<string, string> = {
      '01d': 'wb_sunny',
      '01n': 'nightlight_round',
      '02d': 'partly_cloudy_day',
      '02n': 'partly_cloudy_night',
      '03d': 'cloud',
      '03n': 'cloud',
      '04d': 'cloud',
      '04n': 'cloud',
      '09d': 'grain',
      '09n': 'grain',
      '10d': 'rainy',
      '10n': 'rainy',
      '11d': 'flash_on',
      '11n': 'flash_on',
      '13d': 'ac_unit',
      '13n': 'ac_unit',
      '50d': 'air',
      '50n': 'air'
    };
    return iconMap[iconCode] || 'cloud';
  }

  /**
   * Get outfits for a specific date from events
   */
  private getOutfitsForDate(date: Date, events: CalendarEvent[]): ScheduledOutfit[] {
    return events
      .filter(event => {
        const eventDate = new Date(event.scheduledDate);
        return eventDate.toDateString() === date.toDateString();
      })
      .map(event => ({
        id: event.outfitId,
        name: event.outfitName,
        date: new Date(event.scheduledDate),
        occasion: event.occasion || 'Casual',
        worn: event.worn,
        imageUrl: event.outfitImageUrl,
        items: []
      }));
  }

  previousMonth(): void {
    const currentYearVal = this.year();
    const currentMonthVal = this.month();
    let newMonth = currentMonthVal - 1;
    let newYear = currentYearVal;
    
    if (newMonth < 1) {
      newMonth = 12;
      newYear--;
    }
    
    this.store.dispatch(CalendarActions.setCurrentMonth({ year: newYear, month: newMonth }));
    this.store.dispatch(CalendarActions.loadScheduledOutfits({ year: newYear, month: newMonth }));
    this.store.dispatch(CalendarActions.loadMonthlyStats({ year: newYear, month: newMonth }));
    this.store.dispatch(CalendarActions.loadCalendarEvents({ year: newYear, month: newMonth }));
    this.store.dispatch(CalendarActions.loadWeatherForecast({ year: newYear, month: newMonth }));
    
    const date = new Date(newYear, newMonth - 1, 1);
    this.currentMonth.set(date.toLocaleString('default', { month: 'long' }));
    this.currentYear.set(newYear);
  }

  nextMonth(): void {
    const currentYearVal = this.year();
    const currentMonthVal = this.month();
    let newMonth = currentMonthVal + 1;
    let newYear = currentYearVal;
    
    if (newMonth > 12) {
      newMonth = 1;
      newYear++;
    }
    
    this.store.dispatch(CalendarActions.setCurrentMonth({ year: newYear, month: newMonth }));
    this.store.dispatch(CalendarActions.loadScheduledOutfits({ year: newYear, month: newMonth }));
    this.store.dispatch(CalendarActions.loadMonthlyStats({ year: newYear, month: newMonth }));
    this.store.dispatch(CalendarActions.loadCalendarEvents({ year: newYear, month: newMonth }));
    this.store.dispatch(CalendarActions.loadWeatherForecast({ year: newYear, month: newMonth }));
    
    const date = new Date(newYear, newMonth - 1, 1);
    this.currentMonth.set(date.toLocaleString('default', { month: 'long' }));
    this.currentYear.set(newYear);
  }

  setView(view: 'month' | 'week'): void {
    this.currentView.set(view);
  }

  selectDay(day: CalendarDay): void {
    // Set the selected date
    this.selectedDate.set(day.date);
    this.store.dispatch(CalendarActions.selectDate({ date: day.date }));
  }

  isSameDate(date1: Date, date2: Date): boolean {
    return date1.toDateString() === date2.toDateString();
  }

  // Helper method for template
  getDateDaysFromNow(days: number): Date {
    const date = new Date();
    date.setDate(date.getDate() + days);
    return date;
  }

  /**
   * Calculate adherence percentage
   */
  getAdherence(): number {
    const s = this.stats();
    if (!s || !s.scheduledCount) return 0;
    return Math.round((s.wornCount / s.scheduledCount) * 100);
  }
}
