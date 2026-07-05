# Profile Stats Page Design

## 6. Profile Stats Page

### Page Purpose
Displays the user's weekly style report and overall wardrobe analytics at `/profile/stats`. Triggered primarily by the "Weekly Style Report Ready" notification.

### Page Structure
```
- Page header: "Style Stats" with current week/date range selector
- Weekly Report Card (highlighted if fresh):
  * Report date badge: "Week of {date}"
  * Most worn item display (image + name + wear count)
  * Key metrics row:
    - Outfit variety score (0-100%)
    - Comfort average (1-5 stars)
    - Total wears last week
  * Style trend tag: "{trend}" (e.g. "Classic", "Versatile", "Focused", "Mixed")
- Historical reports list (past weekly reports)
- Empty state: "No stats yet. Start recording wears to see your weekly report."
```

### Component Architecture
```typescript
// Core Components
- ProfileStatsPage (main container, routed at /profile/stats)
- WeeklyReportCard (featured/current report)
- MetricCard (single stat display: variety, comfort, total)
- StyleTrendBadge (colored chip showing trend)
- MostWornItemCard (image + name + count)
- ReportHistoryList (past reports timeline)
- EmptyStatsState (no data yet CTA)
- DateRangeSelector (optional: change report period)
```

### State Management
- NgRx feature: `profileStats`
- State shape:
  ```typescript
  {
    currentReport: WeeklyReport | null;
    reportHistory: WeeklyReport[];
    loading: boolean;
    error: string | null;
    selectedRange: { start: Date; end: Date } | null;
  }
  ```

### Backend Integration
- New GET endpoint: `GET /api/notifications/weekly-report` or extend `ProfileController`
  - Returns current week's aggregated stats + history
  - Response model: `WeeklyReportDto { id, userId, weekStart, weekEnd, mostWornItemId, mostWornItemName, mostWornCount, varietyScore, comfortAverage, totalWears, styleTrend, createdAt }`

### UI Mockup (CSS-based)

```
┌─────────────────────────────────────────────┐
│  Style Stats                            🔔  │
├─────────────────────────────────────────────┤
│                                             │
│  ┌───────────────────────────────────────┐  │
│  │  📅 Week of Jul 6 – Jul 12 (NEW)    │  │
│  │                                       │  │
│  │  Most Worn Item:                      │  │
│  │  ┌─────────┐                         │  │
│  │  │  IMG    │  White Linen Shirt       │  │
│  │  │  (square│  4 times this week       │  │
│  │  │  crop)  │                         │  │
│  │  └─────────┘                         │  │
│  │                                       │  │
│  │  ┌────────┐ ┌────────┐ ┌────────┐   │  │
│  │  │ 72%    │ │ 4.2 ★  │ │  23    │   │  │
│  │  │Variety │ │Comfort │ │ Wears  │   │  │
│  │  └────────┘ └────────┘ └────────┘   │  │
│  │                                       │  │
│  │  Style: [Classic]                     │  │
│  └───────────────────────────────────────┘  │
│                                             │
│  Past Reports                               │
│  ─────────────────────────────────────────  │
│  Week of Jun 30 – Jul 6  →  Style: Mixed     │
│  Week of Jun 23 – Jun 29  →  Style: Versatile│
│                                             │
└─────────────────────────────────────────────┘
```

### Key Interactions
1. On page load, dispatch `loadProfileStats()` to fetch current week + history
2. If a new weekly report exists (created by background job), show "NEW" badge and auto-open detail
3. Tap a historical report to view its detailed breakdown
4. Each metric card has an info tooltip explaining its calculation:
   - Variety Score: `(unique items worn) / (total wears) × 100`
   - Comfort Average: Average rating (1–5) across all wears
   - Total Wears: Count of `WearEvent` records in the period

### Responsive Behavior
- Mobile: metric cards stack vertically (1 column)
- Tablet: metric cards in 2x2 grid
- Desktop: metric cards in single row, side-by-side with most worn item card

### Design Tokens Used
- Background: `#FAFAFA`
- Surface: `#FFFFFF`
- Primary (CTAs/badges): `#F8B4C4`
- Secondary (success/variety): `#9CAF88`
- Text Primary: `#2D3436`
- Text Secondary (metadata): `#636E72`
- Border: `#DFE6E9`
- Card radius: `12px`
- Card shadow: `0 2px 8px rgba(0, 0, 0, 0.06)`
