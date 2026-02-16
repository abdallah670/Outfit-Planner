# Outfit Planner - UI Design Reference

## Homepage Design - AI Image Generation Prompt

For generating visual mockups of the Outfit Planner homepage, use the following detailed prompt with an AI image generator (such as Midjourney, DALL-E, or Stable Diffusion):

---

### Primary Homepage Prompt

```
Create a modern, clean fashion-focused web application homepage for "Outfit Planner - Intelligent Wardrobe Manager". The design should feature:

COLOR PALETTE:
- Light neutral background (off-white #FAFAFA)
- Soft pink accent (#F8B4C4) for primary actions
- Sage green (#9CAF88) for secondary elements
- Charcoal gray (#2D3436) for text

LAYOUT COMPONENTS:

1. NAVIGATION BAR (top):
   - Minimalist navbar with logo on left
   - Menu items centered: My Wardrobe, Plan Outfits, Calendar, Social, Settings
   - User avatar on right with notification bell

2. HERO SECTION (centered):
   - Large elegant title "Intelligent Wardrobe Manager"
   - Subtitle "AI-powered outfit recommendations tailored to your style, weather, and schedule"
   - Primary CTA button "Get Started" in soft pink

3. VIRTUAL CLOSET GRID (main content area):
   - Responsive grid of clothing item cards
   - Category tabs: All, Tops, Bottoms, Shoes, Accessories
   - Each card has: thumbnail image placeholder, item name, color dot indicator, wear count badge
   - Clean white cards with subtle shadows and rounded corners (12px radius)

4. WEATHER-AWARE OUTFIT PANEL (right sidebar):
   - Today's weather display with temperature (22°C), weather icon (partly cloudy), and "Feels like" text
   - Recommended outfit preview showing 3-4 clothing items arranged vertically
   - "Wear This Today" button with sage green background

5. FLOATING ACTION BUTTON:
   - Circular pink button with "+" icon positioned bottom-right
   - For adding new clothing items via photo upload

6. COMMUNITY SECTION (bottom):
   - "Trending Outfits" carousel showing outfit combinations from other users
   - Outfit thumbnail, like count with heart icon, user avatar who created it
   - Horizontal scroll indicator

TYPOGRAPHY:
- Headlines: Modern sans-serif font (Inter or Poppins), semi-bold weight
- Body: Clean sans-serif, regular weight
- Elegant letter spacing for premium feel

DEVICE FRAMES:
- Show desktop view (center, large)
- Show tablet view (right side, medium)
- Show mobile view (left side, small)
- All in a single composition with subtle device bezels

STYLE NOTES:
- Premium, minimalist, fashion-forward aesthetic
- Generous white space
- Subtle drop shadows (0 4px 12px rgba(0,0,0,0.08))
- Smooth rounded corners throughout
- Soft gradients for depth
- Professional UI/UX design quality
```

---

## Additional UI Prompts by Feature

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
```

### Outfit Builder Page

```
Design an outfit builder interface with:
- Central canvas area for assembling outfits (mannequin or flat lay view)
- Left panel with wardrobe items organized by type (scrollable)
- Right panel showing weather data and calendar events
- Bottom panel with AI-generated outfit suggestions (3 cards)
- Color harmony indicator showing how well items match (score out of 100)
- Save outfit button with occasion tags dropdown
- Undo/redo controls
- Color palette: off-white background, soft pink for primary actions, sage green for success states
```

### Social Validation Page

```
Design a social validation interface with:
- Active polls section with countdown timers
- Vote buttons (A, B, C) with outfit preview cards
- Comments section with user avatars and timestamps
- Results visualization with percentage bars
- Share poll link functionality with QR code
- Community feed with trending outfits
- Notification badges for new votes
- Color palette: off-white background, soft pink for votes, sage green for results
```

### Mobile App Views

```
Design mobile app screens for Outfit Planner showing:
- Bottom navigation: Wardrobe, Outfits, Add (+), Social, Profile
- Swipeable outfit suggestion cards (Tinder-style)
- Camera interface for adding clothing items
- Weather-aware daily outfit notification
- Compact wardrobe grid view
- Pull-to-refresh on all lists
- Floating add button on wardrobe screen
- iOS and Android device frames side by side
- Color palette: off-white background, soft pink accents, sage green secondary
```

---

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

---

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
