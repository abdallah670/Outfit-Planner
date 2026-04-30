# Outfit Planner Page Implementation Plans

## 1. Wardrobe Management Page

### Page Structure
```
- Header with title and statistics summary
- Main content area with:
  * Grid/List toggle button
  * Filter sidebar (categories, colors, seasons, occasions)
  * Clothing items grid with drag-and-drop capability
  * Bulk selection mode
- Statistics cards showing:
  * Health percentage
  * Total items count
  * Most worn item
  * Cost per wear analysis
```

### Component Architecture
```typescript
// Core Components
- WardrobeDashboardPage (main container)
- WardrobeHeader (stats summary)
- WardrobeFilters (sidebar)
- ClothingGrid (responsive grid)
- ClothingCard (individual item)
- StatsCard (analytics display)
- BulkActionToolbar (when items selected)
```

### State Management
- NgRx feature for wardrobe state
- Filter state management
- Drag-and-drop state
- Bulk selection state

### Backend Integration
- Connect to `WardrobeController` endpoints:
  - GET `/api/wardrobe` - get all items
  - GET `/api/wardrobe/health` - get statistics
  - POST `/api/wardrobe/{id}/wear` - record wear event

## 2. Outfit Builder Page

### Page Structure
```
- Left sidebar: Clothing items catalog
- Center: Outfit canvas (drop zone)
- Right sidebar: Saved outfits
- Bottom: Action buttons (save, share, wear)
```

### Drag-and-Drop Implementation
```typescript
// Services
- DragDropService (Angular CDK)
- OutfitBuilderService (state management)

// Events
- onClothingItemDragStart
- onOutfitDrop
- onOutfitItemRemove
- onOutfitSave
```

### Features
- Real-time outfit preview
- Outfit validation (completeness, weather appropriateness)
- Outfit templates
- Outfit history

## 3. Social Validation (Feed) Page

### Page Structure
```
- Header: "Community Feed"
- Filter tabs: All, Following, Trending
- Posts grid with:
  * User avatar and name
  * Outfit images
  * Like/comment/share buttons
  * Timestamp
```

### Feed Features
- Infinite scroll pagination
- Real-time updates
- Post creation modal
- Comment system
- Like/reaction system

### Backend Integration
- Connect to `FeedController` endpoints:
  - GET `/api/feed` - get feed posts
  - POST `/api/feed` - create post
  - POST `/api/feed/{id}/like` - add reaction
  - POST `/api/feed/{id}/comments` - add comment

## 4. Profile Page

### Page Structure
```
- User profile header (avatar, name, stats)
- Tabbed content:
  * My Wardrobe (stats overview)
  * My Outfits (gallery)
  * My Activity (recent wears, interactions)
```

### Features
- Profile editing
- Wardrobe statistics visualization
- Outfit gallery
- Activity timeline

## 5. Search Page

### Page Structure
```
- Search bar with AI suggestions
- Filter chips (category, color, season)
- Results grid with:
  * Clothing items
  * Outfits
  * Users
```

### Search Features
- AI-powered suggestions
- Advanced filters
- Search history
- Saved searches

## Design System Reference

### Colors

| Name | Hex Code | Usage |
|------|----------|-------|
| Background | #FAFAFA | Page background |
| Surface | #FFFFFF | Cards, panels |
| Primary | #F8B4C4 | CTAs, highlights |
| Secondary | #9CAF88 | Success, secondary actions |
| Text Primary | #2D3436 | Headlines, body text |
| Text Secondary | #636E72 | Captions, metadata |
| Border | #DFE6E9 | Dividers, borders |
| Error | #E17055 | Error states |
| Warning | #FDCB6E | Warning states |

### Typography

| Element | Font | Size | Weight |
|---------|------|------|--------|
| H1 | Inter | 32px | 700 |
| H2 | Inter | 24px | 600 |
| H3 | Inter | 18px | 600 |
| Body | Inter | 16px | 400 |
| Caption | Inter | 14px | 400 |
| Button | Inter | 14px | 500 |

### Spacing

| Name | Value |
|------|-------|
| xs | 4px |
| sm | 8px |
| md | 16px |
| lg | 24px |
| xl | 32px |
| xxl | 48px |

### Border Radius

| Element | Radius |
|---------|--------|
| Cards | 12px |
| Buttons | 8px |
| Inputs | 8px |
| Chips | 16px |
| Modals | 16px |

### Shadows

```css
/* Subtle shadow for cards */
box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);

/* Medium shadow for elevated elements */
box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);

/* Strong shadow for modals */
box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
```

## Component Library Reference

### Clothing Item Card

```
A card component displaying a clothing item with:
- Square image thumbnail (1:1 ratio)
- Item name below image
- Color dot indicator (small circle)
- Wear count badge (top-right corner)
- Hover state: slight scale up and shadow increase
- Selected state: pink border
- Dimensions: 180x220px (with text)
```

### Outfit Suggestion Card

```
A card component displaying an outfit suggestion with:
- Vertical stack of clothing item thumbnails (3-4 items)
- Outfit name at top
- Match score badge (e.g., "92% Match")
- Weather icons showing suitability
- "Wear Today" button at bottom
- Dimensions: 200x320px
```

### Weather Widget

```
A compact weather display with:
- Large temperature number
- Weather condition icon
- Location name
- Feels like temperature
- Humidity and wind speed in smaller text
- Background gradient based on condition
- Dimensions: 280x120px
```

### Poll Card

```
A card for social validation polls with:
- Question at top
- 2-4 outfit options side by side
- Vote count for each option
- Time remaining badge
- Total votes counter
- "Vote" button for each option
- Dimensions: Full width, 400px height
```

## Icon Set Reference

Use Heroicons or Phosphor Icons for consistency:

| Action | Icon Name |
|--------|-----------|
| Add Item | plus |
| Upload Photo | camera |
| Delete | trash |
| Edit | pencil |
| Filter | funnel |
| Search | magnifying-glass |
| Weather | cloud-sun |
| Calendar | calendar |
| Settings | cog-6-tooth |
| Notification | bell |
| Heart/Like | heart |
| Share | share |
| Close | x-mark |
| Menu | bars-3 |
| User | user |
| Wardrobe | archive-box |
| Outfit | sparkles |

## Implementation Priorities

1. **Phase 1**: Wardrobe Management Page
   - Basic UI with grid/list toggle
   - Filter sidebar
   - Clothing cards display
   - Basic statistics

2. **Phase 2**: Enhanced Wardrobe Features
   - Drag-and-drop outfit creation
   - Bulk selection actions
   - Advanced analytics
   - AI search suggestions

3. **Phase 3**: Social Integration
   - Feed page implementation
   - Post creation and interaction
   - Poll system integration

4. **Phase 4**: Profile & Search
   - Profile page with stats
   - Global search functionality
   - Advanced filtering

*Document Version: 2.0*
*Last Updated: April 2026*