import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

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

interface MonthlyStats {
  worn: number;
  scheduled: number;
  favorite: number;
}

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.scss'
})
export class CalendarComponent implements OnInit {
  currentDate = signal(new Date());
  currentMonth = signal('');
  currentYear = signal(0);
  
  // View toggle (month/week)
  currentView = signal<'month' | 'week'>('month');
  
  calendarDays = signal<CalendarDay[]>([]);
  weekDays = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'];
  
  // Mock data for scheduled outfits with enhanced properties
  scheduledOutfits = signal<ScheduledOutfit[]>([
    { 
      id: '1', 
      name: 'Business Meeting', 
      date: this.getDateDaysFromNow(2), 
      occasion: 'Work',
      worn: false,
      imageUrl: '',
      items: ['Navy Blazer', 'White Shirt', 'Khaki Pants']
    },
    { 
      id: '2', 
      name: 'Weekend Brunch', 
      date: this.getDateDaysFromNow(5), 
      occasion: 'Casual',
      worn: false,
      imageUrl: '',
      items: ['Denim Jacket', 'Graphic Tee', 'Jeans']
    },
    { 
      id: '3', 
      name: 'Date Night', 
      date: this.getDateDaysFromNow(7), 
      occasion: 'Date Night',
      worn: false,
      imageUrl: '',
      items: ['Leather Jacket', 'Black Turtleneck', 'Dress Pants']
    },
    { 
      id: '4', 
      name: 'Gym Session', 
      date: this.getDateDaysFromNow(3), 
      occasion: 'Sports',
      worn: false,
      imageUrl: '',
      items: ['Sports Bra', 'Leggings', 'Sneakers']
    },
    { 
      id: '5', 
      name: 'Wedding Guest', 
      date: this.getDateDaysFromNow(10), 
      occasion: 'Formal',
      worn: false,
      imageUrl: '',
      items: ['Floral Dress', 'Heels', 'Pearl Earrings']
    },
  ]);

  selectedDate = signal<Date | null>(null);
  selectedDayOutfits = signal<ScheduledOutfit[]>([]);
  
  // Computed weather for selected day
  selectedDayWeather = computed<WeatherData | null>(() => {
    const date = this.selectedDate();
    if (!date) return null;
    return this.getWeatherForDate(date);
  });
  
  // Monthly statistics
  monthlyStats = signal<MonthlyStats>({
    worn: 12,
    scheduled: 8,
    favorite: 5
  });

  ngOnInit(): void {
    this.generateCalendar();
    // Select today by default
    this.selectDay(this.calendarDays().find(d => d.isToday) || this.calendarDays()[0]);
  }

  private getDateDaysFromNow(days: number): Date {
    const date = new Date();
    date.setDate(date.getDate() + days);
    return date;
  }

  generateCalendar(): void {
    const today = new Date();
    const month = this.currentDate().getMonth();
    const year = this.currentDate().getFullYear();

    this.currentMonth.set(today.toLocaleString('default', { month: 'long' }));
    this.currentYear.set(year);

    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const startDay = firstDay.getDay();
    const totalDays = lastDay.getDate();

    const days: CalendarDay[] = [];

    // Previous month days
    const prevMonthLastDay = new Date(year, month, 0).getDate();
    for (let i = startDay - 1; i >= 0; i--) {
      const date = new Date(year, month - 1, prevMonthLastDay - i);
      days.push({
        date,
        dayNumber: prevMonthLastDay - i,
        isCurrentMonth: false,
        isToday: this.isSameDate(date, today),
        outfits: this.getOutfitsForDate(date)
      });
    }

    // Current month days
    for (let i = 1; i <= totalDays; i++) {
      const date = new Date(year, month, i);
      days.push({
        date,
        dayNumber: i,
        isCurrentMonth: true,
        isToday: this.isSameDate(date, today),
        outfits: this.getOutfitsForDate(date),
        weather: this.getWeatherForDate(date)
      });
    }

    // Next month days to fill grid
    const remainingDays = 42 - days.length;
    for (let i = 1; i <= remainingDays; i++) {
      const date = new Date(year, month + 1, i);
      days.push({
        date,
        dayNumber: i,
        isCurrentMonth: false,
        isToday: this.isSameDate(date, today),
        outfits: this.getOutfitsForDate(date)
      });
    }

    this.calendarDays.set(days);
  }

  private getOutfitsForDate(date: Date): ScheduledOutfit[] {
    return this.scheduledOutfits()
      .filter(o => this.isSameDate(new Date(o.date), date));
  }

  private getWeatherForDate(date: Date): WeatherData {
    // Mock weather - deterministic based on date
    const dayOfYear = Math.floor((date.getTime() - new Date(date.getFullYear(), 0, 0).getTime()) / (1000 * 60 * 60 * 24));
    const weatherOptions = [
      { temp: 22, icon: '☀️', condition: 'Sunny' },
      { temp: 20, icon: '⛅', condition: 'Cloudy' },
      { temp: 18, icon: '🌤️', condition: 'Partly Cloudy' },
      { temp: 24, icon: '☀️', condition: 'Clear' },
      { temp: 16, icon: '🌧️', condition: 'Rainy' }
    ];
    return weatherOptions[dayOfYear % weatherOptions.length];
  }

  previousMonth(): void {
    const newDate = new Date(this.currentDate());
    newDate.setMonth(newDate.getMonth() - 1);
    this.currentDate.set(newDate);
    this.generateCalendar();
  }

  nextMonth(): void {
    const newDate = new Date(this.currentDate());
    newDate.setMonth(newDate.getMonth() + 1);
    this.currentDate.set(newDate);
    this.generateCalendar();
  }

  setView(view: 'month' | 'week'): void {
    this.currentView.set(view);
  }

  goToToday(): void {
    this.currentDate.set(new Date());
    this.generateCalendar();
  }

  selectDay(day: CalendarDay): void {
    if (!day) return;
    this.selectedDate.set(day.date);
    this.selectedDayOutfits.set(
      this.scheduledOutfits().filter(o => 
        this.isSameDate(new Date(o.date), day.date)
      )
    );
  }

  isSameDate(date1: Date, date2: Date): boolean {
    return date1.getDate() === date2.getDate() &&
           date1.getMonth() === date2.getMonth() &&
           date1.getFullYear() === date2.getFullYear();
  }

  getUpcomingOutfits(): ScheduledOutfit[] {
    const today = new Date();
    return this.scheduledOutfits()
      .filter(o => new Date(o.date) >= today)
      .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime())
      .slice(0, 5);
  }

  formatDate(date: Date): string {
    return date.toLocaleDateString('en-US', { 
      weekday: 'short', 
      month: 'short', 
      day: 'numeric' 
    });
  }
}
