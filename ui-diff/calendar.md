# UI Diff: Calendar Page

## Design Source: `Design/calender.html`
## Implementation: `src/outfit-planner-ui/src/app/presentation/pages/calendar/calendar.component.html`

---

## Overall Alignment: ⚠️ 75% MATCH

---

## 1. Layout Structure

| Element | Design | Implementation | Status |
|---------|--------|----------------|--------|
| Container Max Width | 1440px | N/A (full width) | ⚠️ |
| Container Padding | 24px 40px | N/A (component only) | ⚠️ |
| Main Grid | 1fr 380px | 1fr 380px | ✅ |
| Calendar Section | Card with border, rounded-lg | Same | ✅ |
| Sidebar Section | flex column, gap 24px | Same | ✅ |

---

## 2. Colors

| Element | Design | Implementation | Status |
|---------|--------|----------------|--------|
| Primary | `#db2777` | `#ff6fa8` | ⚠️ |
| Secondary | `#fce7f3` | Same | ✅ |
| Background | `#fdf2f8` | Different | ⚠️ |
| Border | `#e5e7eb` | Same | ✅ |
| Card | `#ffffff` | Same | ✅ |

---

## 3. Calendar Components

### Controls
| Element | Design | Implementation | Status |
|---------|--------|----------------|--------|
| Month Title | 20px, 600 weight | Same | ✅ |
| Control Buttons | 32x32px, border, rounded | Same | ✅ |
| View Toggles | Month/Week tabs | Month/Week | ✅ |

### Calendar Grid
| Element | Design | Implementation | Status |
|---------|--------|----------------|--------|
| Columns | repeat(7, 1fr) | Same | ✅ |
| Day Headers | 40px height, center, 13px | Same | ✅ |
| Day Cell | 8px padding, relative | Same | ✅ |
| Today (day-number.today) | Primary bg, white text | Same | ✅ |
| Selected | #fff1f2 bg, primary border | Same | ✅ |
| Other Month | #fafafa bg | Same | ✅ |

### Weather Mini
| Element | Design | Implementation | Status |
|---------|--------|----------------|--------|
| Position | absolute, top 8px right 8px | Same | ✅ |
| Font Size | 12px | Same | ✅ |

### Outfit Pills
| Element | Design | Implementation | Status |
|---------|--------|----------------|--------|
| Display | flex, gap 6px, white bg | Same | ✅ |
| Worn State | secondary bg | Same | ✅ |
| Scheduled State | white bg, border | Same | ✅ |
| Pill Image | 20x20px | Same | ✅ |

---

## 4. Sidebar Components

### Selected Date Card
| Element | Design | Implementation | Status |
|---------|--------|----------------|--------|
| Date Title | 24px, 700 | Same | ✅ |
| Day Name | 14px, muted | Same | ✅ |
| Weather Widget | #f0f9ff bg, blue text | Same | ✅ |

### Events List
| Element | Design | Implementation | Status |
|---------|--------|----------------|--------|
| Label | "EVENTS", uppercase | Same | ✅ |
| Time | 13px, 600 | Same | ✅ |
| Item | flex, gap 12px, border-bottom | Same | ✅ |

### Outfit Preview Card
| Element | Design | Implementation | Status |
|---------|--------|----------------|--------|
| Aspect Ratio | 3/4 | Same | ✅ |
| Border Radius | var(--radius-md) | Same | ✅ |
| Status Badge | absolute, top-right | Same | ✅ |
| Item Chips | muted bg, rounded | Same | ✅ |
| Buttons | Primary + Outline | Same | ✅ |

### Wear History Stats
| Element | Design | Implementation | Status |
|---------|--------|----------------|--------|
| 3 Stats | Days tracked, Adherence, New Outfits | Same | ✅ |
| Layout | flex, space-between | Same | ✅ |

---

## 5. Differences Found

### 1. Primary Color
- **Design**: `#db2777` (pink-600)
- **Implementation**: `#ff6fa8` (lighter pink)
- **Severity**: Medium - affects active states and accents

### 2. Background Color
- **Design**: `#fdf2f8` (pink-50)
- **Implementation**: Uses different background
- **Severity**: Low - likely handled globally

### 3. Week View
- **Design**: Has week view with time slots
- **Implementation**: Has week view implemented (Google Calendar style)
- **Status**: ✅ Implemented

### 4. Add Event Button
- **Design**: Not explicitly shown
- **Implementation**: Has "Add" button with mat-icon
- **Status**: Enhancement ✅

---

## Missing Features

1. **Page Header** - The design shows full header with logo, nav tabs, and user. This is likely handled by global navbar.

---

## CSS Architecture Notes

### Strengths
- Comprehensive calendar functionality
- Both month and week views implemented
- Proper handling of weather display
- Event management features

### Issues
1. Primary color doesn't match design
2. Component assumes global styling for background