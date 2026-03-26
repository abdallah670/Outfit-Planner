import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
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
import { ScheduleOutfitModalComponent } from '../../components/calendar/schedule-outfit-modal/schedule-outfit-modal.component';
import { AddEventModalComponent } from '../../components/calendar/add-event-modal/add-event-modal.component';
import { Outfit, ClothingItem } from '../../../domain/entities/outfit.entity';
import { MatConfirmDialogComponent } from '../../components/shared/mat-confirm-dialog/mat-confirm-dialog.component';

interface WeekViewDay {
  date: Date;
  dayName: string;
  dayNumber: number;
  isToday: boolean;
  outfits: ScheduledOutfit[];
  events: CalendarEventItem[];
  weather?: { temp: number; icon: string; condition: string };
}

interface WeekViewTimeSlot {
  hour: number;
  displayTime: string;
  events: CalendarEventItem[];
}

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
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.scss'
})
export class CalendarComponent implements OnInit {
  store = inject(Store);
  private dialog = inject(MatDialog);

  // Sample data for outfits (would come from a service in production)
  private sampleOutfits: Outfit[] = [
    {
      id: '1',
      userId: 'user1',
      name: 'Weekend Casual',
      imageUrl: '/outfit_placeholder.png',
      items: [],
      occasion: 'Casual' as any,
      suitableWeather: { temperature: 20, condition: 'Sunny', precipitationProbability: 0, humidity: 50, windSpeed: 5 },
      season: 'AllSeason' as any,
      comfortLevel: 8,
      styleRating: 7,
      createdAt: new Date(),
      lastWorn: new Date(),
      timesWorn: 5,
      status: 'active' as any,
      feedback: []
    },
    {
      id: '2',
      userId: 'user1',
      name: 'Office Look',
      imageUrl: '/outfit_placeholder.png',
      items: [],
      occasion: 'Work' as any,
      suitableWeather: { temperature: 22, condition: 'Cloudy', precipitationProbability: 10, humidity: 55, windSpeed: 3 },
      season: 'AllSeason' as any,
      comfortLevel: 7,
      styleRating: 8,
      createdAt: new Date(),
      lastWorn: new Date(),
      timesWorn: 3,
      status: 'active' as any,
      feedback: []
    },
    {
      id: '3',
      userId: 'user1',
      name: 'Date Night',
      imageUrl: '/outfit_placeholder.png',
      items: [],
      occasion: 'Date' as any,
      suitableWeather: { temperature: 18, condition: 'Clear', precipitationProbability: 0, humidity: 60, windSpeed: 2 },
      season: 'AllSeason' as any,
      comfortLevel: 9,
      styleRating: 9,
      createdAt: new Date(),
      lastWorn: new Date(),
      timesWorn: 2,
      status: 'favorite' as any,
      feedback: []
    }
  ];

  // Sample clothing items (would come from a service in production)
  private sampleClothingItems: ClothingItem[] = [
    { id: '1', name: 'Blue Jeans', type: 'Bottom' as any, category: 'Pants', color: 'Blue', imageUrl: '/clothing-placeholder.png' },
    { id: '2', name: 'White T-Shirt', type: 'Top' as any, category: 'Shirts', color: 'White', imageUrl: '/clothing-placeholder.png' },
    { id: '3', name: 'Black Jacket', type: 'Outerwear' as any, category: 'Jackets', color: 'Black', imageUrl: '/clothing-placeholder.png' },
    { id: '4', name: 'White Sneakers', type: 'Footwear' as any, category: 'Shoes', color: 'White', imageUrl: '/clothing-placeholder.png' },
    { id: '5', name: 'Leather Belt', type: 'Accessory' as any, category: 'Accessories', color: 'Brown', imageUrl: '/clothing-placeholder.png' },
    { id: '6', name: 'Red Dress', type: 'Dress' as any, category: 'Dresses', color: 'Red', imageUrl: '/clothing-placeholder.png' },
  ];

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
  
  // Week view signals
  currentWeekStart = signal<Date>(new Date());
  weekViewDays = computed((): WeekViewDay[] => {
    const startOfWeek = this.currentWeekStart();
    const events = this.eventsSignal();
    const calendarEvents = this.calendarEventsSignal();
    const weatherMap = this.weatherMapSignal();
    
    const days: WeekViewDay[] = [];
    const today = new Date();
    
    for (let i = 0; i < 7; i++) {
      const date = new Date(startOfWeek);
      date.setDate(date.getDate() + i);
      const dateKey = date.toISOString().split('T')[0];
      const weather = weatherMap.get(dateKey);
      
      days.push({
        date,
        dayName: date.toLocaleDateString('en-US', { weekday: 'short' }),
        dayNumber: date.getDate(),
        isToday: date.toDateString() === today.toDateString(),
        outfits: this.getOutfitsForDate(date, events),
        events: calendarEvents.filter(event => {
          const eventDate = new Date(event.eventDate);
          return eventDate.toDateString() === date.toDateString();
        }),
        weather: weather ? this.mapWeatherToDisplay(weather) : undefined
      });
    }
    
    return days;
  });
  
  // Time slots for week view (6 AM to 10 PM)
  timeSlots = signal<WeekViewTimeSlot[]>(
    Array.from({ length: 17 }, (_, i) => {
      const hour = i + 6; // Start from 6 AM
      return {
        hour,
        displayTime: `${hour % 12 || 12}:00 ${hour < 12 ? 'AM' : 'PM'}`,
        events: []
      };
    })
  );
  
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

  /**
   * Select a day from week view
   */
  selectWeekDay(day: WeekViewDay): void {
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

  /**
   * Open the Add Event Modal
   */
  openAddEventModal(): void {
    const date = this.selectedDate() || new Date();
    
    const dialogRef = this.dialog.open(AddEventModalComponent, {
      width: '500px',
      maxWidth: '95vw',
      panelClass: 'add-event-dialog',
      data: {
        date: date,
        outfits: this.sampleOutfits,
      },
    });

    dialogRef.afterClosed().subscribe((result: { success: boolean } | undefined) => {
      if (result?.success) {
        console.log('Event added successfully:', result);
        // Refresh the calendar events
        const year = this.year();
        const month = this.month();
        this.store.dispatch(CalendarActions.loadCalendarEvents({ year, month }));
      }
    });
  }

  /**
   * Open the Schedule Outfit Modal
   */
  openScheduleOutfitModal(): void {
    const date = this.selectedDate() || new Date();
    
    const dialogRef = this.dialog.open(ScheduleOutfitModalComponent, {
      width: '700px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      panelClass: 'schedule-outfit-dialog',
      data: {
        date: date,
        outfits: this.sampleOutfits,
        clothingItems: this.sampleClothingItems,
      },
    });

    dialogRef.afterClosed().subscribe((result: { success: boolean } | undefined) => {
      if (result?.success) {
        console.log('Outfit scheduled successfully:', result);
        // Refresh the calendar data
        const year = this.year();
        const month = this.month();
        this.store.dispatch(CalendarActions.loadScheduledOutfits({ year, month }));
        this.store.dispatch(CalendarActions.loadMonthlyStats({ year, month }));
      }
    });
  }

  // ==================== Week View Navigation ====================

  /**
   * Get the start of the week for a given date
   */
  private getStartOfWeek(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day; // Adjust when day is Sunday
    return new Date(d.setDate(diff));
  }

  /**
   * Navigate to previous week
   */
  previousWeek(): void {
    const currentStart = this.currentWeekStart();
    const newStart = new Date(currentStart);
    newStart.setDate(newStart.getDate() - 7);
    this.currentWeekStart.set(newStart);
    
    // Load data for the new week
    const year = newStart.getFullYear();
    const month = newStart.getMonth() + 1;
    this.store.dispatch(CalendarActions.loadScheduledOutfits({ year, month }));
    this.store.dispatch(CalendarActions.loadCalendarEvents({ year, month }));
    this.store.dispatch(CalendarActions.loadWeatherForecast({ year, month }));
    
    // Update displayed month/year
    this.currentMonth.set(newStart.toLocaleString('default', { month: 'long' }));
    this.currentYear.set(year);
  }

  /**
   * Navigate to next week
   */
  nextWeek(): void {
    const currentStart = this.currentWeekStart();
    const newStart = new Date(currentStart);
    newStart.setDate(newStart.getDate() + 7);
    this.currentWeekStart.set(newStart);
    
    // Load data for the new week
    const year = newStart.getFullYear();
    const month = newStart.getMonth() + 1;
    this.store.dispatch(CalendarActions.loadScheduledOutfits({ year, month }));
    this.store.dispatch(CalendarActions.loadCalendarEvents({ year, month }));
    this.store.dispatch(CalendarActions.loadWeatherForecast({ year, month }));
    
    // Update displayed month/year
    this.currentMonth.set(newStart.toLocaleString('default', { month: 'long' }));
    this.currentYear.set(year);
  }

  /**
   * Set the current week based on selected date
   */
  setCurrentWeekFromDate(date: Date): void {
    const startOfWeek = this.getStartOfWeek(date);
    this.currentWeekStart.set(startOfWeek);
  }

  // ==================== Delete Functionality ====================

  /**
   * Delete a scheduled outfit
   */
  deleteScheduledOutfit(outfitId: string): void {
    const dialogRef = this.dialog.open(MatConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Scheduled Outfit',
        message: 'Are you sure you want to delete this scheduled outfit? This action cannot be undone.',
        confirmText: 'Delete',
        cancelText: 'Cancel',
        isDanger: true,
      },
    });

    dialogRef.afterClosed().subscribe((result: boolean | undefined) => {
      if (result) {
        // Find the event ID for this outfit
        const events = this.eventsSignal();
        const event = events.find(e => e.outfitId === outfitId);
        if (event) {
          this.store.dispatch(CalendarActions.deleteWearEvent({ eventId: event.id }));
        }
      }
    });
  }

  /**
   * Delete a calendar event
   */
  deleteCalendarEvent(eventId: string): void {
    const dialogRef = this.dialog.open(MatConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Event',
        message: 'Are you sure you want to delete this event? This action cannot be undone.',
        confirmText: 'Delete',
        cancelText: 'Cancel',
        isDanger: true,
      },
    });

    dialogRef.afterClosed().subscribe((result: boolean | undefined) => {
      if (result) {
        this.store.dispatch(CalendarActions.deleteCalendarEvent({ eventId }));
      }
    });
  }
}
