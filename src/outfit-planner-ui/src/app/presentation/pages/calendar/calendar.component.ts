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
} from '../../../core/state/calendar/calendar.selectors';
import { CalendarEvent, MonthlyStats } from '../../../domain/entities/wear-event.entity';

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

interface WeatherData {
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
  private eventsSignal = toSignal(this.store.select(selectEvents), { initialValue: [] as CalendarEvent[] }) as () => CalendarEvent[];
  stats = toSignal(this.store.select(selectStats), { initialValue: null as MonthlyStats | null }) as () => MonthlyStats | null;
  loading = toSignal(this.store.select(selectLoading), { initialValue: false }) as () => boolean;
  
  // Get current year/month from store
  year = toSignal(this.store.select(selectCurrentYear), { initialValue: new Date().getFullYear() }) as () => number;
  month = toSignal(this.store.select(selectCurrentMonth), { initialValue: new Date().getMonth() + 1 }) as () => number;

  // Selected date signals for sidebar display
  selectedDate = signal<Date | null>(null);
  
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
  
  selectedDayWeather = computed((): WeatherData | null => {
    const date = this.selectedDate();
    if (!date) return null;
    
    // For demo, return mock weather data
    // In real app, this would come from weather API based on date
    return {
      temp: 22,
      icon: '☀️',
      condition: 'Sunny'
    };
  });

  // Calendar days computed from events
  calendarDays = computed((): CalendarDay[] => {
    const yr = this.year();
    const monthNum = this.month();
    const events = this.eventsSignal();
    return this.generateCalendarDays(yr, monthNum, events);
  });

  ngOnInit(): void {
    // Load scheduled outfits from API
    const now = new Date();
    this.store.dispatch(CalendarActions.loadScheduledOutfits({ year: now.getFullYear(), month: now.getMonth() + 1 }));
    this.store.dispatch(CalendarActions.loadMonthlyStats({ year: now.getFullYear(), month: now.getMonth() + 1 }));
    
    // Set current month display
    const date = new Date();
    this.currentMonth.set(date.toLocaleString('default', { month: 'long' }));
    this.currentYear.set(date.getFullYear());
  }

  /**
   * Generate calendar days for the given month
   */
  private generateCalendarDays(year: number, month: number, events: CalendarEvent[]): CalendarDay[] {
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
      days.push({
        date,
        dayNumber: dayNum,
        isCurrentMonth: false,
        isToday: false,
        outfits: this.getOutfitsForDate(date, events)
      });
    }
    
    // Current month days
    for (let day = 1; day <= lastDay.getDate(); day++) {
      const date = new Date(year, month - 1, day);
      const isToday = date.toDateString() === today.toDateString();
      days.push({
        date,
        dayNumber: day,
        isCurrentMonth: true,
        isToday,
        outfits: this.getOutfitsForDate(date, events)
      });
    }
    
    // Next month days to fill the grid
    const remainingDays = 42 - days.length;
    for (let day = 1; day <= remainingDays; day++) {
      const date = new Date(year, month, day);
      days.push({
        date,
        dayNumber: day,
        isCurrentMonth: false,
        isToday: false,
        outfits: this.getOutfitsForDate(date, events)
      });
    }
    
    return days;
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
    console.log('Selected day:', day);
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
}
