import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { WeeklyStyleStatsDto, WeeklyReportDto } from '../../../../domain/entities/profile-stats.entity';

export interface WeeklyReport {
  weekStart: string;
  weekEnd: string;
  isNew: boolean;
  trend: string;
  mostWornItem: {
    name: string;
    imageUrl: string;
    count: number;
  };
  varietyScore: number;
  comfortAverage: number;
  totalWears: number;
}

export interface PastReport {
  weekStart: string;
  weekEnd: string;
  variety: number;
  comfort: number;
  wears: number;
  trend: string;
}

@Component({
  selector: 'app-profile-stats',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './profile-stats.component.html',
  styleUrl: './profile-stats.component.scss',
})
export class ProfileStatsComponent implements OnInit {
  readonly loading = signal<boolean>(true);
  readonly error = signal<string | null>(null);
  readonly currentReport = signal<WeeklyReport | null>(null);
  readonly pastReports = signal<PastReport[]>([]);
  readonly dateRange = signal<string>('');

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.loading.set(true);
    this.error.set(null);

    this.http.get<WeeklyStyleStatsDto>(`${environment.baseUrl}/user/weekly-style-stats`).subscribe({
      next: (data) => {
        const reports = data.weeklyReports ?? [];
        const mapped = reports.map((r) => this.mapReport(r));
        this.currentReport.set(mapped[0] ?? null);
        this.pastReports.set(mapped.slice(1) as any);
        this.dateRange.set(
          mapped.length > 0 ? `${mapped[0].weekStart} – ${mapped[0].weekEnd}` : ''
        );
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load style stats. Please try again later.');
        this.loading.set(false);
      },
    });
  }

  private mapReport(r: WeeklyReportDto): WeeklyReport {
    const [startMonth, startDay] = r.weekStart.split('-');
    const [endMonth, endDay] = r.weekEnd.split('-');

    return {
      weekStart: startMonth ?? '',
      weekEnd: endMonth ? `${endMonth} ${endDay}` : '',
      isNew: r.isCurrentWeek,
      trend: r.trend || 'Mixed',
      mostWornItem: {
        name: r.mostWornItemName || 'No data',
        imageUrl: '',
        count: r.mostWornCount,
      },
      varietyScore: Math.round(r.varietyScore * 100),
      comfortAverage: r.totalWears > 0 ? Number(r.comfortAverage.toFixed(1)) : 0,
      totalWears: r.totalWears,
    };
  }

  getTrendBackground(trend: string): string {
    const map: Record<string, string> = {
      Classic: 'bg-accent-lavender',
      Versatile: 'bg-secondary',
      Mixed: 'bg-primary',
      Focused: 'bg-accent-peach',
    };
    return map[trend] || 'bg-muted';
  }

  getTrendTextColor(trend: string): string {
    const map: Record<string, string> = {
      Classic: 'text-primary-foreground',
      Versatile: 'text-secondary-foreground',
      Mixed: 'text-primary-foreground',
      Focused: 'text-primary-foreground',
    };
    return map[trend] || 'text-foreground';
  }
}
