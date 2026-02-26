### Wardrobe Management Page

```
Design a wardrobe management interface with:
- Large grid/list toggle for clothing items
- Filter sidebar with categories, colors, seasons, occasions
- Bulk selection mode for organizing items
- Drag-and-drop zones for creating outfits
- Statistics cards showing wardrobe analytics (total items, most worn, cost per wear)
- Search bar with AI-powered suggestions
- Color palette: off-white background, soft pink accents, sage green secondary
- Modern minimalist design with rounded corners and subtle shadows
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

---

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

---

*Document Version: 1.0*
*Last Updated: February 2026*
