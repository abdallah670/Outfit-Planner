# Outfit-Planner: Admin Panel — Site-Aligned Design

## Design Philosophy

Admin pages follow the **exact same layout, colors, and component system** as the main Outfit Planner website. No dark sidebar, no glassmorphism — just clean, consistent, on-brand admin pages that feel like a natural extension of the app.

---

## 1. Site Design System (From `styles.scss`)

### CSS Variables (Reused)

```css
/* Backgrounds */
--background: #fdf2f8       /* Light pink page background */
--card: #ffffff              /* White card surfaces */

/* Text */
--foreground: #1f2937        /* Dark text */
--accent-dark: #2d3436       /* Dark accents */

/* Primary (deep pink) */
--primary: #db2777
--primary-foreground: #ffffff
--ring: #db2777

/* Secondary (light pink) */
--secondary: #fce7f3
--secondary-foreground: #be185d

/* Accents */
--accent: #fbcfe8
--accent-pink: #f8b4c4
--accent-green: #9caf88

/* Misc */
--border: #e5e7eb
--muted: #f3f4f6
--muted-foreground: #6b7280
--card-shadow: 0 4px 12px rgba(0, 0, 0, 0.08)

/* Radius */
--radius-lg: 16px
--radius-xl: 24px
```

### Global Background (From `app.html`)

```
Floating decorative blobs behind all content:
  .blob-1 → radial-gradient(circle, var(--accent-pink) 0%, transparent 70%)
  .blob-2 → radial-gradient(circle, var(--accent-green) 0%, transparent 70%)
  .blob-3 → radial-gradient(circle, #a2c2e6 0%, transparent 70%)
```

### Component Style Patterns

| Element | Classes / Styling |
|---------|-------------------|
| Card | `<mat-card>` with white bg, --card-shadow, --radius-lg |
| Button | `<button mat-button>` with --primary or --accent-pink |
| Input | `<mat-form-field appearance="outline">` |
| Table | `<mat-table>` inside `<mat-card>` |
| Icon | `<mat-icon>` or `<iconify-icon>` |
| Badge | `<span class="badge">` with colored bg |
| Nav link | `.nav-link` with pink color on active |

---

## 2. Admin Layout

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  Background: #fdf2f8 with 3 floating blobs (pink, green, blue)   │
│                                                                    │
│  ┌─ Navbar ────────────────────────────────────────────────────┐  │
│  │  [👕 Outfit Planner]    [Home] [Wardrobe] [Social] [Admin] │  │
│  │                                        [🔔] [Profile pic]  │  │
│  └─────────────────────────────────────────────────────────────┘  │
│                                                                    │
│  ┌─ Page Content (max-width: 1280px, centered) ───────────────┐  │
│  │                                                              │  │
│  │  ┌── White Card ──────────────────────────────────────────┐ │  │
│  │  │  📊 Page Title                        [+ Action]      │ │  │
│  │  └────────────────────────────────────────────────────────┘ │  │
│  │                                                              │  │
│  │  ┌── White Card ──────────────────────────────────────────┐ │  │
│  │  │  Filters / Search Bar                                  │ │  │
│  │  └────────────────────────────────────────────────────────┘ │  │
│  │                                                              │  │
│  │  ┌── White Card ──────────────────────────────────────────┐ │  │
│  │  │  Content: Table / Cards / Charts / Form                │ │  │
│  │  └────────────────────────────────────────────────────────┘ │  │
│  │                                                              │  │
│  │  ┌── White Card ──────────────────────────────────────────┐ │  │
│  │  │  Pagination / Actions                                   │ │  │
│  │  └────────────────────────────────────────────────────────┘ │  │
│  │                                                              │  │
│  └──────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

### Admin Navigation
```
Admin links are in the existing navbar, not a sidebar:
  Navbar: [Home] [Wardrobe] [Social] [Calendar] [Admin] [Settings]

Admin pages are accessed via the navbar "Admin" link which shows:
  ┌──────────────────────────────────────────────┐
  │  📊 Dashboard                                │
  │  👥 Users              (or click directly)   │
  │  📝 Content (Posts/Outfits/Polls)            │
  │  📈 Analytics / Reports                      │
  │  🛠 System Operations                        │
  │  📋 Audit Logs                               │
  │  ⚙ Settings                                  │
  │  🎫 Support Tickets                          │
  └──────────────────────────────────────────────┘
```

---

## 3. Dashboard

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  📊 Dashboard                                           [🔄]    │
├──────────────────────────────────────────────────────────────────┤
│  ┌───── White Card ─────┐  ┌───── White Card ─────┐            │
│  │  👥 Total Users      │  │  📈 New Today        │            │
│  │  12,847              │  │  +124                │            │
│  │  Registered users    │  │  New registrations   │            │
│  │  ▲ 12% vs last month │  │  ▲ 8% vs yesterday  │            │
│  └──────────────────────┘  └──────────────────────┘            │
│  ┌───── White Card ─────┐  ┌───── White Card ─────┐            │
│  │  🟢 Active Users     │  │  🚩 Pending Reports  │            │
│  │  3,205               │  │  23                  │            │
│  │  Last 30 days        │  │  Awaiting review     │            │
│  │  ▼ 2% vs last month  │  │  ▲ 5% vs yesterday  │            │
│  └──────────────────────┘  └──────────────────────┘            │
│  ┌───── White Card ─────┐                                      │
│  │  🔒 Locked Accounts  │                                      │
│  │  2                   │                                      │
│  │  Currently locked    │                                      │
│  └──────────────────────┘                                      │
├──────────────────────────────────────────────────────────────────┤
│  ┌───────────── White Card ──────────┐  ┌───── White Card ──┐  │
│  │  📈 User Registration Trend      │  │  📊 Content        │  │
│  │  [canvas line chart]             │  │  Creation          │  │
│  │  ▲ +12.4% vs last month         │  │  [canvas bar chart]│  │
│  └──────────────────────────────────┘  │  ▲ +8.2%          │  │
│  ┌────────────────────────────────────┐└────────────────────┘  │
│  │  🚩 Report Status                 │                         │
│  │  [canvas doughnut chart]          │                         │
│  │  Pending: 23 | Reviewed: 156 | Resolved: 89               │  │
│  └────────────────────────────────────┘                         │
└──────────────────────────────────────────────────────────────────┘
```

### Component Architecture
```typescript
- AdminDashboardComponent (main container)
- MatCard → KpiCard (icon + value + label + trend)
- MatCard → ChartCard (canvas element)
```

---

## 4. Content Management (Unified — Posts + Outfits + Polls)

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  📝 Content Management                          [+ Create New]   │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card: Tabs ────────────────────────────────────────┐ │
│  │  [📄 Posts]  [👕 Outfits]  [📋 Polls]                     │ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card: Filters ─────────────────────────────────────┐ │
│  │  [🔍 Search...]  [Type: ▼]  [Status: ▼]  [📅 Date: ▼]    │ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ─── Tab: 📄 Posts Active ───                                   │
│                                                                  │
│  ┌── White Card: Table ───────────────────────────────────────┐ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │  ☐ │ Title     │ Author  │ Type │ Status   │ Date  │  │ │
│  │  ├──────────────────────────────────────────────────────┤  │ │
│  │  │  ☐ │ Summer..  │ @jane   │ Post │ ✅ Active│ 2h   │  │ │
│  │  │  ☐ │ Beach..   │ @mark   │ Poll │ ✅ Active│ 30m  │  │ │
│  │  │  ☐ │ Casual..  │ @lisa   │ Outf │ 🗑 Delete│ 1d  │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ─── Tab: 👕 Outfits Active ───                                 │
│                                                                  │
│  ┌── White Card: Table ───────────────────────────────────────┐ │
│  │  ┌────────┬────────────┬──────────┬──────────┬──────────┐  │ │
│  │  │ ☐ │Img │ Name       │ Creator  │ Status   │ Wears   │  │ │
│  │  ├────────┼────────────┼──────────┼──────────┼──────────┤  │ │
│  │  │ ☐ │[🖼]│ Beach Look │ @mark    │ ✅ Active│ 342     │  │ │
│  │  │ ☐ │[🖼]│ Office Fit │ @jane    │ ✅ Active│ 89      │  │ │
│  │  │ ☐ │[🖼]│ Casual     │ @lisa    │ 🗑 Delete│ 12     │  │ │
│  │  └────────┴────────────┴──────────┴──────────┴──────────┘  │ │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ─── Tab: 📋 Polls Active ───                                   │
│                                                                  │
│  ┌── White Card: Table ───────────────────────────────────────┐ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │ ☐ │ Question     │ Creator │ Status  │Votes│ Ends  │  │ │
│  │  ├──────────────────────────────────────────────────────┤  │ │
│  │  │ ☐ │ What suit?   │ @jane   │ 🟢 Open │ 156 │ 2d   │  │ │
│  │  │ ☐ │ Dress code?  │ @mark   │ 🔴 Closed│ 89 │Ended │  │ │
│  │  │ ☐ │ Color match? │ @lisa   │ 🟢 Open │ 234 │ 5d   │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card: Actions + Pagination ────────────────────────┐ │
│  │  [✅ Active]  [🔴 Close Poll]  [🗑 Delete]  [📥 Export]    │ │
│  │  < 1  2  3  4  5  ...  10  >    50 / page                 │ │
│  └────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
```

### Component Architecture
```typescript
- AdminContentComponent            // Main container with tab switching
- AdminContentTabsComponent       // Tab bar: Posts | Outfits | Polls
- AdminContentFilterBarComponent  // Shared filter bar
- AdminContentTableComponent      // Single mat-table adapting columns per tab
- AdminContentActionsComponent    // Context-aware action bar
```

---

## 5. Users Management

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  👥 Users Management                         [+ Invite User]    │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  [🔍 Search by name or email...]  [Role: ▼]  [Status: ▼]  │ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │ ☐ │ Name     │ Email       │ Role   │ Status│ Joined │  │ │
│  │  ├──────────────────────────────────────────────────────┤  │ │
│  │  │ ☐ │ Jane D.  │ j@e.com     │ User   │ 🟢 On │ J'26  │  │ │
│  │  │ ☐ │ Mark S.  │ m@e.com     │ Admin  │ 🔴 Ban│ D'25  │  │ │
│  │  │ ☐ │ Lisa K.  │ l@e.com     │ Moder  │ 🟡 Ina│ M'26  │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  [🔨 Ban]  [🔓 Unban]  [👑 Assign Role]  [🗑 Delete]       │ │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

### User Detail Modal (With Activity Tabs)
```
┌──────────────────────────────────────────────────────────────┐
│  ┌── White Modal Card (on overlay, max-width: 700px) ────┐  │
│  │                                                        │  │
│  │           👤 Jane Doe                                  │  │
│  │        [avatar image - 80px]                           │  │
│  │         @janedoe                         [✕]          │  │
│  │                                                        │  │
│  │  Stats:  245 Outfits | 89 Posts | 12 Followers        │  │
│  │                                                        │  │
│  │  [ℹ️ Info]  [📋 Activity]  [🔑 Login History]        │  │
│  │                                                        │  │
│  │  ─── Tab: ℹ️ Info ───                                  │  │
│  │  Email:    jane@example.com                            │  │
│  │  Role:     User    [Change Role ▾]                    │  │
│  │  Status:   🟢 Active  [🔨 Ban User]  [📧 Send Email]  │  │
│  │  Joined:   Jan 15, 2026                                │  │
│  │  Last Seen: 2 hours ago                                │  │
│  │  Verified: ✅ Email | ✅ Identity | ❌ Phone           │  │
│  │                                                        │  │
│  │  ─── Tab: 📋 Activity ───                              │  │
│  │  [📅 Last 30 Days ▾]  [Type: ▼]                       │  │
│  │                                                        │  │
│  │  📅 Today                                              │  │
│  │    🕐 08:15  Created outfit "Beach Day Look"          │  │
│  │    🕐 07:45  Voted on poll "Best summer dress?"       │  │
│  │    🕐 07:30  Logged in (IP: 10.0.0.5)                │  │
│  │                                                        │  │
│  │  📅 Yesterday                                          │  │
│  │    🕐 22:10  Added item "Blue Linen Shirt"            │  │
│  │    🕐 18:30  Wore outfit "Office Casual" (4th wear)   │  │
│  │    🕐 14:00  Commented on post "Summer styles"        │  │
│  │                                                        │  │
│  │  ─── Tab: 🔑 Login History ───                        │  │
│  │  🟢 May 25, 08:15  - IP: 10.0.0.5  - Chrome/Win      │  │
│  │  🟢 May 24, 22:30  - IP: 10.0.0.5  - Chrome/Win      │  │
│  │  🟢 May 24, 14:00  - IP: 192.168.1.5 - Mobile/iOS    │  │
│  │  ❌ May 24, 13:55  - Failed login - IP: 192.168.1.5   │  │
│  │                                                        │  │
│  │  [📥 Export Activity]  [Close]                        │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

### User Activity Page (Dedicated per User)
```
┌──────────────────────────────────────────────────────────────────┐
│  👤 Jane Doe — Activity Log                    [📥 Export CSV]  │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  [📅 Last 30 Days ▾]  [Activity Type: ▼]  [🔍 Search]     │ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  📊 Activity Summary                                       │ │
│  │  Logins: 47 | Outfits: 23 | Posts: 12 | Comments: 8       │ │
│  │  Items: 18 | Reports: 5                                    │ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  📋 Activity Timeline                                      │ │
│  │  🕐 May 25, 08:15  Created outfit "Beach Day Look"        │ │
│  │  🕐 May 25, 07:45  Voted on poll "Best summer dress?"     │ │
│  │  🕐 May 25, 07:30  Login — IP: 10.0.0.5 - Chrome/Win     │ │
│  │  🕐 May 24, 22:10  Added item "Blue Linen Shirt"          │ │
│  │  🕐 May 24, 18:30  Wore outfit "Office Casual" (4th wear) │ │
│  │  🕐 May 24, 14:00  Commented on post "Summer styles"      │ │
│  │  🕐 May 24, 13:55  ❌ Failed login — IP: 192.168.1.5      │ │
│  │  🕐 May 23, 12:15  Updated profile photo                  │ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  < 1 2 3 ... 10 >    20 items / page                      │ │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 6. System Operations

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  🛠 System Operations                                            │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  ⚡ System Health                                          │ │
│  │  ● API Server     🟢 Online    99.9% uptime  [Check Now]  │ │
│  │  ● Database       🟢 Online    2.3ms avg                  │ │
│  │  ● Cache (Redis)  🟢 Online    98% hit rate               │ │
│  │  ● Storage        🟡 72% Used  1.2TB / 2TB               │ │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  📦 Quick Actions                                          │ │
│  │  [🔄 Restart Service]  [🗑 Clear Cache]  [📤 Create Backup]│ │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  📋 Recent System Logs                   [View All →]       │ │
│  │  🕐 07:30  ✅ INFO  Cache cleared by admin                │ │
│  │  🕐 07:15  ⚠️ WARN  High API latency (850ms)              │ │
│  │  🕐 23:00  ✅ INFO  Daily backup completed (1.2GB)        │ │
│  │  🕐 21:30  ❌ ERROR  Failed login ×3 from IP 10...       │ │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 7. Analytics / Reports

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  📈 Analytics & Reports                      [📥 Export CSV]    │
├──────────────────────────────────────────────────────────────────┤
│  [📅 Last 30 Days ▾]  [Compare: Previous Period ▾]             │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────┐  ┌── White Card ───────────────┐   │
│  │  📊 Platform Metrics   │  │  👥 User Metrics            │   │
│  │  Total Users   12,847  │  │  New Users/day   +124      │   │
│  │  Total Posts   5,234   │  │  Active Users    3,205     │   │
│  │  Total Outfits 8,912   │  │  Retention 30d   78%       │   │
│  │  Total Polls   1,234   │  │  Avg Session     12m       │   │
│  └────────────────────────┘  └─────────────────────────────┘   │
│                                                                  │
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  📈 Engagement Trend (Last 30 Days) — DAU/MAU chart       │ │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  🚩 Pending Reports                       [View All →]     │ │
│  │  #23 Spam — @user1 reported @user2 — 2h ago  [Resolve]    │ │
│  │  #24 Harassment — @user3 — 30m ago            [Resolve]   │ │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 8. Audit Logs

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  📋 Audit Logs                                [📥 Export]       │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  [Actor: ▼]  [Action: ▼]  [📅 Date Range: ▼]  [🔍 Filter]│ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  🕐 07:30:12  admin@outfit.com  |  🗑 Cache Cleared        │ │
│  │              IP: 192.168.1.1  |  SYSTEM_CACHE_CLEAR        │ │
│  │  ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─                          │ │
│  │  🕐 07:15:44  jane@email.com  |  📝 Post Created (#4523)  │ │
│  │              IP: 10.0.0.5  |  POST_CREATE                 │ │
│  │  ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─                          │ │
│  │  🕐 06:45:22  mark@email.com  |  ❌ Login Failed (×3)     │ │
│  │              IP: 192.168.1.50  |  AUTH_LOGIN_FAILED       │ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  < 1 2 3 4 5 ... 10 >    50 / page                        │ │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 9. Settings

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  ⚙ Settings                                                     │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  🛡 General Settings                                       │ │
│  │  Site Name:         [Outfit Planner                  ]     │ │
│  │  Site Description:  [Your AI wardrobe assistant      ]     │ │
│  │  Maintenance Mode:  [❌ Off]  [🔧 Toggle]                  │ │
│  │  Max Upload Size:   [10 MB ▾]                              │ │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  🔐 Security Settings                                      │ │
│  │  Max Login Attempts: [5 ▾]                                 │ │
│  │  Session Duration:   [24 hours ▾]                         │ │
│  │  Email Verification:  [✅ Required]  [Toggle]              │ │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  📧 Notification Settings                                  │ │
│  │  Welcome Email:     [✅ Enabled]  [Toggle]                  │ │
│  │  Password Reset:    [✅ Enabled]  [Toggle]                  │ │
│  │  Daily Digest:      [❌ Disabled] [Toggle]                  │ │
│  └────────────────────────────────────────────────────────────┘  │
│                                                     [💾 Save]   │
└──────────────────────────────────────────────────────────────────┘
```

---

## 10. AI Fashion Assistant

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  Background: #fdf2f8 with floating pink/green/blue blobs         │
│                                                                    │
│  ┌─ Navbar ────────────────────────────────────────────────────┐  │
│  │  [👕 Outfit Planner]  [Home] [Wardrobe] [Social] [Admin]   │  │
│  └─────────────────────────────────────────────────────────────┘  │
│                                                                    │
│  ┌── White Card: Chat Container ─────────────────────────────────┐ │
│  │  💬 AI Fashion Assistant                         [⚙] [✕]    │ │
│  ├───────────────────────────────────────────────────────────────┤ │
│  │  ┌────────────────────────────────────────────────────┐      │ │
│  │  │ 🤖 AI: "Hi! I'm your fashion assistant. Ask me    │      │ │
│  │  │ anything about your wardrobe!"                    │      │ │
│  │  └────────────────────────────────────────────────────┘      │ │
│  │  ┌────────────────────────────────────────────────────┐      │ │
│  │  │ 😊 You: "What should I wear for a rainy interview │      │ │
│  │  │ tomorrow?"                                         │      │ │
│  │  └────────────────────────────────────────────────────┘      │ │
│  │  ┌── White Card (inline outfit card) ────────────────┐       │ │
│  │  │  🤖 AI: "Based on your wardrobe + weather...      │       │ │
│  │  │  👔 Navy Blazer  👖 Grey Pants  👞 Oxfords       │       │ │
│  │  │  Style Score: 88/100  ✅  [💾 Save as Outfit]     │       │ │
│  │  └───────────────────────────────────────────────────┘       │ │
│  │  ┌────────────────────────────────────────────────────┐      │ │
│  │  │ 🤖 AI is thinking... ⚫ ⚫ ⚫                     │      │ │
│  │  └────────────────────────────────────────────────────┘      │ │
│  ├───────────────────────────────────────────────────────────────┤ │
│  │  [Date night?] [Casual Friday] [Beach trip] [What's missing?] │ │
│  ├───────────────────────────────────────────────────────────────┤ │
│  │  💬 Type your fashion question...                   [Send]   │ │
│  └──────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
```

---

## 11. Support System

### Support Floating Widget
```
           [💬]  ← Floating button (bottom-right)
  bg: var(--accent-pink), white icon, --card-shadow

                    ↓ Click opens slide-out panel

┌── White Card: Slide-out Panel ────────────────────────┐
│  💬 Need help?                         [−] [✕]        │
├────────────────────────────────────────────────────────┤
│  🤖 Bot: "Hi! How can I help you today?"               │
│  [🔑 Reset password] [🐛 Report bug]                   │
│  [🚩 Report user]    [💬 Talk to admin]                │
│  ┌────────────────────────────────────────────────┐   │
│  │ Type your message...                  [Send]   │   │
│  └────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────┘
```

### Support Tickets List
```
┌──────────────────────────────────────────────────────────────────┐
│  🎫 My Support Tickets                        [+ New Ticket]    │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  [All] [Open] [In Progress] [Resolved] [Closed]           │ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  ! HIGH  Login issue after email change                    │ │
│  │  Account • 2h ago • John A. • 3 messages                  │ │
│  └────────────────────────────────────────────────────────────┘  │
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  ◉ MED  Can't upload clothing photo                        │ │
│  │  Technical • 1d ago • 1 message                           │ │
│  └────────────────────────────────────────────────────────────┘  │
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  ● LOW  Feature suggestion                                 │ │
│  │  Other • 3d ago • Resolved • 5 messages                   │ │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

### Ticket Detail / Admin Support Dashboard
```
┌──────────────────────────────────────────────────────────────────┐
│  🎫 #142: Login issue                  [Cancel] [← Back]        │
│  Status: ● InProgress   Priority: ! High                        │
├──────────────────────────────────────────────────────────────────┤
│  ┌── White Card ──────────────────────────────────────────────┐ │
│  │  📌 "I can't log in after changing my email."              │ │
│  └────────────────────────────────────────────────────────────┘  │
│  ┌── White Card (Admin, left) ────────────────────────────────┐ │
│  │  🛡️ Admin John: "I've reset your account."  2:30 PM ✓     │ │
│  └────────────────────────────────────────────────────────────┘  │
│  ┌── White Card (You, right, pink bg) ────────────────────────┐ │
│  │  😊 You: "It worked! Thank you."         2:45 PM          │ │
│  └────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  Type your message...                                   [Send]  │
└──────────────────────────────────────────────────────────────────┘
```

### Content Report Modal
```
┌──────────────────────────────────────────────────────────────┐
│  🚩 Report Content                                           │
│  Why are you reporting this?                                 │
│  ○ Spam  ● Harassment  ○ Inappropriate  ○ Copyright         │
│  ○ Other: [________________]                                 │
│  ┌───────────────────────────────────────────┐              │
│  │ Description (optional)...                 │              │
│  └───────────────────────────────────────────┘              │
│  [Cancel]  [Submit Report]                                   │
└──────────────────────────────────────────────────────────────┘
```

---

## 12. Component Architecture

### Shared Components (All Use `mat-card`)

```
All pages follow this pattern:
  1. <mat-card> for every section (filters, table, actions, pagination)
  2. <mat-form-field appearance="outline"> for inputs
  3. <mat-table> for data tables
  4. <mat-paginator> for pagination
  5. <mat-checkbox> for bulk selection
  6. <mat-icon> for icons
  7. White bg (#ffffff) with --card-shadow
  8. Border radius: --radius-lg (16px)
```

### Admin Page Components
```typescript
- AdminDashboardComponent       // KPI grid + charts in mat-cards
- AdminContentComponent         // Unified content (Posts | Outfits | Polls) with tabs
- AdminUsersComponent           // mat-table + user detail modal with activity tabs
- AdminSystemComponent          // Health + actions + logs
- AdminAnalyticsComponent       // Metrics + charts + reports
- AdminAuditLogsComponent       // Filter + log timeline
- AdminSettingsComponent        // Grouped settings sections
```

### AI Chat Components
```typescript
- AiAssistantComponent          // Chat page with mat-card container
- ChatMessageComponent          // User (pink bg) / AI (white bg) bubbles
- TypingIndicatorComponent      // Animated dots
- OutfitCardPreviewComponent    // Inline outfit suggestion mat-card
- QuickSuggestionChipsComponent // Chips with --accent-pink border
- ChatInputComponent            // Form field + send button
```

### Support Components
```typescript
- SupportFabComponent           // Floating button (--accent-pink)
- SupportWidgetComponent        // Slide-out mat-card panel
- SupportTicketsComponent       // Ticket list with mat-cards
- TicketDetailComponent         // Detail with messages
- ReportContentModalComponent   // Modal mat-card
- AdminSupportDashboardComponent // Support dashboard
- AdminLiveChatComponent        // Split-pane live chat
```

---

## Status Badge Colors

| Status | Style |
|--------|-------|
| Active / Approved | `--accent-green: #9caf88` |
| Pending / Open | `--warning: #FDCB6E` |
| Rejected / Banned | `--destructive: #ef4444` |
| In Progress | `--info: #4D96FF` |
| Closed / Expired | `--muted-foreground: #6b7280` |
| Priority Low | `--muted-foreground: #6b7280` |
| Priority Medium | `--warning: #FDCB6E` |
| Priority High | `--destructive: #ef4444` |
| Priority Urgent | `--destructive: #ef4444` (pulsing) |

## Button Variants

| Button | Style |
|--------|-------|
| Primary | `background: var(--primary, #db2777); color: white` |
| Secondary | `background: var(--secondary, #fce7f3); color: #be185d` |
| Accent (pink) | `background: var(--accent-pink, #f8b4c4); color: white` |
| Success (green) | `background: var(--accent-green, #9caf88); color: white` |
| Danger | `background: var(--destructive, #ef4444); color: white` |
| Outline | `border: 1px solid var(--border); background: white` |

---

*Document Version: 2.0 — Site-Aligned Edition*
*Last Updated: May 2026*