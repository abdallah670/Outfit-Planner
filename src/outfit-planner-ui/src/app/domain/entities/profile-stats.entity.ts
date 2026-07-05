export interface WeeklyStyleStatsDto {
  weeklyReports: WeeklyReportDto[];
}


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

export interface WeeklyStyleStatsDto {
  weeklyReports: WeeklyReportDto[];
}

export interface WeeklyReportDto {
  weekStart: string;
  weekEnd: string;
  isCurrentWeek: boolean;
  mostWornItemName?: string;
  mostWornCount: number;
  varietyScore: number;
  comfortAverage: number;
  totalWears: number;
  trend: string;
}