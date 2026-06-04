# Outfit-Planner: Admin Panel — Glassmorphism Design

## Design Philosophy

**Glassmorphism** — frosted glass aesthetics with layered transparency, subtle backdrop blur, and a soft gradient background. The interface feels light, airy, and modern, like working on a high-end fashion app.

---

## 1. Design Tokens

### Background
```
Main Background:
  Linear gradient 135deg: #667eea → #764ba2  (purple-blue sunset)
  or
  Linear gradient 135deg: #f093fb → #f5576c  (pink-coral sunset)
  or
  Animated gradient that slowly shifts

Glass Cards:
  background: rgba(255, 255, 255, 0.15)
  backdrop-filter: blur(20px)
  -webkit-backdrop-filter: blur(20px)
  border: 1px solid rgba(255, 255, 255, 0.25)
  border-radius: 16px
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1)

Elevated Glass (hover/active):
  background: rgba(255, 255, 255, 0.25)
  backdrop-filter: blur(24px)
  border: 1px solid rgba(255, 255, 255, 0.35)
  box-shadow: 0 12px 48px rgba(0, 0, 0, 0.15)

Sidebar Glass:
  background: rgba(0, 0, 0, 0.2)
  backdrop-filter: blur(30px)
  border-right: 1px solid rgba(255, 255, 255, 0.1)
```

### Color Palette

| Token | Value | Usage |
|-------|-------|-------|
| Primary | #F8B4C4 | Accents, active nav, CTAs |
| Primary Glow | rgba(248, 180, 196, 0.3) | Glow effects, hover states |
| Secondary | #A78BFA | Secondary accents, charts |
| Text Primary | rgba(255, 255, 255, 0.95) | Headlines, body text |
| Text Secondary | rgba(255, 255, 255, 0.65) | Captions, metadata |
| Text Muted | rgba(255, 255, 255, 0.4) | Placeholder, disabled |
| Glass Light | rgba(255, 255, 255, 0.15) | Card backgrounds |
| Glass Medium | rgba(255, 255, 255, 0.25) | Hovered cards |
| Glass Heavy | rgba(255, 255, 255, 0.35) | Active/selected |
| Error | #FF6B6B | Error states |
| Warning | #FFD93D | Warning states |
| Success | #6BCB77 | Success states |
| Info | #4D96FF | Info states |

### Glassmorphism Status & Priority Badges

```
Status Open:      background: rgba(255, 217, 61, 0.2)  border: 1px solid rgba(255, 217, 61, 0.4)
Status InProgress: background: rgba(77, 150, 255, 0.2)  border: 1px solid rgba(77, 150, 255, 0.4)
Status Resolved:  background: rgba(107, 203, 119, 0.2) border: 1px solid rgba(107, 203, 119, 0.4)
Status Closed:    background: rgba(255, 255, 255, 0.1)  border: 1px solid rgba(255, 255, 255, 0.2)

Priority Low:     background: rgba(255, 255, 255, 0.1)  text: rgba(255,255,255,0.5)
Priority Medium:  background: rgba(255, 217, 61, 0.15)  text: #FFD93D
Priority High:    background: rgba(255, 107, 107, 0.15) text: #FF6B6B
Priority Urgent:  background: rgba(255, 107, 107, 0.25) text: #FF6B6B  animation: pulse-glow
```

### Typography

| Element | Font | Size | Weight | Style |
|---------|------|------|--------|-------|
| Page Title | Inter | 28px | 700 | letter-spacing: -0.5px |
| Section Title | Inter | 20px | 600 | |
| Card Title | Inter | 16px | 600 | |
| Table Header | Inter | 12px | 600 | letter-spacing: 0.5px, uppercase |
| Table Cell | Inter | 14px | 400 | |
| Caption | Inter | 12px | 400 | |
| Badge Text | Inter | 11px | 600 | letter-spacing: 0.3px, uppercase |

### Spacing & Layout

| Token | Value |
|-------|-------|
| Page padding | 32px |
| Card padding | 24px |
| Card gap (grid) | 20px |
| Section margin-bottom | 28px |
| Sidebar width | 260px |
| Header height | 64px |

### Glass Effects

```
/* Subtle glass for cards */
.glass-card {
  background: rgba(255, 255, 255, 0.12);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 16px;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
}

/* Glass sidebar */
.glass-sidebar {
  background: rgba(0, 0, 0, 0.15);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border-right: 1px solid rgba(255, 255, 255, 0.08);
}

/* Glass modal/dialog */
.glass-modal {
  background: rgba(255, 255, 255, 0.18);
  backdrop-filter: blur(32px);
  -webkit-backdrop-filter: blur(32px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 20px;
  box-shadow: 0 24px 80px rgba(0, 0, 0, 0.2);
}

/* Glass table */
.glass-table {
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(12px);
  border-radius: 16px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

/* Glass table row hover */
.glass-table-row:hover {
  background: rgba(255, 255, 255, 0.12);
}

/* Glass input */
.glass-input {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 10px;
  color: white;
}
.glass-input:focus {
  border-color: rgba(248, 180, 196, 0.5);
  box-shadow: 0 0 0 3px rgba(248, 180, 196, 0.15);
}

/* Glass button */
.glass-button {
  background: rgba(255, 255, 255, 0.12);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 10px;
  color: rgba(255, 255, 255, 0.9);
  transition: all 0.2s;
}
.glass-button:hover {
  background: rgba(255, 255, 255, 0.2);
  border-color: rgba(255, 255, 255, 0.3);
  transform: translateY(-1px);
}

/* Primary glass button */
.glass-button-primary {
  background: rgba(248, 180, 196, 0.25);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(248, 180, 196, 0.4);
  border-radius: 10px;
  color: #fff;
}
.glass-button-primary:hover {
  background: rgba(248, 180, 196, 0.35);
  box-shadow: 0 4px 20px rgba(248, 180, 196, 0.2);
}
```

---

## 2. Admin Layout — Glassmorphism

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  Background: Animated gradient or abstract fashion pattern        │
│                                                                    │
│  ┌──────────────┬───────────────────────────────────────────┐    │
│  │  Glass        │  Glass Header                             │    │
│  │  Sidebar      │  ┌─────────────────────────────────┐     │    │
│  │  (blur:24px)  │  │  🌤 Outfit Planner     [🔔][👤]  │     │    │
│  │               │  └─────────────────────────────────┘     │    │
│  │  [logo]       │                                           │    │
│  │  ───────      │  ┌─ Glass Content Area ──────────────┐   │    │
│  │               │  │                                     │   │    │
│  │  📊 Dashboard │  │  ┌──────────────────────────────┐  │   │    │
│  │  👥 Users     │  │  │  Page Title          [+New]  │  │   │    │
│  │  📝 Posts     │  │  └──────────────────────────────┘  │   │    │
│  │  👕 Outfits   │  │                                     │   │    │
│  │  📋 Polls     │  │  ┌─ Glass Card ──────────────────┐ │   │    │
│  │  📈 Analytics │  │  │                               │ │   │    │
│  │  🛠 System    │  │  │  Content (table / cards /     │ │   │    │
│  │  📋 Audit     │  │  │  charts / form)               │ │   │    │
│  │  ⚙ Settings   │  │  │                               │ │   │    │
│  │  🎫 Support   │  │  └───────────────────────────────┘ │   │    │
│  │               │  │                                     │   │    │
│  │  ───────      │  │  ┌─ Glass Paginator ─────────────┐ │   │    │
│  │  🚪 Logout    │  │  │  < 1 2 3 ... 10 >    50/page │ │   │    │
│  │               │  │  └───────────────────────────────┘ │   │    │
│  └──────────────┘  └─────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
```

### Sidebar Nav Items

```
┌──────────────────┐
│ [Fashion icon]   │  ← Logo in glass circle
│ Outfit Planner   │
│ Admin Panel      │
│                  │
│ ─── Main ───     │
│                  │
│ 📊 Dashboard     │  ← Active: glass highlight with pink left border
│ 👥 Users         │
│ 📝 Posts         │
│ 👕 Outfits       │
│ 📋 Polls         │
│                  │
│ ─── Insights ─── │
│                  │
│ 📈 Analytics     │
│ 📋 Audit Logs    │
│ 🎫 Support       │
│                  │
│ ─── System ───  │
│                  │
│ 🛠 Operations    │
│ ⚙ Settings       │
│                  │
│ ─────────────── │
│                  │
│ 🚪 Logout        │
└──────────────────┘
```

---

## 3. Dashboard (Glassmorphism)

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  📊 Dashboard                                            [🔄]   │
├──────────────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  Glass KPI   │  │  Glass KPI   │  │  Glass KPI   │          │
│  │              │  │              │  │              │          │
│  │ 👥 12,847   │  │ 📈 +124     │  │ 🟢 3,205    │          │
│  │ Total Users  │  │ New Today    │  │ Active Users │          │
│  │ ▲ 12%       │  │ ▲ 8%        │  │ ▼ 2%        │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                  │
│  ┌──────────────┐  ┌──────────────┐                             │
│  │  Glass KPI   │  │  Glass KPI   │                             │
│  │              │  │              │                             │
│  │ 🚩 23       │  │ 🔒 2        │                             │
│  │ Pending Rpts │  │ Locked Accts │                             │
│  │ ▲ 5%        │  │ — 0%        │                             │
│  └──────────────┘  └──────────────┘                             │
├──────────────────────────────────────────────────────────────────┤
│  ┌───────────────────────────┐  ┌───────────────────────────┐   │
│  │  Glass Chart Card         │  │  Glass Chart Card         │   │
│  │  📈 User Registration     │  │  📊 Content Creation      │   │
│  │                           │  │                           │   │
│  │  [gradient line chart     │  │  [gradient bar chart      │   │
│  │   with glass styling]     │  │   with glass styling]     │   │
│  │                           │  │                           │   │
│  │  ───────────────          │  │  ───────────────          │   │
│  │  ▲ +12.4% vs last month  │  │  ▲ +8.2% vs last month   │   │
│  └───────────────────────────┘  └───────────────────────────┘   │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Glass Chart Card                                         │  │
│  │  🚩 Report Status                                         │  │
│  │                                                           │  │
│  │  [gradient doughnut chart with glass styling]             │  │
│  │                                                           │  │
│  │  Pending: 23  |  Reviewed: 156  |  Resolved: 89          │  │
│  └───────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 4. Posts Management (Glassmorphism)

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  📝 Posts Management                            [+ New Post]    │
├──────────────────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Glass Filter Bar                                         │ │
│  │  [🔍 Search posts...]  [Status: ▼]  [Category: ▼]  [📅]  │ │
│  └────────────────────────────────────────────────────────────┘ │
├──────────────────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Glass Table                                               │ │
│  │  ┌─────┬───────────┬──────────┬────────┬───────┬────────┐ │ │
│  │  │  ☐  │ Title     │ Author   │ Status │ Likes │ Date   │ │ │
│  │  ├─────┼───────────┼──────────┼────────┼───────┼────────┤ │ │
│  │  │  ☐  │ Summer..  │ @jane    │ 🟢 Aprv│ 245   │ 2h ago │ │ │
│  │  │  ☐  │ Beach..   │ @mark    │ ⏳ Pndg│ 12    │ 30m ago│ │ │
│  │  │  ☐  │ Casual..  │ @lisa    │ 🔴 Rej │ 0     │ 1d ago │ │ │
│  │  └─────┴───────────┴──────────┴────────┴───────┴────────┘ │ │
│  └────────────────────────────────────────────────────────────┘ │
├──────────────────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Glass Action Bar                                         │ │
│  │  [✅ Approve]  [❌ Reject]  [🗑 Delete]                    │ │
│  └────────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Glass Paginator                                          │ │
│  │  < 1  2  3  4  5  ...  10  >    50 / page  [►]            │ │
│  └────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
```

---

## 5. Users Management (Glassmorphism)

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  👥 Users Management                            [+ Invite User]  │
├──────────────────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Glass Filter Bar                                         │ │
│  │  [🔍 Search by name or email...]  [Role: ▼]  [Status: ▼] │ │
│  └────────────────────────────────────────────────────────────┘ │
├──────────────────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Glass Table                                               │ │
│  │  ┌─────┬──────────┬────────────┬────────┬────────┬──────┐ │ │
│  │  │  ☐  │ Name     │ Email      │ Role   │ Status │ Jnd │ │ │
│  │  ├─────┼──────────┼────────────┼────────┼────────┼──────┤ │ │
│  │  │  ☐  │ Jane D.  │ j@e.com    │ User   │ 🟢 On  │J'26 │ │ │
│  │  │  ☐  │ Mark S.  │ m@e.com    │ Admin  │ 🔴 Off │D'25 │ │ │
│  │  │  ☐  │ Lisa K.  │ l@e.com    │ Moder  │ 🟡 Away│M'26 │ │ │
│  │  └─────┴──────────┴────────────┴────────┴────────┴──────┘ │ │
│  └────────────────────────────────────────────────────────────┘ │
├──────────────────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Glass Action Bar                                         │ │
│  │  [🔨 Ban]  [🔓 Unban]  [👑 Assign Role]  [🗑 Delete]      │ │
│  └────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
```

---

## 6. User Detail Modal (Glassmorphism)

### Component Structure
```
┌──────────────────────────────────────────────────────────────┐
│  ┌─ Glass Modal Backdrop (blur:8px, dark overlay) ────────┐ │
│  │                                                          │ │
│  │  ┌─── Glass Modal ───────────────────────────────────┐  │ │
│  │  │                                                    │  │ │
│  │  │           👤                                     │  │ │
│  │  │        [Glass avatar circle]                      │  │ │
│  │  │         Jane Doe                                  │  │ │
│  │  │         @janedoe                     [✕ Close]    │  │ │
│  │  │                                                    │  │ │
│  │  │  ┌──────────┐  ┌──────────┐  ┌──────────┐       │  │ │
│  │  │  │ 245      │  │ 89       │  │ 12       │       │  │ │
│  │  │  │ Outfits  │  │ Posts    │  │ Followers│       │  │ │
│  │  │  └──────────┘  └──────────┘  └──────────┘       │  │ │
│  │  │                                                    │  │ │
│  │  │  ─── Details ───                                   │  │ │
│  │  │  Email:     jane@example.com                       │  │ │
│  │  │  Role:      User         [Change Role ▾]          │  │ │
│  │  │  Status:    🟢 Active    [🔨 Ban User]             │  │ │
│  │  │  Joined:    Jan 15, 2026                           │  │ │
│  │  │  Last Seen: 2 hours ago                            │  │ │
│  │  │  Verified:  ✅ Email | ✅ Identity                  │  │ │
│  │  └────────────────────────────────────────────────────┘  │ │
│  └──────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
```

---

## 7. System Operations (Glassmorphism)

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  🛠 System Operations                                            │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Health Card ─────────────────────────────────────┐  │
│  │  ⚡ System Health                                          │  │
│  │                                                           │  │
│  │  ● API Server     🟢 Online    99.9% uptime   [Check]    │  │
│  │  ● Database       🟢 Online    2.3ms avg                 │  │
│  │  ● Cache (Redis)  🟢 Online    98% hit rate              │  │
│  │  ● Storage        🟡 72% Used  1.2TB / 2TB              │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌─── Glass Actions Card ────────────────────────────────────┐  │
│  │  📦 Quick Actions                                          │  │
│  │                                                           │  │
│  │  [🔄 Restart Service]  [🗑 Clear Cache]  [📤 Create Backup]│  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌─── Glass Logs Card ───────────────────────────────────────┐  │
│  │  📋 Recent System Logs                    [View All →]     │  │
│  │                                                           │  │
│  │  🕐 07:30  INFO  Cache cleared by admin                  │  │
│  │  🕐 07:15  ⚠ WARN  High API latency detected (850ms)    │  │
│  │  🕐 23:00  INFO  Daily backup completed (1.2GB)          │  │
│  │  🕐 22:45  ✅ OK  Health check passed                    │  │
│  │  🕐 21:30  ❌ ERR  Failed login attempt ×3 from IP 10.. │  │
│  └───────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 8. Analytics (Glassmorphism)

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  📈 Analytics & Reports                        [📥 Export CSV]  │
├──────────────────────────────────────────────────────────────────┤
│  [📅 Last 30 Days ▾]  [Compare: Previous Period ▾]             │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Metrics Card ───┐  ┌─── Glass Metrics Card ────┐  │
│  │ 📊 Platform Metrics      │  │ 👥 User Metrics            │  │
│  │                          │  │                            │  │
│  │ Total Users   12,847     │  │ New Users/day   +124      │  │
│  │ Total Posts   5,234      │  │ Active Users    3,205     │  │
│  │ Total Outfits 8,912      │  │ Retention 30d   78%       │  │
│  │ Total Polls   1,234      │  │ Avg Session     12m       │  │
│  └──────────────────────────┘  └────────────────────────────┘  │
│                                                                  │
│  ┌─── Glass Chart Card ─────────────────────────────────────┐  │
│  │  📈 Engagement Trend (Last 30 Days)                      │  │
│  │                                                          │  │
│  │  [gradient area chart — DAU/MAU with glass styling]     │  │
│  │                                                          │  │
│  │  Peak: 4,521 users on May 20  |  Avg: 3,205 users/day  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌─── Glass Reports Card ────────────────────────────────────┐  │
│  │  🚩 Pending Reports                         [View All →]  │  │
│  │                                                           │  │
│  │  #23 🚩 Spam — @user1 reported @user2 — 2h ago  [Resolve]│  │
│  │  #24 🚩 Harassment — @user3 — 30m ago          [Resolve]│  │
│  │  #25 🚩 Inappropriate — @user4 — 5m ago        [Resolve]│  │
│  └───────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 9. Audit Logs (Glassmorphism)

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  📋 Audit Logs                                 [📥 Export]      │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Filter Bar ──────────────────────────────────────┐  │
│  │  [Actor: ▼]  [Action: ▼]  [📅 Date Range: ▼]  [🔍 Filter]│  │
│  └───────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Log List ─────────────────────────────────────────┐  │
│  │                                                             │  │
│  │  🕐 07:30:12                                                │  │
│  │  admin@outfit.com  |  🗑 Cache Cleared                     │  │
│  │  IP: 192.168.1.1  |  SYSTEM_CACHE_CLEAR                    │  │
│  │  ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─                              │  │
│  │                                                             │  │
│  │  🕐 07:15:44                                                │  │
│  │  jane@email.com  |  📝 Post Created                        │  │
│  │  IP: 10.0.0.5  |  POST_CREATE (#4523)                     │  │
│  │  ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─                              │  │
│  │                                                             │  │
│  │  🕐 06:45:22                                                │  │
│  │  mark@email.com  |  ❌ Login Failed (×3)                   │  │
│  │  IP: 192.168.1.50  |  AUTH_LOGIN_FAILED                    │  │
│  │  ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─                              │  │
│  │                                                             │  │
│  └───────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Paginator ───────────────────────────────────────┐  │
│  │  <  1  2  3  4  5  ...  10  >    50 / page               │  │
│  └───────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 10. Settings (Glassmorphism)

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  ⚙ Settings                                                     │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Settings Section ───────────────────────────────┐   │
│  │  🛡 General Settings                                     │   │
│  │                                                          │   │
│  │  Site Name:        ┌─────────────────────────────┐      │   │
│  │                    │ Outfit Planner              │      │   │
│  │                    └─────────────────────────────┘      │   │
│  │                                                          │   │
│  │  Site Description: ┌─────────────────────────────┐      │   │
│  │                    │ Your AI wardrobe assistant  │      │   │
│  │                    └─────────────────────────────┘      │   │
│  │                                                          │   │
│  │  Maintenance Mode:  [❌ Off]           [🔧 Toggle]       │   │
│  │  Max Upload Size:   [10 MB ▾]                           │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌─── Glass Settings Section ───────────────────────────────┐   │
│  │  🔐 Security Settings                                    │   │
│  │                                                          │   │
│  │  Max Login Attempts: [5 ▾]                               │   │
│  │  Session Duration:   [24 hours ▾]                       │   │
│  │  Require Email Verification:  [✅ On]     [Toggle]      │   │
│  │  Two-Factor Auth:             [❌ Off]     [Toggle]      │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌─── Glass Settings Section ───────────────────────────────┐   │
│  │  📧 Notification Settings                                │   │
│  │                                                          │   │
│  │  Welcome Email:     [✅ Enabled]    [Toggle]             │   │
│  │  Password Reset:    [✅ Enabled]    [Toggle]             │   │
│  │  Daily Digest:      [❌ Disabled]   [Toggle]             │   │
│  │  Support Tickets:   [✅ Enabled]    [Toggle]             │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│                                                     [💾 Save]   │
└──────────────────────────────────────────────────────────────────┘
```

---

## 11. AI Fashion Assistant — Glassmorphism

### Page Structure
```
┌──────────────────────────────────────────────────────────────────┐
│  Background: Subtle animated gradient (purple → pink → blue)     │
│                                                                    │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │  Glass Chat Container                                      │  │
│  │  ┌────────────────────────────────────────────────────┐   │  │
│  │  │  💬 AI Fashion Assistant                [⚙] [✕]   │   │  │
│  │  ├────────────────────────────────────────────────────┤   │  │
│  │  │                                                    │   │  │
│  │  │  ┌────────────────────────────────────────┐       │   │  │
│  │  │  │ 🤖 AI: "Hi! I'm your fashion           │       │   │  │
│  │  │  │ assistant. Ask me anything about        │       │   │  │
│  │  │  │ your wardrobe!"                        │       │   │  │
│  │  │  └────────────────────────────────────────┘       │   │  │
│  │  │                                                    │   │  │
│  │  │  ┌────────────────────────────────────────┐       │   │  │
│  │  │  │ 😊 You: "What should I wear           │       │   │  │
│  │  │  │ for a rainy interview tomorrow?"      │       │   │  │
│  │  │  └────────────────────────────────────────┘       │   │  │
│  │  │                                                    │   │  │
│  │  │  ┌─── Glass Outfit Card ─────────────────────┐    │   │  │
│  │  │  │  🤖 AI: "Based on your wardrobe +        │    │   │  │
│  │  │  │  weather (rain, 18°C)...                 │    │   │  │
│  │  │  │                                          │    │   │  │
│  │  │  │  👔 Navy Blazer     👖 Grey Pants       │    │   │  │
│  │  │  │  👞 Brown Oxfords   🧥 Beige Coat       │    │   │  │
│  │  │  │                                          │    │   │  │
│  │  │  │  Style Score: 88/100  ✅                │    │   │  │
│  │  │  │              [💾 Save as Outfit]         │    │   │  │
│  │  │  └──────────────────────────────────────────┘    │   │  │
│  │  │                                                    │   │  │
│  │  │  ┌────────────────────────────────────────┐       │   │  │
│  │  │  │ 🤖 AI is thinking...                   │       │   │  │
│  │  │  │ ⚫ ⚫ ⚫                                 │       │   │  │
│  │  │  └────────────────────────────────────────┘       │   │  │
│  │  │                                                    │   │  │
│  │  ├────────────────────────────────────────────────────┤   │  │
│  │  │  [Date night?]  [Casual Friday]  [Beach trip]     │   │  │
│  │  │  [What's missing?]  [Rate my outfit]              │   │  │
│  │  ├────────────────────────────────────────────────────┤   │  │
│  │  │  ┌──────────────────────────────────────────┐ [▶] │   │  │
│  │  │  │ 💬 Type your fashion question...          │     │   │  │
│  │  │  └──────────────────────────────────────────┘     │   │  │
│  │  └────────────────────────────────────────────────────┘   │  │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 12. Support System — Glassmorphism

### Support Floating Widget
```
┌─────────────────────────────────────────────────────┐
│              [💬]  ← Glass FAB button               │
│  background: rgba(248, 180, 196, 0.25)              │
│  backdrop-filter: blur(16px)                        │
│  border: 1px solid rgba(248, 180, 196, 0.4)        │
└─────────────────────────────────────────────────────┘
                    ↓ Click opens

┌─── Glass Slide-Out Panel ──────────────────────────┐
│  💬 Need help?                        [−] [✕]     │
├────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────────┐   │
│  │ 🤖 Bot: "Hi! I'm your support assistant.  │   │
│  │ How can I help you today?"                │   │
│  │                                           │   │
│  │ [🔑 Reset password]  [🐛 Report bug]      │   │
│  │ [🚩 Report user]     [💬 Talk to admin]   │   │
│  └────────────────────────────────────────────┘   │
│                                                    │
│  ┌─ Glass Input ─────────────────────────────┐    │
│  │ Type your message...              [Send]  │    │
│  └────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────┘
```

### Support Tickets List
```
┌──────────────────────────────────────────────────────────────────┐
│  🎫 My Support Tickets                        [+ New Ticket]    │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Filter Tabs ─────────────────────────────────────┐  │
│  │  [All] [Open] [In Progress] [Resolved] [Closed]           │  │
│  └───────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Ticket Card ─────────────────────────────────────┐  │
│  │  ! HIGH  Login issue after email change                    │  │
│  │  Account  •  Opened 2h ago  •  John A.                    │  │
│  │  ─────────────────────────────────────────────             │  │
│  │  Status: ● InProgress  •  3 messages                      │  │
│  └───────────────────────────────────────────────────────────┘  │
│  ┌─── Glass Ticket Card ─────────────────────────────────────┐  │
│  │  ◉ MED  Can't upload clothing photo                       │  │
│  │  Technical  •  Opened 1d ago                              │  │
│  │  ─────────────────────────────────────────────             │  │
│  │  Status: ○ Open  •  1 message                             │  │
│  └───────────────────────────────────────────────────────────┘  │
│  ┌─── Glass Ticket Card ─────────────────────────────────────┐  │
│  │  ● LOW  Feature suggestion for color filter               │  │
│  │  Other  •  Opened 3d ago                                  │  │
│  │  ─────────────────────────────────────────────             │  │
│  │  Status: ● Resolved  •  5 messages                        │  │
│  └───────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

### Ticket Detail
```
┌──────────────────────────────────────────────────────────────────┐
│  🎫 #142: Login issue               [Cancel] [← Back]           │
│  Status: ● InProgress   Priority: ! High                        │
│  Assigned to: John A. (Admin)                                   │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Detail Card ──────────────────────────────────────┐ │
│  │  📌 Originally reported:                                  │ │
│  │  "I can't log in after changing my email. It keeps        │ │
│  │  saying invalid credentials."                             │ │
│  └───────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ─── Messages ───                                                │
│                                                                  │
│  ┌─── Glass Message Bubble (Admin, left) ────────────────────┐  │
│  │  🛡️ Admin John A.: "I've reset your account. Try         │  │
│  │  logging in with your new email. Let me know if it        │  │
│  │  works."                           2:30 PM  ✓ Read       │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌─── Glass Message Bubble (User, right) ────────────────────┐  │
│  │  😊 You: "It worked! Thank you."       2:45 PM           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Input ────────────────────────────────────────────┐ │
│  │  Type your message...                           [Send]     │ │
│  └───────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
```

### Content Report Modal
```
┌──────────────────────────────────────────────────────────────────┐
│  ┌─── Glass Modal Backdrop (blur:8px) ───────────────────────┐  │
│  │                                                            │  │
│  │  ┌─── Glass Modal ────────────────────────────────────┐   │  │
│  │  │  🚩 Report Content                                 │   │  │
│  │  │                                                     │   │  │
│  │  │  Why are you reporting this?                       │   │  │
│  │  │                                                     │   │  │
│  │  │  Reason:                                            │   │  │
│  │  │  ○ Spam                                             │   │  │
│  │  │  ● Harassment                                       │   │  │
│  │  │  ○ Inappropriate                                    │   │  │
│  │  │  ○ Copyright violation                              │   │  │
│  │  │  ○ Other: [________________]                       │   │  │
│  │  │                                                     │   │  │
│  │  │  Description:                                       │   │  │
│  │  │  ┌─────────────────────────────────────────┐       │   │  │
│  │  │  │ This user has been posting offensive    │       │   │  │
│  │  │  │ comments on my outfits repeatedly.     │       │   │  │
│  │  │  └─────────────────────────────────────────┘       │   │  │
│  │  │                                                     │   │  │
│  │  │  [Cancel]  [Submit Report]                         │   │  │
│  │  └─────────────────────────────────────────────────────┘   │  │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

### Admin Support Dashboard
```
┌──────────────────────────────────────────────────────────────────┐
│  🎫 Support Dashboard                                            │
├──────────────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  Glass KPI   │  │  Glass KPI   │  │  Glass KPI   │          │
│  │  12 Open     │  │  5 In Progr  │  │  8 Resolved  │          │
│  │  Tickets     │  │              │  │  Today       │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│  ┌──────────────┐                                               │
│  │  Glass KPI   │                                               │
│  │  4.2h Avg    │                                               │
│  │  Response Tm │                                               │
│  └──────────────┘                                               │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Filter Tabs ─────────────────────────────────────┐  │
│  │  [All] [Open] [In Progress] [Urgent] [Assigned to Me]     │  │
│  └───────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Table ────────────────────────────────────────────┐ │
│  │  ┌────┬────────────┬──────────┬────────┬──────┬────────┐  │ │
│  │  │ #  │ Subject    │ User     │ Status │ Prio │ Time   │  │ │
│  │  ├────┼────────────┼──────────┼────────┼──────┼────────┤  │ │
│  │  │142 │ Login issue│ john_d   │ ● InP  │ ! Hi │ 2h     │  │ │
│  │  │141 │ Bug report │ sarah_m  │ ○ Open │ ◉ Md │ 5h     │  │ │
│  │  │140 │ Report usr │ bob_k    │ ○ Open │ !! Ur│ 30m    │  │ │
│  │  │139 │ Can't upl. │ emma_l   │ ● InP  │ ◉ Md │ 1d     │  │ │
│  │  └────┴────────────┴──────────┴────────┴──────┴────────┘  │ │
│  └───────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│  ┌─── Glass Live Chat Monitor ───────────────────────────────┐  │
│  │  💬 Live Chat Monitor                                     │  │
│  │                                                           │  │
│  │  🟢 User #142 is online...                     [Open]     │  │
│  │  ⚫ User #139 was active 5m ago                [Open]     │  │
│  └───────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 13. Component Architecture (Glassmorphism)

### Shared Components
```typescript
// Core glass components
- GlassCardComponent          // Reusable frosted glass card wrapper
- GlassTableComponent         // Frosted glass data table
- GlassInputComponent         // Frosted glass text input
- GlassButtonComponent        // Frosted glass button (primary/secondary/danger)
- GlassSelectComponent        // Frosted glass dropdown
- GlassModalComponent         // Frosted glass modal dialog
- GlassBadgeComponent         // Frosted glass status/priority badge
- GlassPaginatorComponent     // Frosted glass pagination
- GlassFilterBarComponent     // Frosted glass filter row
- GlassToggleComponent        // Frosted glass toggle switch
- GlassAvatarComponent        // Frosted glass avatar circle
- GlassKpiCardComponent       // Frosted glass KPI metric card
- GlassChartCardComponent     // Frosted glass chart container
- GlassNotificationDot        // Pulsing glass notification indicator

// Admin layout
- AdminGlassLayoutComponent   // Glass sidebar + header + content
- AdminGlassSidebarComponent  // Glass nav sidebar with active states
- AdminGlassHeaderComponent   // Glass top header bar

// Page-level containers
- AdminDashboardComponent     // KPI grid + chart cards
- AdminPostsComponent         // Filter + table + actions
- AdminOutfitsComponent       // Filter + table + actions
- AdminPollsComponent         // Filter + table + actions
- AdminUsersComponent         // Filter + table + actions + detail modal
- AdminSystemComponent        // Health + actions + logs
- AdminAnalyticsComponent     // Metrics + charts + reports
- AdminAuditLogsComponent     // Filter + log list
- AdminSettingsComponent      // Settings sections + save
- AdminUserDetailModalComponent  // Glass modal for user details
- AdminRoleDialogComponent    // Glass modal for role assignment

// AI Chat
- AiAssistantComponent        // Full glass chat container
- GlassChatMessageComponent   // Message bubble with glass styling
- GlassTypingIndicator        // Animated dots with glass styling
- GlassOutfitCardComponent    // Inline outfit suggestion with glass styling
- GlassSuggestionChips        // Quick action chips with glass styling
- GlassChatInputComponent     // Chat input with glass styling

// Support System
- SupportFabComponent         // Glass floating action button
- SupportGlassWidgetComponent // Glass slide-out chat panel
- SupportTicketsComponent     // Glass ticket list
- TicketGlassCardComponent    // Glass ticket summary card
- TicketGlassDetailComponent  // Glass ticket detail + messages
- ReportGlassModalComponent   // Glass report content modal
- AdminSupportGlassDashboard  // Glass admin support dashboard
- AdminLiveChatComponent      // Glass split-pane live chat
```

### CSS Animations
```css
/* Background gradient animation */
@keyframes gradient-shift {
  0%   { background-position: 0% 50%; }
  50%  { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
}

.glass-bg {
  background: linear-gradient(135deg, #667eea, #764ba2, #f093fb, #f5576c);
  background-size: 400% 400%;
  animation: gradient-shift 15s ease infinite;
}

/* Card hover lift */
.glass-card {
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}
.glass-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 48px rgba(0, 0, 0, 0.15);
}

/* Pulsing glow for urgent priority */
@keyframes pulse-glow {
  0%, 100% { box-shadow: 0 0 8px rgba(255, 107, 107, 0.3); }
  50%      { box-shadow: 0 0 24px rgba(255, 107, 107, 0.6); }
}
.glass-badge-urgent {
  animation: pulse-glow 2s ease-in-out infinite;
}

/* Skeleton loading shimmer */
@keyframes glass-shimmer {
  0%   { background-position: -200% 0; }
  100% { background-position: 200% 0; }
}
.glass-skeleton {
  background: linear-gradient(90deg,
    rgba(255,255,255,0.05) 25%,
    rgba(255,255,255,0.15) 50%,
    rgba(255,255,255,0.05) 75%);
  background-size: 200% 100%;
  animation: glass-shimmer 1.5s ease-in-out infinite;
  border-radius: 12px;
}
```

---

## Glassmorphism Implementation Notes

### CSS Backdrop-Filter Support
```
/* Standard */
backdrop-filter: blur(20px);
-webkit-backdrop-filter: blur(20px);

/* Fallback for browsers that don't support backdrop-filter */
.glass-card {
  background: rgba(255, 255, 255, 0.85);  /* solid fallback */
}
@supports (backdrop-filter: blur(20px)) {
  .glass-card {
    background: rgba(255, 255, 255, 0.15);
    backdrop-filter: blur(20px);
  }
}
```

### Dark Mode Consideration
```
/* Dark mode: same glass effect, darker base */
@media (prefers-color-scheme: dark) {
  .glass-card {
    background: rgba(0, 0, 0, 0.2);
    border-color: rgba(255, 255, 255, 0.1);
  }
  .glass-input {
    background: rgba(0, 0, 0, 0.2);
  }
}
```

### Background Options
```
Option 1: Gradient (default)
  background: linear-gradient(135deg, #667eea, #764ba2)

Option 2: Fashion-themed 
  background: linear-gradient(135deg, #f8b4c4, #9caf88, #667eea)

Option 3: Abstract particles
  Use a subtle SVG particle pattern or floating shapes behind glass

Option 4: Animated gradient
  background: linear-gradient(135deg, #667eea, #764ba2, #f093fb, #f5576c)
  animation: gradient-shift 15s ease infinite
```

---

*Document Version: 1.0 — Glassmorphism Edition*
*Last Updated: May 2026*